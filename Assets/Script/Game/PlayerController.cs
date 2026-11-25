using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
	// Start is called before the first frame update
	public float _moveSpeed = 5f;

	[SerializeField] private Rigidbody2D _rb;

	[Header("Camera Shake")]
	[SerializeField] private float _hitShakeDuration = 0.2f;
	[SerializeField] private float _hitShakeMagnitude = 0.15f;

	private Vector2 _moveDir;
	private Vector2 _lastDir = Vector2.right; //캐릭터가 마지막에 바라본 방향
	private IAnimator _spineAnimator;
	private HitFlashEffect _hitFlash;

	//동료가 따라올 위치 (2열)
	[SerializeField] private Transform _companionAnchorA;
	[SerializeField] private Transform _companionAnchorB;

	public Transform CompanionAnchorA => _companionAnchorA;
	public Transform CompanionAnchorB => _companionAnchorB;

	private bool _isDead = false;
	public bool IsDead => _isDead;
	public event Action<bool> OnInteractableChanged;

	private IInteractable _currentInteractable;

	private void Awake()
	{
		_spineAnimator = GetComponent<IAnimator>();
		_hitFlash = GetComponent<HitFlashEffect>();
	}

	void Update()
	{
		//움직임이 잠겨있으면 입력 불가
		if (GameManager.Instance != null && GameManager.Instance.IsInputLock)
		{
			_moveDir = Vector2.zero;
			_spineAnimator.PlayAnimation("idle");

			//다이얼로그 진행, 타이핑
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
			{
				if(UIManager.Instance.IsDialogueOpen())
				{ 
					UIManager.Instance.AdvanceDialogue();
				}
			}
			return;
		}

		// 키보드 입력 받기
		HandleInput();
		UpdateDirection();
		string anim = _moveDir.magnitude > 0.1f ? "run" : "idle";
		_spineAnimator.PlayAnimation(anim);
	}

	private void FixedUpdate()
	{
		Move();
	}

	private void Move()
	{
		// 캐릭터 이동
		if(_rb == null)
			return;

		_rb.MovePosition(_rb.position + _moveDir * _moveSpeed * Time.fixedDeltaTime);
	}

	private void HandleInput()
	{

		if (_isJoystickActive)
		{
			_moveDir = _joystickInput;
		}
		else
		{
			// 키보드 입력 받기
			float horizontal = Input.GetAxisRaw("Horizontal"); // A, D 또는 ←, →
			float vertical = Input.GetAxisRaw("Vertical");   // W, S 또는 ↑, ↓

			//대각선 이동 제어
			if (vertical != 0)
			{
				_moveDir = new Vector2(0, vertical).normalized;
			}
			else if (horizontal != 0)
			{
				_moveDir = new Vector2(horizontal, 0).normalized;
			}
			else
			{
				_moveDir = Vector2.zero;
			}
		}

		if (_moveDir.magnitude > 0.1f) //일정 거리 이상 움직였다면
		{
			_lastDir = _moveDir; //마지막 방향 갱신
		}

		if (Input.GetKeyDown(KeyCode.I))
		{
			//인벤토리 토글
			UIManager.Instance.ToggleInventory();
		}

		if(Input.GetKeyDown(KeyCode.Q))
		{
			//퀘스트 UI 토글
			UIManager.Instance.ToggleQuest();
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			TryInteract();
		}
	}

	private Vector2 _joystickInput = Vector2.zero;
	private bool _isJoystickActive = false;
	public void SetJoystickInput(Vector2 direction)
	{
		_joystickInput = direction;
		_isJoystickActive = direction.magnitude > 0.1f;
	}


	private void UpdateDirection()
	{
		var facing = _moveDir != Vector2.zero ? _moveDir : _lastDir;

		if(_spineAnimator != null)
		{
			_spineAnimator.SetFacing(facing);
		}

		
		bool isRight = facing.x > 0;
		CompanionManager.Instance.SetFacingDirection(isRight);
	}

	/// <summary>
	/// AutoAttack에서 공격 시 호출. _lastDir까지 갱신해야 다음 프레임 UpdateDirection()이
	/// 이동 입력 없을 때 이 방향을 유지함 (안 그러면 바로 덮어써져서 타겟을 안 보고 공격하는 것처럼 보임)
	/// </summary>
	public void FaceDirection(Vector2 direction)
	{
		if (direction == Vector2.zero) return;

		_lastDir = direction;

		if (_spineAnimator != null)
			_spineAnimator.SetFacing(direction);

		bool isRight = direction.x > 0;
		CompanionManager.Instance.SetFacingDirection(isRight);
	}

	public void SetPosition(Transform pos)
	{
		transform.position = pos.position;
	}

	// 위치는 SetPosition으로 분리 — 스폰 포인트가 없어도 맵별 스케일/속도는 적용되도록
	public void SetMapCondition(Vector3 scale, float speed)
	{
		transform.localScale = scale;
		_moveSpeed = speed;
	}

	public Vector3 GetLastDirection()
	{
		if(_moveDir.magnitude > 0.1f)
		{
			return new Vector3 (_moveDir.x, _moveDir.y, 0f);
		}
		else
		{
			return new Vector3(_lastDir.x, _lastDir.y, 0f);
		}
	}

	//인터페이스 구현
	public void TakeDamage(int damage)
	{
		if(_isDead) return;

		PlayerInfoManager.Instance.TakeDamage(damage);
		_hitFlash?.Flash();
		_spineAnimator.PlayAnimation("hit", false);
		GameManager.Instance?.CameraShake(_hitShakeDuration, _hitShakeMagnitude);

		if(PlayerInfoManager.Instance.IsDead)
		{
			_isDead = true;
			StartCoroutine(DieAndRespawn());
		}
	}

	private IEnumerator DieAndRespawn()
	{
		//입력 잠금
		GameManager.Instance.SetLockInput(true);

		//페이드 인아웃
		bool fadeDone = false;

		UIManager.Instance.RequestFadeTransition(0.5f, () =>
		{
			//hp 회복 + 스폰 포인트로 이동
			PlayerInfoManager.Instance.RestoreHp();
			Transform spawnPoint = GameManager.Instance.CurrentMapController.GetSpawnPoint(SpawnPointId.Default);
			if (spawnPoint != null)
				SetPosition(spawnPoint);
			else
				Debug.LogError("[PlayerController] Default 스폰 포인트가 없어 사망 위치에서 부활합니다.");
			_isDead = false;

			GameManager.Instance.SnapCamera();
		},
		() => {
			fadeDone = true;
			}
		);

		yield return new WaitUntil(() => fadeDone);

		//입력 잠금 해제
		GameManager.Instance.SetLockInput(false);
	}

	public void SetInteractable(IInteractable target)
	{
		_currentInteractable = target;
		OnInteractableChanged?.Invoke(true);
	}
	public void ClearInteractable()
	{
		_currentInteractable = null;
		OnInteractableChanged?.Invoke(false);
	}

	public void TryInteract()
	{
		if (GameManager.Instance.IsInputLock)
		{
			return;
		}
		if (_currentInteractable != null)
		{
			_currentInteractable.Interact(this);
			//인터렉티브 상대가 있을때는 프롬프트를 띄우지 않음
			UIManager.Instance?.HidePrompt();
		}
	}
}


using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
	// Start is called before the first frame update
	public float _moveSpeed = 5f;

	[SerializeField] private Rigidbody2D _rb;

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

		if (_isjoystickActive)
		{
			_moveDir = _joysticInput;
			Debug.Log($"[PlayerContorller] HandleInput - moveDir: {_moveDir}");
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

		//test
		// 키보드 T 누르면 아이템 추가 테스트
		if (Input.GetKeyDown(KeyCode.T))
		{
			InventoryManager.Instance.AddItem("weapon_sword_01");
			InventoryManager.Instance.AddItem("potion_hp_small", 3);
			Debug.Log($"장비 수: {InventoryManager.Instance.GetItemsByType(ItemType.Equipment).Count}");
			Debug.Log($"소비 수: {InventoryManager.Instance.GetItemsByType(ItemType.Consumable).Count}");
		}

		// 키보드 Y 누르면 장착 테스트
		if (Input.GetKeyDown(KeyCode.Y))
		{
			InventoryManager.Instance.EquipWeapon("weapon_sword_01");
			var weapon = InventoryManager.Instance.GetEquippedWeaponData();
			Debug.Log($"장착 무기: {weapon?._itemName}, 공격력 +{weapon?._effectValue}");
		}

		// 키보드 U 누르면 사용 테스트
		if (Input.GetKeyDown(KeyCode.U))
		{
			InventoryManager.Instance.UseItem("potion_hp_small");
			Debug.Log($"남은 포션: {InventoryManager.Instance.GetItemsByType(ItemType.Consumable)[0].count}");
		}
	}

	private Vector2 _joysticInput = Vector2.zero;
	private bool _isjoystickActive = false;
	public void SetJoystickInput(Vector2 direction)
	{
		_joysticInput = direction;
		_isjoystickActive = direction.magnitude > 0.1f;
		Debug.Log($"[PlayerController] SetJoystickInput - input: {_joysticInput}, isActive: {_isjoystickActive}");
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

	public void SetPosition(Transform pos)
	{
		transform.position = pos.position;
	}

	public void SetMapCondition(Transform pos, Vector3 scale, float speed)
	{
		SetPosition(pos);
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
			SetPosition(spawnPoint);
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
			UIManager.Instance?.HideNPCPrompt();
		}
	}
}


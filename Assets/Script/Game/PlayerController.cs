using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	// Start is called before the first frame update
	public float _moveSpeed = 5f;

	[SerializeField]
	private SkeletonAnimation _skel;
	[SerializeField]
	private Rigidbody2D _rb;

	private Vector2 _moveDir;
	private Vector2 _lastDir = Vector2.right; //캐릭터가 마지막에 바라본 방향

	//동료가 따라올 위치 (2열)
	public Transform _companionAnchorA; 
	public Transform _companionAnchorB;
	private void Awake()
	{
	}

	void Update()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsInputLock)
		{
			_moveDir = Vector2.zero;
			PlaySkeletonAnimation(); // idle 애니메이션 유지
			return;
		}

		// 키보드 입력 받기
		HandleInput();
		UpdateDirection();
		PlaySkeletonAnimation();

		if (CompanionManager.Instance != null)
		{
			//동료 위치 갱신
			CompanionManager.Instance.RefreshFollowPosition();
		}
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

		if(_moveDir.magnitude > 0.1f) //일정 거리 이상 움직였다면
		{
			_lastDir = _moveDir; //마지막 방향 갱신
		}

		if(Input.GetKeyDown(KeyCode.I))
		{
			//인벤토리 토글
			UIManager.Instance.ToggleInventory();
		}
	}

	private void PlaySkeletonAnimation()
	{
		if (_skel == null)
			return;

		string aniName = GetMoveAnimation();
		if (_skel.AnimationName == aniName)
			return; //이미 재생 중인 애니메이션이면 무시

		_skel.AnimationState.SetAnimation(0, aniName, true);

	}

	string GetMoveAnimation()
	{
		if (_moveDir.magnitude > 0.1f)
		{
			// 이동 중일 때
			return "run_1";
		}
		else
		{
			// 정지 상태일 때
			return "idle";
		}
	}

	private void UpdateDirection()
	{
		if(_skel == null)
			return;

		var facing = _moveDir != Vector2.zero ? _moveDir : _lastDir;

		if (facing.x < 0)
		{
			_skel.skeleton.ScaleX = 1f; //왼쪽 바라보기
		}
		else if (facing.x > 0)
		{
			_skel.skeleton.ScaleX = -1f; //오른쪽 바라보기
		}

		bool isRight = facing.x > 0;
		MirrorFollowPos(isRight);
		CompanionManager.Instance.SetFacingDirection(isRight);
	}

	public void SetPosition(Transform pos)
	{
		transform.position = pos.position;
	}

	private void MirrorFollowPos(bool right)
	{
		float sign = right ? -1f : 1f;

		_companionAnchorA.localPosition = new Vector3(Mathf.Abs(_companionAnchorA.localPosition.x) * sign, _companionAnchorA.localPosition.y, _companionAnchorA.localPosition.z);
		_companionAnchorB.localPosition = new Vector3(Mathf.Abs(_companionAnchorB.localPosition.x) * sign, _companionAnchorB.localPosition.y, _companionAnchorB.localPosition.z);
	}
}


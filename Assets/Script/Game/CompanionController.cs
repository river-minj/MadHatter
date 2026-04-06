using UnityEngine;

public class CompanionController : MonoBehaviour
{
	[Header("Basic Info")]
	[SerializeField] private CompanionData _companionData;
    [SerializeField] private float _moveSpeed = 3.0f; // 동료의 이동 속도

	[Header("Follow Setting")]
	public int _followIndex;
	[SerializeField] private int _stepPerFollower = 6; // 한 동료가 몇 스텝 뒤를 따를지 (트레일 인덱스 간격)
	
	private IAnimator _spineAnimator;
	private PlayerController _player;
    private Vector3 _targetPos;

	//동료 줄에서 자기 위치 정보
	private bool _isLineA = true; // A라인인지 B라인인지
	private int _indexInLine = 0; // 같은 라인에서 몇 번째 동료인지

	//A,B라인의 횡방향 오프셋 (플레이어 앵커 기준)
	private Vector3 _lateralOffsetA;
	private Vector3 _lateralOffsetB;


	public void Initialize(PlayerController player, CompanionData data, int followIndex, bool isLineA, int lineIndex)
	{
		_followIndex = followIndex;
		_player = player;
		_companionData = data;
		_isLineA = isLineA;
		_indexInLine = lineIndex;

		_spineAnimator = GetComponent<IAnimator>();

		var skelAnim = _spineAnimator as SpineAnimator;
		var skel = skelAnim?.Skeleton;
		if (skel != null)
		{
			skel.initialSkinName = _companionData._skinName;
			skel.Initialize(true);
		}

		if(_player.CompanionAnchorA != null)
		{
			_lateralOffsetA = _player.CompanionAnchorA.localPosition;
		}
		if(_player.CompanionAnchorB != null)
		{
			_lateralOffsetB = _player.CompanionAnchorB.localPosition;
		}

		//_moveSpeed = data._followSpeed;
	}


	private void Update()
	{
		var recorder = PlayerTrailRecorder.Instance;
		if (recorder == null)
			return;

		if (_player == null)
			return;

		//목표 위치 계산
		CalculateTargetPosition(recorder);
		MoveToTarget();

		// 부드럽게 이동
		transform.position = Vector3.MoveTowards(
			transform.position,
			_targetPos,
			_moveSpeed * Time.deltaTime
		);

		// 방향 전환
		//Vector3 moveDir = (_targetPos - transform.position).normalized;
		//UpdateFacingDirection(moveDir);

	}

	public void SetFacingDirection(bool isRight)
	{
		if (_spineAnimator == null) return;
		Vector2 dir = isRight ? Vector2.right : Vector2.left;
		_spineAnimator.SetFacing(dir);
	}

	private void CalculateTargetPosition(PlayerTrailRecorder recorder)
	{
		if(recorder == null)
		{
			_targetPos = transform.position;
			return;
		}

		// 1. 이 동료가 참조할 Trail 인덱스 계산
		int stepsBack = _stepPerFollower * (_indexInLine + 1);
		int trailIndex = recorder.Count - 1 - stepsBack;
		trailIndex = Mathf.Clamp(trailIndex, 0, recorder.Count - 1);

		// 2. 그 시점에 Trail 상의 위치 + 방향
		TrailPoint point = recorder.GetPoint(trailIndex);

		// 3.2의 방향을 기준으로 오프셋 계산
		Vector3 offset = CalculateOffset(point.direction);

		_targetPos = point.position + offset;
	
	}

	private Vector3 CalculateOffset(Vector3 direction)
	{
		float lateral = _isLineA ? _lateralOffsetA.x : _lateralOffsetB.x;

		// 상하 이동
		if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
		{
			if (direction.y > 0)
			{
				// 상 이동 → 동료는 아래쪽 (y-), A/B는 좌우(x)로 벌어짐
				return new Vector3(lateral, -Mathf.Abs(_lateralOffsetA.y), 0f);
			}
			else
			{
				// 하 이동 → 동료는 위쪽 (y+), A/B는 좌우(x)로 벌어짐
				return new Vector3(lateral, Mathf.Abs(_lateralOffsetA.y), 0f);
			}
		}
		// 좌우 이동
		else
		{
			float verticalLateral = _isLineA
				? Mathf.Abs(_lateralOffsetA.y)
				: -Mathf.Abs(_lateralOffsetB.y);

			if (direction.x > 0)
			{
				// 우 이동 → 동료는 왼쪽 (x-), A/B는 상하(y)로 벌어짐
				return new Vector3(-Mathf.Abs(_lateralOffsetA.x), verticalLateral, 0f);
			}
			else
			{
				// 좌 이동 → 동료는 오른쪽 (x+), A/B는 상하(y)로 벌어짐
				return new Vector3(Mathf.Abs(_lateralOffsetA.x), verticalLateral, 0f);
			}
		}
	}
	private void MoveToTarget()
	{
		// MoveTowards는 직선 보간이므로 대각선 이동이 발생할 수 있음
		// 4방향 제한 이동: 먼저 x축을 맞추고, 그다음 y축을 맞추는 방식
		Vector3 current = transform.position;
		float step = _moveSpeed * Time.deltaTime;

		Vector3 newPos = MoveCardinalStep(current, _targetPos, step);
		transform.position = newPos;
	}

	private Vector3 MoveCardinalStep(Vector3 current, Vector3 target, float step)
	{
		float dx = target.x - current.x;
		float dy = target.y - current.y;

		// 이미 도착
		if (Mathf.Abs(dx) < 0.01f && Mathf.Abs(dy) < 0.01f)
			return target;

		// 차이가 큰 축부터 이동 (자연스러운 따라가기)
		if (Mathf.Abs(dx) > Mathf.Abs(dy))
		{
			// x축 이동
			float moveX = Mathf.MoveTowards(current.x, target.x, step);
			return new Vector3(moveX, current.y, current.z);
		}
		else
		{
			// y축 이동
			float moveY = Mathf.MoveTowards(current.y, target.y, step);
			return new Vector3(current.x, moveY, current.z);
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		// 디버깅용: 목표 위치 표시
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(_targetPos, 0.2f);


		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, _targetPos);

	}
#endif
}

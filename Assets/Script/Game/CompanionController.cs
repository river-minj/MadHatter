using JetBrains.Annotations;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
	[Header("Basic Info")]
	[SerializeField] private CompanionData _companionData;
	[SerializeField] private SkeletonAnimation _skel;
    [SerializeField] private float _moveSpeed = 3.0f; // 동료의 이동 속도

	[Header("Follow Setting")]
	public int _followIndex;
	[SerializeField] private int _stepPerFollower = 6; // 한 동료가 몇 스텝 뒤를 따를지 (트레일 인덱스 간격)

	private PlayerController _player;
    private Vector3 _targetPos;
	private Queue<Vector3> _trailQue;

	//동료 줄에서 자기 위치 정보
	private bool _isLineA = true; // A라인인지 B라인인지
	private int _indexInLine = 0; // 같은 라인에서 몇 번째 동료인지

	private void Start()
	{
		_trailQue = PlayerTrailRecorder.Instance?.TrailQue;
	}

	public void SetData(PlayerController player, CompanionData data, int followIndex)
	{
		_followIndex = followIndex;
		_player = player;
		_companionData = data;
		if (_skel != null)
		{
			_skel.initialSkinName = _companionData._skinName;
			_skel.Initialize(true);
		}
		//_moveSpeed = data._followSpeed;
	}

	public void SetLineInfo(bool isLineA, int lineIndex)
	{
		_isLineA = isLineA;
		_indexInLine = lineIndex;
	}

	private void Update()
	{
		if (_trailQue == null)
			return;

		if (_player == null)
			return;

		//목표 위치 계산
		CalculateTargetPosition(_trailQue.ToArray());

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
		if (_skel == null)
		{
			return;
		}

		float absScale = MathF.Abs(_skel.skeleton.ScaleX);
		_skel.skeleton.ScaleX = isRight ? -absScale : absScale;
	}

	private void CalculateTargetPosition(Vector3[] trailArray)
	{
		// 1. 이 동료가 참조할 Trail 인덱스 계산
		int stepsBack = _stepPerFollower * (_indexInLine + 1);
		int trailIndex = trailArray.Length - 1 - stepsBack;
		trailIndex = Mathf.Clamp(trailIndex, 0, trailArray.Length - 1);

		// 2. Trail 상의 기준 위치
		Vector3 trailPosition = trailArray[trailIndex];

		// 3.플레이어의 진행 방향
		Vector3 playerMoveDir = _player.GetLastDirection();

		//4. 이동 방향에 따라 좌우 오프셋 적용
		Vector3 offset = GetOffsetByDirection(playerMoveDir);

		_targetPos = trailPosition + offset;
	
	}

	private Vector3 GetOffsetByDirection(Vector3 playerMoveDir)
	{
		float horizontalOffset = _isLineA ? _player.CompanionAnchorA.localPosition.x : _player.CompanionAnchorB.localPosition.x;

		//이동 방향에 따른 오프셋 결정
		if (Mathf.Abs(playerMoveDir.y) > Mathf.Abs(playerMoveDir.x))
		{
			//상하 이동중
			if(playerMoveDir.y > 0)
			{
				Debug.Log($"상 이동 : 동료들은 아래쪽 | horizontalOffset : {horizontalOffset} , offset : {Mathf.Abs(_player.CompanionAnchorA.localPosition.y)}");

				return new Vector3(horizontalOffset, Mathf.Abs(_player.CompanionAnchorA.localPosition.y), 0f);
			}
			else
			{
				Debug.Log($"하 이동 : 동료들은 위쪽 | horizontalOffset : {horizontalOffset} , offset : {-Mathf.Abs(_player.CompanionAnchorB.localPosition.y)}" );

				return new Vector3(horizontalOffset, -Mathf.Abs(_player.CompanionAnchorB.localPosition.y), 0f);
			}

		}
		else
		{
			//좌우 이동중
			if (playerMoveDir.x > 0)
			{
				// 우 이동: 동료들은 좌측 (두 줄이상하로 벌어짐)
				return new Vector3(-0.5f, horizontalOffset, 0);
			}
			else
			{
				// 좌 이동: 동료들은 우측 (두 줄이 상하로 벌어짐)
				return new Vector3(0.5f, horizontalOffset, 0);
			}
		}
	}

	private void UpdateFacingDirection(Vector3 moveDir)
	{
		if (_skel == null)
			return;
		bool isMovingRight = moveDir.x > 0;
		float absScale = Mathf.Abs(_skel.skeleton.ScaleX);
		_skel.skeleton.ScaleX = isMovingRight ? -absScale : absScale;
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

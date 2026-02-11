using JetBrains.Annotations;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
	[SerializeField] private CompanionData _companionData;
	[SerializeField] private SkeletonAnimation _skel;

	[SerializeField] private float _moveSmooth = 0.12f; // 동료의 이동 부드러움
    [SerializeField] private float _moveSpeed = 3.0f; // 동료의 이동 속도

	public int _followIndex;
	[SerializeField] private int _followDistance = 6;

	private PlayerController _player;
    private Vector3 _targetPos;
	private Queue<Vector3> _trailQue;

	private CompanionController _frontCompanion;

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
		}
		//_moveSpeed = data._followSpeed;
	}

	public void SetFrontCompanion(CompanionController frontCom)
	{
		_frontCompanion = frontCom; 
	}

	private void Update()
	{
		if (_trailQue == null)
			return;

		if (_trailQue.Count < 2)
			return;

		if (_player == null)
			return;


		//목표 위치 계산
		CalculateTargetPosition();

		// 부드럽게 이동
		transform.position = Vector3.Lerp(
			transform.position,
			_targetPos,
			_moveSpeed * Time.deltaTime
		);

		// 방향 전환
		Vector3 moveDir = (_targetPos - transform.position).normalized;
		UpdateFacingDirection(moveDir);


		////quere를 배열로 변환하여 인덱스로 접근
		//Vector3[] trailArray = _trailQue.ToArray();
		//if (trailArray.Length < 2)
		//	return;
		
		////A라인인지 B라인인지
		//bool isLineA = (_followIndex % 2 == 0);
		//Transform anchor = isLineA ? _player.CompanionAnchorA : _player.CompanionAnchorB;
		
		////같은 라인에서 몇번째 동료인지 계산
		//int positionInLine = _followIndex / 2;

		////TrailQue에서 뒤로 몇 칸 이동해야 하는지 계산 (각 줄마다 일정 간격)
		//int stepBack = _followDistance * positionInLine;
		
		////해당 동료가 따라가야 할 trail index
		//int trailIndex = trailArray.Length - 1 - stepBack;
		//trailIndex = Mathf.Clamp(trailIndex, 1, trailArray.Length - 1);

		////Trail 상의 기준 위치와 이전 위치
		//Vector3 basePosition = trailArray[trailIndex];
		//Vector3 prevPosition = trailArray[trailIndex - 1];

		////경로 진행 방향, 오른쪽, 뒤쪽 벡터 계산
		//Vector3 moveDir = (basePosition-prevPosition).normalized;
		//if(moveDir == Vector3.zero)
		//{
		//	moveDir = Vector3.right;
		//}

		////진행 방향의 오른쪽 (2D 기준 90도 회전)
		//Vector3 right = new Vector3(-moveDir.y, moveDir.x, 0f); //오른쪽
		////Vector3 back = -moveDir; //뒤

		////anchor의 로컬 오프셋을 월드 좌표로 적용
		////x: 좌우 오프셋 (right 뱡향)
		////y: 앞뒤 오프셋 (moveDir 방향, 음수면 뒤쪽)
		//Vector3 localAnchor = anchor.localPosition;
		//Vector3 side = localAnchor.x * right; //좌우
		//Vector3 head = localAnchor.y * moveDir; //앞뒤


		////최종 타겟 위치 (trail 위치 + 오프셋)
		//_targetPos = basePosition + side + head;

		//// 타겟 포지션으로 부드럽게 이동
		//transform.position = Vector3.MoveTowards(transform.position, _targetPos,  Time.deltaTime * _moveSpeed);

		////방향 전환 (필요시 애니메이션 추가 가능)
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

	private void CalculateTargetPosition()
	{
		// A라인인지 B라인인지는 Manager의 리스트로 판단됨
		// 여기서는 _frontCompanion 유무로 판단

		if (_frontCompanion == null)
		{
			// 첫 번째 동료: Anchor 위치 결정 필요
			// Manager에서 설정한 인덱스가 짝수면 A, 홀수면 B
			bool isLineA = (_followIndex % 2 == 0);
			Transform anchorTransform = isLineA ? _player.CompanionAnchorA : _player.CompanionAnchorB;
			_targetPos = anchorTransform.position;
		}
		else
		{
			// 두 번째 이후 동료: 앞 동료 뒤를 따라가기
			Vector3 frontPos = _frontCompanion.transform.position;
			Vector3 myPos = transform.position;

			// 앞 동료의 이동 방향 계산
			Vector3 frontMoveDir = (_frontCompanion._targetPos - frontPos).normalized;

			if (frontMoveDir.magnitude < 0.01f)
			{
				// 앞 동료가 정지 중이면 현재 방향 유지
				frontMoveDir = (frontPos - myPos).normalized;
				if (frontMoveDir.magnitude < 0.01f)
					frontMoveDir = Vector3.down; // 기본 방향
			}

			// 앞 동료로부터 일정 거리 뒤
			_targetPos = frontPos - frontMoveDir * _followDistance;
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

		if (_frontCompanion != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position, _frontCompanion.transform.position);
		}
	}
#endif
}

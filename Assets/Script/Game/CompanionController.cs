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
	[SerializeField] private int _stepPerFollower = 6;

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

		if (_trailQue.Count < 2)
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
		trailIndex = Mathf.Clamp(trailIndex, 1, trailArray.Length - 1);

		// 2. Trail 상의 기준 위치
		Vector3 basePos = trailArray[trailIndex];
		Vector3 prevPos = trailArray[trailIndex - 1];

		// 3. 진행 방향
		Vector3 moveDirection = (basePos - prevPos).normalized;
		if (moveDirection.magnitude < 0.01f)
			moveDirection = Vector3.right;

		// 4. 진행 방향의 오른쪽 벡터 (2D)
		Vector3 rightVector = new Vector3(-moveDirection.y, moveDirection.x, 0f);

		// 5. Anchor 오프셋 가져오기
		Transform anchor = _isLineA ? _player.CompanionAnchorA : _player.CompanionAnchorB;
		Vector3 anchorLocal = anchor.localPosition;

		// 6. 최종 위치 = Trail 위치 + 좌우 오프셋 + 앞뒤 오프셋
		Vector3 lateralOffset = rightVector * anchorLocal.x; // 좌우
		Vector3 longitudinalOffset = moveDirection * anchorLocal.y; // 앞뒤

		_targetPos = basePos + lateralOffset + longitudinalOffset;
	
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

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
	public int _followOffset = 10; // trail 에서 얼마나 뒤를 따라갈지
	public int _stepPerRow = 6;

    private Vector3 _targetPos;

	private Queue<Vector3> _trail;

	private void Start()
	{
		_trail = PlayerTrailRecorder.Instance?.TrailQue;
	}

	public void Init(CompanionData data)
	{
		_companionData = data;
		_moveSpeed = data._followSpeed;
	}

	public void SetFollowPosition(Vector3 pos)
    {
		_targetPos = pos;
	}

	internal void SetFacingDirection(bool isRight)
	{
		if(_skel == null)
		{
			return;
		}

		var scale = _skel.skeleton.ScaleX;
		float avsScale = MathF.Abs(scale);

		_skel.skeleton.ScaleX = isRight ? -avsScale : avsScale;
	}

	private void Update()
	{
		if (_trail == null)
			return;

		if (_trail.Count < 2)
			return;

		//해당 동료가 따라가야 할 trail index
		Vector3[] arr = _trail.ToArray();
		int row = _followIndex / 2; //몇번째 줄에 서는 동료인지
		int baseOffset = _stepPerRow *(row + 1); //뒤로 떨어질 정도
		int index = arr.Length - _followOffset - 1;

		int trailIndex = arr.Length - 1 - baseOffset;
		trailIndex = Mathf.Clamp(trailIndex, 1, arr.Length - 1);

		Vector3 pos = arr[trailIndex];
		Vector3 prev = arr[trailIndex - 1];

		//경로 진행 방향, 오른쪽, 뒤쪽 벡터 계산
		Vector3 dir = (pos-prev).normalized;
		if(dir == Vector3.zero)
		{
			dir = Vector3.right;
		}

		Vector3 right = new Vector3(-dir.y, dir.x, 0f); //오른쪽
		Vector3 back = -dir; //뒤

		//해당 동료의 라인 구분 A or B
		bool isA = (_followIndex % 2 == 0);
		// --------- 여기서 부터 다시
		

		if (index >= 0 && index < arr.Length)
		{
			_targetPos = arr[index];
		}



		// 타겟 포기션으로 부드럽게 이동
		transform.position = Vector3.MoveTowards(transform.position, _targetPos,  Time.deltaTime * _moveSpeed);
		
		//방향 전환 (필요시 애니메이션 추가 가능)
	}
}

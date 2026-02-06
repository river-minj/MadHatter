using Spine.Unity;
using System;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
	[SerializeField] private CompanionData _companionData;
	[SerializeField] private SkeletonAnimation _skel;

	[SerializeField] private float _moveSmooth = 0.12f; // 동료의 이동 부드러움
    [SerializeField] private float _moveSpeed = 3.0f; // 동료의 이동 속도

    private Vector3 _targetPos;
	private Vector3 _velocity = Vector3.zero;

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
		// 타겟 포기션으로 부드럽게 이동
		transform.position = Vector3.SmoothDamp(transform.position, _targetPos, ref _velocity, _moveSmooth, _moveSpeed);
		
		//방향 전환 (필요시 애니메이션 추가 가능)
	}
}

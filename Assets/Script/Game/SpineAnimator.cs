using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationNameMapping
{
	public string starndardName;
	public string actualName;

}
public class SpineAnimator : MonoBehaviour
{
	[SerializeField] private SkeletonAnimation _skel;
	[SerializeField] private List<AnimationNameMapping> _animationMapping = new();
	private Dictionary<string, string> _animMap;

	public SkeletonAnimation Skeleton => _skel;

    private string _currentLoopAnim;

	//anim 이름 dic 초기화
	private void Awake()
	{
		_animMap = new Dictionary<string, string>();
		foreach(var mapping in _animationMapping)
		{
			_animMap[mapping.starndardName] = mapping.actualName;
		}
	}

	private void OnEnable()
	{
		if(_skel != null) {
			_skel.AnimationState.Complete += OnAnimationComplete;
		}
	}

	private void OnDisable()
	{
		if (_skel != null)
		{
			_skel.AnimationState.Complete -= OnAnimationComplete;
		}
	}

	public void DisableAutoIdle()
	{
		if (_skel != null)
		{
			_skel.AnimationState.Complete -= OnAnimationComplete;
		}
	}

	/// <summary>
	/// Spine 애니메이션 재생
	/// loop=true: 같은 애니메이션 중복 재생 방지
	/// loop=false: 항상 재생 (attack, hit, die 등)
	/// </summary>
	public void PlayAnimation(string animName, bool loop = true)
    {
		string resolved = ResolveAnimationName(animName);
		if (resolved == null)
			return;

		if (loop)
		{
			if (_currentLoopAnim == resolved)
				return;
			
			_currentLoopAnim = resolved;
		}

		_skel.AnimationState.SetAnimation(0, resolved, loop);
	}


	private string ResolveAnimationName(string standardName)
	{
		if (_animMap == null || _animMap.Count == 0)
			return standardName;

		if (_animMap.TryGetValue(standardName, out string actualName))
			return actualName;

		return null;
	}
	/// <summary>
	/// Spine ScaleX로 좌우 반전. direction.x 기준.
	/// </summary>
	public void SetFacing(Vector2 direction)
	{
		if (direction.x == 0f)
			return;
	
		_skel.skeleton.ScaleX = direction.x > 0f ? 1f : -1f;
	}

	private void OnAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Loop == false)
		{
			PlayAnimation("idle");
		}
	}
}

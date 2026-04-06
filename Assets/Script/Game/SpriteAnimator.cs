using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour, IAnimator
{
	[SerializeField] private Animator _animator;
	[SerializeField] private SpriteRenderer _spriteRenderer;
	[SerializeField] private List<AnimationNameMapping> _animationMappings = new List<AnimationNameMapping>();

	private Dictionary<string, string> _dicAnim;
	private string _currentAnim;

	private void Awake()
	{
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}

		if (_spriteRenderer == null)
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}

		_dicAnim = new Dictionary<string, string>();
		foreach (var mapping in _animationMappings)
		{
			if (!string.IsNullOrEmpty(mapping.starndardName) && !string.IsNullOrEmpty(mapping.actualName))
			{
				_dicAnim[mapping.starndardName] = mapping.actualName;
			}
		}
	}

	public void PlayAnimation(string animName, bool loop = true)
	{
		string resolved = ResolveAnimName(animName);
		if (resolved == null)
			return;
		if (_currentAnim == resolved)
			return; //같은 애니메이션 중복 재생 방지

		_currentAnim = resolved;
		_animator.Play(resolved);
	}

	public void SetFacing(Vector2 direction)
	{
		if (direction.x == 0f)
		{
			return;
		}

		_spriteRenderer.flipX = direction.x > 0f;
	}

	public void DisableAutoIdle()
	{
		// Sprite 애니메이터는 자동으로 idle로 돌아가는 기능이 없으므로 구현할 필요 없음
	}

	private string ResolveAnimName(string standardName)
	{
		if(_dicAnim == null || _dicAnim.Count == 0)
		{
			Debug.LogWarning("Animation mapping dictionary is not initialized.");
			return standardName;
		}

		if(_dicAnim.TryGetValue(standardName, out string actualName))
		{
			return actualName;
		}

		return null;
	}

}


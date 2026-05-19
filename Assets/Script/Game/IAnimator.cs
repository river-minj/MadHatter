using System;
using UnityEngine;

public interface IAnimator
{
	void PlayAnimation(string name, bool loop = true, Action onComplete = null);
	void SetFacing(Vector2 direction);
	void DisableAutoIdle();
}

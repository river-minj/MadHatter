using UnityEngine;

public interface IAnimator
{
	void PlayAnimation(string name, bool loop = true);
	void SetFacing(Vector2 direction);
	void DisableAutoIdle();
}

using UnityEngine;

public class CompanionController : MonoBehaviour
{
    [SerializeField] private float followDistance = 2.0f; // 동료가 플레이어를 따라가는 거리
    [SerializeField] private float moveSpeed = 3.0f; // 동료의 이동 속도

    private Vector3 _followTarget;

    public void SetFollowTarget(Vector3 target)
    {
		_followTarget = target;
	}

	private void LateUpdate()
	{
		if(_followTarget == null)
		{
			return;
		}

		transform.position = Vector3.MoveTowards(transform.position, _followTarget, moveSpeed * Time.deltaTime);

	}
}

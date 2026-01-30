using UnityEngine;

public class CompanionController : MonoBehaviour
{
	[SerializeField] private CompanionData _companionData;
    [SerializeField] private float _followDistance = 2.0f; // 동료가 플레이어를 따라가는 거리
    [SerializeField] private float _moveSpeed = 3.0f; // 동료의 이동 속도

    private Vector3 _followPos;

    public void SetFollowPosition(Vector3 pos)
    {
		_followPos = pos;
	}

	private void LateUpdate()
	{
		if(_followPos == null)
		{
			return;
		}

		if (_followPos == Vector3.zero)
			return;

		float followSpeed = _companionData != null ? _companionData._followSpeed : _moveSpeed;


		transform.position = Vector3.MoveTowards(transform.position, _followPos, followSpeed * Time.deltaTime);

	}
}

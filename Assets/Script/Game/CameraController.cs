
using UnityEngine;

/// <summary>
/// 1.tracing target
/// 2.map boundary 
/// 3.smooth movement
/// </summary>
public class CameraController : MonoBehaviour
{
    //추적
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

    private Camera _cam;
    private Bounds? _currentBounds;
    private Vector3 _uiOffset = Vector3.zero;

	private void Awake()
	{
        _cam = Camera.main;
	}

	void Start()
    {
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
            else
            {
                Debug.LogError("Player object with tag 'Player' not found in the scene.");
            }

        }
    }

    void LateUpdate()
    {
        if (_target == null)
            return;

        //목표 위치
        Vector3 targetPosition = _target.position + _offset;

		//카메라 경계 처리 및 맵이 카메라보다 작은 경우 중심 고정
		if (_currentBounds.HasValue)
		{
			targetPosition = ClampPositionToBounds(targetPosition, _currentBounds.Value);
		}

		// UI 오프셋은 경계 클램핑 이후 적용 (맵 경계에 막히지 않도록)
		targetPosition += _uiOffset;

        //부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

	private Vector3 ClampPositionToBounds(Vector3 targetPosition, Bounds bounds)
	{
		float camHalfHeight = _cam.orthographicSize;
		float camHalfWidth = camHalfHeight * _cam.aspect;

		float minX = bounds.min.x + camHalfWidth;
		float maxX = bounds.max.x - camHalfWidth;
		float minY = bounds.min.y + camHalfHeight;
		float maxY = bounds.max.y - camHalfHeight;

		// 맵이 카메라보다 작은 경우 → 중심 고정
		if (minX > maxX)
			targetPosition.x = bounds.center.x;
		else
			targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

		if (minY > maxY)
			targetPosition.y = bounds.center.y;
		else
			targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

		return targetPosition;
	}


	public float GetClampedTargetY()
	{
		if (_target == null) return transform.position.y;
		float targetY = _target.position.y + _offset.y;
		if (_currentBounds.HasValue)
		{
			float camH = _cam.orthographicSize;
			float minY = _currentBounds.Value.min.y + camH;
			float maxY = _currentBounds.Value.max.y - camH;
			if (minY > maxY) targetY = _currentBounds.Value.center.y;
			else targetY = Mathf.Clamp(targetY, minY, maxY);
		}
		return targetY;
	}

	public void SetBounds(Bounds bounds)
    {
        _currentBounds = bounds;
    }

    public void SetUIOffset(Vector3 offset)
    {
        _uiOffset = offset;
    }

    public void ClearUIOffset()
    {
        _uiOffset = Vector3.zero;
    }

	public void SnapToTarget()
	{
		if (_target == null) return;

		Vector3 targetPosition = _target.position + _offset;

		if (_currentBounds.HasValue)
		{
			targetPosition = ClampPositionToBounds(targetPosition, _currentBounds.Value);
		}

		transform.position = targetPosition;
	}
}

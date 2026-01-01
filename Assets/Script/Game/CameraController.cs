
using System;
using UnityEngine;

/// <summary>
/// 1.tracing target
/// 2.map boundary 
/// 3.smooth movement
/// </summary>
public class CameraController : MonoBehaviour
{
    //추적
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    //맵 경계
	[SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

	// Start is called before the first frame update
	void Start()
    {
		if (target == null)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
            if(player != null)
            {
                target = player.transform;
			}
			else
			{
				Debug.LogError("Player object with tag 'Player' not found in the scene.");
			}

		}
	}

    void LateUpdate()
    {
        if (target == null)
            return;

        //목표 위치
        Vector3 targetPosition = target.position + offset;


        //화면의 경계
		if (useBounds)
        {
            //경계 내로 제한
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
       
        //부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed*Time.deltaTime);

        transform.position = smoothedPosition;
    }

	public void SetBounds(Vector2 minBounds, Vector2 maxBounds)
	{

	}
}

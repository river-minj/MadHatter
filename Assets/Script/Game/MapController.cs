
using System.Collections;
using UnityEngine;

/// <summary>
/// 현재 활성화 된 맵을 관리
/// 카메라, 플레이어, 전환 흐름을 조율하는 컨트롤러
/// </summary>
public class MapController : MonoBehaviour
{
	//[SerializeField] private CameraController _cameraController;
	//[SerializeField] private Transform _player;
	[SerializeField] private MapBounds _mapBounds;

	public MapBounds MapBounds => _mapBounds;

	private void Awake()
	{
		if(_mapBounds == null)
		{
			_mapBounds = GetComponentInChildren<MapBounds>();
		}

		//if (_cameraController == null)
		//{
		//	_cameraController = Camera.main.GetComponent<CameraController>();
		//	if (_cameraController == null)
		//	{
		//		Debug.LogError("CameraController component is required on the main camera.");
		//	}
		//}

		//if (_player == null)
		//{
		//	GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		//	if (playerObj != null)
		//	{
		//		_player = playerObj.transform;
		//	}
		//	else
		//	{
		//		Debug.LogError("Player object with tag 'Player' is required in the scene.");
		//	}
		//}

	}
	private void Start()
	{
		InitializeMap();
	}

	private void InitializeMap()
	{
		if(_mapBounds != null && GameManager.Instance != null)
		{
			GameManager.Instance.ApplyCameraBounds(_mapBounds.GetBounds());
		}

		OnMapEnter();
	}

	protected virtual void OnMapEnter()
	{
		//맵 진입 시 처리 (적 스폰, 오브젝트 활성화 등)
		Debug.LogErrorFormat("[Map Controller] Entered {0}", gameObject.name);
	}

	protected virtual void OnMapExit()
	{
		//맵 진입 시 처리 (적 스폰, 오브젝트 활성화 등)
		Debug.LogErrorFormat("[Map Controller] Exiting {0}", gameObject.name);
	}

	//맵 전환 요청
	public void RequestMapTransition(GameObject nextMapObj, Transform playerSpawnPos)
	{
		if(nextMapObj == null)
		{
			Debug.LogError("New MapBounds is null.");
			return;
		}

		//if (_player != null)
		//{
		//	//플레이어 위치 이동
		//	_player.position = playerSpawnPosition;
		//}
		//else
		//{
		//	Debug.LogWarning("Player transform is not assigned.");
		//}

		GameManager.Instance?.RequestMapTransition(nextMapObj, playerSpawnPos);
	}

	//for debugging
	public Bounds GetCurrentMapBounds()
	{
		if(_mapBounds != null)
		{
			return _mapBounds._mapBound;
		}
		else
		{
			Debug.LogWarning("Current MapBounds is not assigned.");
			return default;
		}
	}
}

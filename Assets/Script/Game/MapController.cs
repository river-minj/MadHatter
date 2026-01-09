
using System.Collections;
using UnityEngine;

/// <summary>
/// 현재 활성화 된 맵을 관리
/// 카메라, 플레이어, 전환 흐름을 조율하는 컨트롤러
/// </summary>
public class MapController : MonoBehaviour
{
	[SerializeField] private CameraController _cameraController;
	[SerializeField] private Transform _player;
	[SerializeField] private MapBounds _currentMap;

	private void Awake()
	{
		if (_cameraController == null)
		{
			_cameraController = Camera.main.GetComponent<CameraController>();
			if (_cameraController == null)
			{
				Debug.LogError("CameraController component is required on the main camera.");
			}
		}

		if (_player == null)
		{
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null)
			{
				_player = playerObj.transform;
			}
			else
			{
				Debug.LogError("Player object with tag 'Player' is required in the scene.");
			}
		}

	}
	private void Start()
	{
		ApplyCurrentMap();
	}

	private void ApplyCurrentMap()
	{
		if (_cameraController == null || _currentMap == null)
		{
			Debug.LogWarning("CameraController or MapBounds is not assigned.");
			return;
		}

		_cameraController.SetBounds(_currentMap.GetBounds());
	}

	//맵 전환 요청
	public void ChangeMap(MapBounds newMap, Vector3 playerSpawnPosition)
	{
		if(newMap == null)
		{
			Debug.LogError("New MapBounds is null.");
			return;
		}

		//새로운 맵 갱신
		_currentMap = newMap;

		if (_player != null)
		{
			//플레이어 위치 이동
			_player.position = playerSpawnPosition;
		}
		else
		{
			Debug.LogWarning("Player transform is not assigned.");
		}

		//카메라 경계 갱신
		ApplyCurrentMap();
	}

	bool isTransitioning = false;
	public void RequestMapTransition(GameObject nextMapObj, Transform playerSpawnPos)
	{
		//이미 전환 중이면 무시
		if (isTransitioning)
			return;

		if(nextMapObj == null)
		{
			Debug.LogError("Next nextMapObj is null.");
			return;
		}

		GameManager.Instance?.RequestMapChange(nextMapObj, playerSpawnPos.transform);
	}
	
	//for debugging
	public Bounds GetCurrentMapBounds()
	{
		if(_currentMap != null)
		{
			return _currentMap._mapBound;
		}
		else
		{
			Debug.LogWarning("Current MapBounds is not assigned.");
			return default;
		}
	}
}

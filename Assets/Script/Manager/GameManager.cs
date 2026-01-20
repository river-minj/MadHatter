using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //전역 시스템 (플레이어, 카메라, UI 등) 관리
    [SerializeField] private CameraController _cameraCntroller;
    [SerializeField] private PlayerController _playerController;

    //맵 생성 위치
    [SerializeField] private Transform _mapRoot;
    //인스펙터로 임시 연결해둔 첫번째 맵, 나중에는 첫번째 맵을 데이터에서 찾아와 로드하도록 변경
    [SerializeField] MapController _firstMapMc;

    private GameObject? _currentMapObject;
    private MapController? _currentMapController;

    public MapController CurrentMapController => _currentMapController;

    

    private void Awake()
    {
        //게임 매니저
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
		}
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //카메라
        if (_cameraCntroller == null)
        {
            _cameraCntroller = Camera.main.GetComponent<CameraController>();
            Debug.LogError("CameraController component is required on the main camera.");
        }
        
        //플레이어
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerController = playerObj.GetComponent<PlayerController>();
        }

		//시작 맵 설정
		SetCurrentMap(_firstMapMc);
		SetcurrentMapObject(_firstMapMc ? _firstMapMc.gameObject : null);
	}

    public void SetCurrentMap(MapController mc)
    {
        _currentMapController = mc;
    }

    public void SetcurrentMapObject(GameObject currentMap)
    {
        _currentMapObject = currentMap;
    }

    public void RequestMapTransition(GameObject nextMapPrefab, Transform playerSpawnPos)
    {
        Debug.LogError("[GameManager] Map transition requested.");

		if (nextMapPrefab == null)
		{
			Debug.LogError("[GameManager] Next map prefab is null.");
			return;
		}

		if (UIManager.Instance == null)
		{
			Debug.LogError("[GameManager] UIManager not found.");
			return;
		}

        //맵 전환 요청 처리
        //맵 전환 연출은 ui 매니저에게 맡긴다.
        //페이드 아웃
        UIManager.Instance?.RequestFadeTransition(0, () => ChangeMap(nextMapPrefab, playerSpawnPos),
			() =>
			{
				Debug.LogErrorFormat("[GameManager] Map Transition complete");
			});

		//기존 맵 언로드
		//새 맵 로드
		//플레이어 위치 설정
		//카메라 경계 설정
		//페이드 인
	}

    private void ChangeMap(GameObject nextMapObject, Transform playerSpawnPos)
    {
        //기존 맵 언로드
        if (CurrentMapController != null)
        {
            //protected virtual 메서드라서 직접 호출 불가 -> 상속받은 클래스에서 처리하도록 변경 필요
            //CurrentMapController?.OnMapExit();

            Destroy(CurrentMapController.gameObject);
            Debug.LogErrorFormat("[GameManager] Unloaded map: {0}", CurrentMapController.gameObject.name);
        }

        //새 맵 로드
        _currentMapObject = Instantiate(nextMapObject, _mapRoot) as GameObject;
        _currentMapController = _currentMapObject.GetComponent<MapController>();
        Debug.LogErrorFormat("[GameManager] Loaded new map: {0}", _currentMapObject.name);

        //플레이어 위치 설정
        if(_playerController != null && playerSpawnPos != null)
        {
            _playerController.GetComponentInParent<Transform>().position = playerSpawnPos.position;
        }
        
        //카메라 경계 설정
	}

	public void ApplyCameraBounds(Bounds bounds)
	{
        if(_cameraCntroller != null)
        {
            _cameraCntroller.SetBounds(bounds);
            Debug.LogErrorFormat("[GameManager] Applied camera bounds: {0}", bounds);
        }
	}
}

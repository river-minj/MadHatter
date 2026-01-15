using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //전역 시스템 (플레이어, 카메라, UI 등) 관리
    [SerializeField] private CameraController _cameracCntroller;
    [SerializeField] private PlayerController _playerController;

    public MapController CurrentMap { get; private set; }

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
        if (_cameracCntroller == null)
        {
            _cameracCntroller = Camera.main.GetComponent<CameraController>();
            Debug.LogError("CameraController component is required on the main camera.");
        }
        
        //플레이어
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerController = playerObj.GetComponent<PlayerController>();
        }
	}

	void Start()
    {
        //시작 맵 설정
        CurrentMap = FindObjectOfType<MapController>();
	}

    public void RequestMapTransition(GameObject newMapObj, Transform playerSpawnPos)
    {
        Debug.LogError("[GameManager] Map transition requested.");

        //맵 전환 요청 처리
        //StartCoroutine(ChangeMapRoutine);
        //맵 전환 연출은 ui 매니저에게 맡긴다.
        //페이드 아웃
        UIManager.Instance?.RequestFadeTransition(1.0f, () => ChangeMap(), null);
		//기존 맵 언로드
		//새 맵 로드
		//플레이어 위치 설정
		//카메라 경계 설정
		//페이드 인
	}

    private void ChangeMap()
    {
        //기존 맵 언로드
        if (CurrentMap != null)
        {
            Destroy(CurrentMap.gameObject);
        }
        //새 맵 로드
        //플레이어 위치 설정
        
        //카메라 경계 설정
	}

}

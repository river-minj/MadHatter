using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //전역 시스템 (플레이어, 카메라, UI 등) 관리
    private CameraController _cameracCntroller;
    private PlayerController _playerController;

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

    public void RequestMapChange(GameObject newMapObj, Transform playerSpawnPos)
    {
		//맵 전환 요청 처리
	    //StartCoroutine(ChangeMapRoutine);
		//맵 전환 연출은 ui 매니저에게 맡긴다.
		//페이드 아웃
		//UIManager.Instance;

		//기존 맵 언로드
		//새 맵 로드
		//플레이어 위치 설정
		//카메라 경계 설정
		//페이드 인
	}

	private IEnumerator ChangeMapRoutine()
    {
		//GameManager는 현재의 맵을 다음 맵으로 전환하는 요청을 받아 전환하는 역할을 한다.
		//전환 시의 연출은 UIManager에게 맡긴다.
     
        yield return null;
	}
}

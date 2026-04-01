using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// game manager는 게임의 전역 상태와 시스템을 관리하는 싱글톤 클래스
/// 게임의 전체 흐름을 제어하고, 맵 전환, 플레이어 상태, UI 관리 등을 담당
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //전역 시스템 (플레이어, 카메라, UI 등) 관리
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private PlayerController _playerController;

    //맵 생성 위치
    [SerializeField] private Transform _mapRoot;
    //인스펙터로 임시 연결해둔 첫번째 맵, 나중에는 첫번째 맵을 데이터에서 찾아와 로드하도록 변경
    [SerializeField] MapController _firstMapMc;

    public MapController CurrentMapController => _currentMapController;
    private MapController? _currentMapController;

    public bool IsInputLock { get; private set; }
    

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
        if (_cameraController == null)
        {
            _cameraController = Camera.main.GetComponent<CameraController>();
            Debug.LogError("CameraController component is required on the main camera.");
        }
        
        //플레이어
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerController = playerObj.GetComponent<PlayerController>();
        }
	}

	private void OnDestroy()
	{
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestRewardClaimed -= OnQuestRewardClaimed;
        }
    }

	public void SaveGame()
    {

		Debug.Log($"[GameManager] PlayerInfoManager: {PlayerInfoManager.Instance}");
		Debug.Log($"[GameManager] QuestManager: {QuestManager.Instance}");

		SaveData data = new SaveData
        {
            playerInfo = PlayerInfoManager.Instance.GetSaveData(),
            questInfo = QuestManager.Instance.GetSaveData(),
            inventoryData = InventoryManager.Instance.GetSaveData(),
            companionData = CompanionManager.Instance.GetSaveData(),
        };

        GameSystem.Save(data);
    }

    public void LoadGame(SaveData data)
    {
        if (data == null) return;

        PlayerInfoManager.Instance.ApplyData(data.playerInfo);
        QuestManager.Instance.ApplyData(data.questInfo);
        InventoryManager.Instance.ApplyData(data.inventoryData);
        CompanionManager.Instance.ApplyData(data.companionData);
	}

	public void Start()
	{
		QuestManager.Instance.OnQuestRewardClaimed += OnQuestRewardClaimed;
		InventoryManager.Instance.OnInventoryChanged += SaveGame;
		InventoryManager.Instance.OnEquipChanged += SaveGame;

		if (SceneLoader.Instance != null)
		{
			// 정상 흐름: SceneLoader가 LoadGame 관리, 맵만 로드
			LoadFristMap();
			return;
		}

		// 개발 모드: Main 씬 직접 실행
		StartCoroutine(DevModeInit());
	}

	private IEnumerator DevModeInit()
	{
		if (DataManager.Instance != null && !DataManager.Instance.IsLoaded)
		{
			yield return DataManager.Instance.LoadAllDataAsync();
		}

		if (GameSystem.Exists())
		{
			SaveData data = GameSystem.Load();
			LoadGame(data);
		}

		LoadFristMap();
	}

	private void OnQuestRewardClaimed(string questId)
    {
        SaveGame();
    }

    private void LoadFristMap()
    {
        if (_firstMapMc == null)
        {
			Debug.LogError("[GameManager] First map controller is not assigned.");
			return;
		}

        ChangeMap(_firstMapMc, SpawnPointId.Default, save: false);
    
    }

    public void RequestMapTransition(MapController nextMap, SpawnPointId spawnPointId)
    {
        Debug.Log("[GameManager] Map transition requested.");

		if (nextMap == null)
		{
			Debug.Log("[GameManager] Next map prefab is null.");
			return;
		}

		if (UIManager.Instance == null)
		{
			Debug.Log("[GameManager] UIManager not found.");
			return;
		}

        SetLockInput(true);

        UIManager.Instance?.RequestFadeTransition(0, () => ChangeMap(nextMap, spawnPointId),
			() =>
			{
                SetLockInput(false);
				Debug.LogFormat("[GameManager] Map Transition complete");
			});

	}

    private void ChangeMap(MapController nextMap, SpawnPointId spawnPointId = SpawnPointId.Default, bool save = true)
    {
        //기존 맵 언로드
        if (CurrentMapController != null)
        {
            //protected virtual 메서드라서 직접 호출 불가 -> 상속받은 클래스에서 처리하도록 변경 필요
            //CurrentMapController?.OnMapExit();

            Destroy(CurrentMapController.gameObject);
            Debug.LogFormat("[GameManager] Unloaded map: {0}", CurrentMapController.gameObject.name);
        }

		//새 맵 로드
		//새 맵이 instantiate될 때 해당 map의 mapController의 start에서 카메라 바운드 적용 요청을 함
		_currentMapController = Instantiate(nextMap, _mapRoot);
        _cameraController.SetBounds(_currentMapController.GetCurrentMapBounds());

        //플레이어 위치 설정
        Transform playerSpawnPos = _currentMapController.GetSpawnPoint(spawnPointId);
        if(_playerController != null && playerSpawnPos != null)
        {
            _playerController.SetPosition(playerSpawnPos);
        }
    
        Debug.LogFormat("[GameManager] Loaded new map: {0}", _currentMapController.gameObject.name);
    
        if(save)
        {
            SaveGame();
        }
	}

    public void SetLockInput(bool locked)
    {
        IsInputLock = locked;
    }

    public void StartDialogue(DialogueData dialogueData, Action onComplete = null)
    {
        SetLockInput(true);

        UIManager.Instance?.StartDialogue(dialogueData._lines, () => {

            onComplete?.Invoke();

            EndDialogue();
        });
    }

    public void EndDialogue()
	{

		SetLockInput(false);
    }

    public PlayerController GetPlayerController()
    {
        return _playerController;
	}

    public void SnapCamera()
	{
		if (_cameraController != null)
		{
			_cameraController.SnapToTarget();
		}
	}
}

# PROJECT STRUCTURE

## 개요
2D 탑다운 RPG 게임 (Unity, uGUI)
씬 구조: Start → Main (2씬, 나중에 Boot 씬 분리 가능)
입력 시스템: Legacy Input Manager (Input.GetAxisRaw)
플랫폼 대응: PC(키보드) + WebGL/모바일(가상 조이스틱)

---

## 씬 구성

### Start 씬
| 오브젝트 | 컴포넌트 | 비고 |
|---|---|---|
| SceneLoader | SceneLoader.cs (DontDestroyOnLoad) | 씬 전환 + 로딩 총괄 |
| └ LoadingCanvas | LoadingUI.cs, Canvas (Sort Order 100) | 로딩바 오버레이 |
| TitleCanvas | TitleUI.cs | 시작/이어하기/옵션/끝내기 |

### Main 씬
매니저들이 DontDestroyOnLoad로 유지됨.

---

## 매니저 (싱글톤, DontDestroyOnLoad)

### GameManager
- 역할: 게임 전역 상태, 맵 전환, 입력 잠금, 대화 시작/종료, 저장/불러오기 진입점
- 필드:
  - `CameraController _cameraController` (SerializeField, 없으면 Camera.main에서 GetComponent)
  - `PlayerController _playerController` (SerializeField, 없으면 태그 "Player"로 Find)
  - `Transform _mapRoot` (맵 프리팹이 Instantiate될 부모 Transform)
  - `MapController _firstMapMc` (SerializeField, Inspector에서 첫 번째 맵 프리팹 연결)
  - `MapController _currentMapController` (현재 활성 맵, 읽기전용 프로퍼티 CurrentMapController)
  - `bool IsInputLock` (읽기전용 프로퍼티)
- 주요 메서드:
  - `RequestMapTransition(MapController nextMap, SpawnPointID spawnPointID)` → 입력잠금 → 페이드인 → ChangeMap → 페이드아웃 → 입력잠금 해제
  - `ChangeMap(MapController nextMap, SpawnPointID spawnPointID = Default)` → 기존맵 Destroy → 새맵 Instantiate → 카메라 바운드 적용 → 스폰포인트 적용 → SaveGame
  - `LoadFirstMap()` → ChangeMap(_firstMapMc) 호출 (spawnPointID Default)
  - `SetLockInput(bool locked)`
  - `StartDialogue(DialogueData data, Action onComplete)` → 입력잠금 + UIManager.StartDialogue
  - `EndDialogue()` → 입력잠금 해제
  - `SaveGame()` → 각 매니저 GetSaveData 수집 → GameSystem.Save
  - `LoadGame(SaveData data)` → 각 매니저 ApplyData 호출
  - `GetPlayerController()` → PlayerController (JoystickUI에서 참조용)
- 저장 트리거:
  - Start()에서 `QuestManager.OnQuestRewardClaimed` 구독 → 자동저장
  - `ChangeMap()` 완료 시 → 맵 이동 자동저장 (단, LoadFirstMap은 save: false로 제외)
  - `TitleUI.OnQuit()` 호출 시 → 수동저장
  - `QuestManager.StartQuest()` 내부 직접 호출 → 퀘스트 수락 시 저장
  - `QuestManager.AbandonQuest()` 내부 직접 호출 → 퀘스트 포기 시 저장

### UIManager
- 역할: 모든 UI 관리 진입점
- 필드:
  - `NPCPromptUI _npcPromptUI`
  - `DialogueController _dialogueController`
  - `InventoryUI _inventoryUI`
  - `QuestUI _questUI`
  - `Image _fadeImage`
- 주요 메서드:
  - `RequestFadeTransition(float targetAlpha, Action onFadeComplete, Action onComplete)`
  - `StartDialogue(string speakerName, string[] lines, Action onComplete)`
  - `ShowConfirmPopup(string message, Action onConfirm)`
  - `ToggleInventory()`
  - `ToggleQuest()`

### QuestManager
- 역할: 퀘스트 등록/진행/보상 수령
- 필드:
  - `Dictionary<string, QuestState> _dicActiveQuest` (진행 중인 퀘스트, questID 키)
  - `HashSet<string> _setStartedQuest` (시작된 모든 퀘스트 ID)
  - `HashSet<string> _setCompletedQuest` (완료된 모든 퀘스트 ID)
- 주요 메서드:
  - `TryQuestStart(string questID)` → 퀘스트 시작 조건 확인 후 등록
  - `ClaimReward(string questID)` → 완료 확인 → GiveReward → 이벤트 발행
  - `GiveReward(QuestReward reward)` → PlayerInfoManager.AddGold/AddExp + CompanionManager.AddCompanion
  - `ReportTalktoNPC(string npcID)` → Talk 타입 퀘스트 진행
  - `ReportKill(string targetID)` → Kill 타입 퀘스트 진행 (미구현)
  - `ReportReach(string targetID)` → Explore 타입 퀘스트 진행 (미구현)
  - `GetSaveData()` → QuestSaveData
  - `ApplyData(QuestSaveData data)` → 세이브 데이터로 상태 복원 후 OnQuestListChanged 발행
- 이벤트:
  - `Action OnQuestListChanged`
  - `Action<string, int> OnQuestProgressUpdate` (questID, currentProgress)
  - `Action<string> OnQuestRewardClaimed` (questID)

### PlayerInfoManager
- 역할: 플레이어 레벨/경험치/골드 관리
- 필드: PlayerInfo 구조체 `_playerInfo`
  - `string _name`, `int _level`, `int _exp`, `int _gold`
- 주요 메서드:
  - `AddGold(int amount)`
  - `AddExp(int amount)`
  - `AddLevel(int amount)`
  - `GetMaxCompanionCount()` → int (레벨 기반 동료 최대 수)
  - `GetSaveData()` → PlayerInfoSaveData
  - `ApplyData(PlayerInfoSaveData data)` → 복원 후 OnGoldChanged, OnLevelChanged, OnExpChanged 발행
- 이벤트:
  - `Action<int> OnGoldChanged`
  - `Action<int> OnLevelChanged`
  - `Action<int> OnExpChanged`

### CompanionManager
- 역할: 언락된 동료 관리, 생성, 줄 배치
- 필드:
  - `List<string> _ownedCompanions` (언락된 동료 ID 목록)
  - `List<Transform> _lineA`, `List<Transform> _lineB` (줄 배치 위치)
- 주요 메서드:
  - `AddCompanion(string companionID)` → 동료 언락 + SpawnCompanion
  - `SpawnCompanion(string companionID)` → CompanionDatabase에서 데이터 조회 후 Instantiate
  - `SetFacingDirection(Vector2 direction)` → 동료들 방향 전환

---

## 플레이어 시스템

### PlayerController
- 역할: 플레이어 이동, 애니메이션, 입력 처리 (키보드 + 조이스틱 통합)
- 필드:
  - `float _moveSpeed` (SerializeField)
  - `Rigidbody2D _rb`
  - `Animator _animator`
  - `Vector2 _moveDir` (현재 이동 방향)
  - `Vector2 _lastDir` (마지막 이동 방향, 정지 시 애니메이션용)
  - `Vector2 _joystickInput` (JoystickUI에서 주입받는 방향값)
  - `bool _isJoystickActive` (조이스틱 입력 활성 여부, magnitude > 0.1f 기준)
- 주요 메서드:
  - `HandleInput()` → _isJoystickActive 여부에 따라 조이스틱/키보드 분기
  - `SetPosition(Transform spawnPoint)` → 위치 즉시 이동
  - `SetJoystickInput(Vector2 direction)` → JoystickUI가 매 프레임 호출, _joystickInput/_isJoystickActive 갱신
- 입력 분기 로직:
  - `_isJoystickActive == true` → `_moveDir = _joystickInput`
  - `_isJoystickActive == false` → `Input.GetAxisRaw("Horizontal/Vertical")`로 4방향 이동
  - 수직 입력 우선 (vertical != 0이면 horizontal 무시)
- 단축키: `KeyCode.I` → ToggleInventory, `KeyCode.Q` → ToggleQuest

---

## 맵 시스템

### MapController
- 역할: 활성화된 맵의 룰/정보 관리, 스폰포인트 제공, 맵 전환 요청
- 필드:
  - `List<SpawnPointEntry> _spawnPointList` (SerializeField, Inspector 등록 → Awake에서 Dictionary 변환)
  - `Dictionary<SpawnPointID, Transform> _spawnPoints` (런타임 조회용)
  - `MapController _nextMapMc` (다음 맵 프리팹 참조)
  - `MapBounds _mapBounds`
- 주요 메서드:
  - `GetSpawnPoint(SpawnPointID id)` → Transform (없으면 Default로 fallback, Default도 없으면 LogError)
  - `RequestMapTransition(SpawnPointID spawnPointID)` → GameManager.RequestMapTransition 호출
  - `GetCurrentMapBounds()` → Bounds
  - `OnMapEnter()` (virtual), `OnMapExit()` (virtual)

### SpawnPointID (enum) - MapController.cs 상단 정의
- Default, Left, Right, North, South

### SpawnPointEntry (class) - MapController.cs 상단 정의
- `SpawnPointID id`, `Transform point`
- 역할: Inspector에서 enum+Transform 쌍으로 등록하기 위한 직렬화 클래스
- Dictionary 직렬화 불가 문제를 List로 우회, Awake에서 Dictionary로 변환

### MapTransistor
- 역할: 맵 전환 트리거 (문, 포탈 등 충돌 기반)
- 필드: `SpawnPointID _spawnPointID` (이 출구로 나가면 도착 맵의 어느 스폰포인트에 세울지)
- 흐름: OnTriggerEnter2D(Player 태그) → MapController.RequestMapTransition(_spawnPointID)

### MapBounds
- 역할: 카메라 제한용 맵 크기 계산 전용 컴포넌트
- 필드: `BoxCollider2D _boundsCollider`, `Bounds _mapBound`
- 주요 메서드: `GetBounds()` → Bounds

---

## UI 클래스

### JoystickUI
- 역할: 모바일/WebGL 가상 조이스틱 입력 처리, PlayerController에 방향값 주입
- 필드:
  - `RectTransform _joystickRoot` (조이스틱 비주얼 루트, 평소 비활성)
  - `RectTransform _handle` (드래그 시 움직이는 핸들)
  - `float _handleRange = 60f` (핸들 최대 이동 반경)
  - `PlayerController _playerController` (Awake에서 GameManager.GetPlayerController()로 획득)
  - `Canvas _canvas` (GetComponentInParent)
- 인터페이스: `IPointerDownHandler`, `IDragHandler`, `IPointerUpHandler`
- 동작 방식: Floating (터치한 위치에 _joystickRoot 생성, 평소 숨김)
- 주요 메서드:
  - `OnPointerDown` → 터치 위치에 _joystickRoot 배치 후 활성화
  - `OnDrag` → 핸들 위치 계산(ClampMagnitude) → 방향값 정규화 → SetJoystickInput 호출
  - `OnPointerUp` → _joystickRoot 비활성화, 핸들 초기화, SetJoystickInput(Vector2.zero)
- 플랫폼 처리: `#if !UNITY_WEBGL && !UNITY_ANDROID && !UNITY_IOS` → gameObject.SetActive(false)
- Inspector 설정:
  - JoystickArea: Image(alpha=0), Raycast Target=ON, 전체화면 RectTransform
  - JoystickRoot: 기본 비활성, Background(반투명 원) + Handle(작은 원) 자식으로 구성
- 주의: 부모 Canvas의 Render Mode가 반드시 **Screen Space - Overlay**여야 함. Screen Space - Camera 사용 시 좌표계가 뒤집혀 조이스틱 방향과 캐릭터 이동 방향이 불일치함

### QuestUI
- 역할: 퀘스트 패널 (슬롯 리스트 관리)
- 구독: OnQuestListChanged, OnQuestProgressUpdate, OnQuestRewardClaimed
- 슬롯 생성 시 OnClaimButtonClicked 콜백 연결 → ClaimReward 중개

### QuestSlotController
- 역할: 개별 퀘스트 슬롯 프리팹
- UI: 제목, 설명, 진행도, 보상받기 버튼
- 이벤트: `Action<string> OnClaimButtonClicked` (QuestUI가 구독)
- 버튼: 완료 시 활성(금색), 미완료 시 비활성(회색)

### SceneLoader
- 역할: 씬 전환 + 로딩 오버레이 총괄
- DontDestroyOnLoad, Start 씬에 배치
- 새 게임 시: Main 씬 로드 → GameManager.Start()에서 LoadFirstMap
- 이어하기 시: `GameSystem.Load()` → Main 씬 로드 → `GameManager.LoadGame(data)` 호출

### LoadingUI
- 역할: 로딩바 + 퍼센트 텍스트 + 상태 텍스트
- SceneLoader 하위 Canvas (Sort Order 100)

### TitleUI
- 역할: 시작화면 버튼 (시작/이어하기/옵션/끝내기)
- Start()에서 `GameSystem.Exists()`로 이어하기 버튼 interactable 결정
- OnQuit() → GameManager.SaveGame() → Application.Quit()

---

## 데이터베이스 (싱글톤)

### QuestDatabase
- `GetQuestByID(string questID)` → QuestData
- `GetAllQuests()` → IEnumerable<QuestData>
- Inspector에서 List<QuestData> 등록, Awake에서 Dictionary로 변환

### DialogueDatabase
- `GetDialogueByID(string dialogueID)` → DialogueData
- Inspector에서 List<DialogueData> 등록

### CompanionDatabase (코드 미공유)
- `GetCompanionByID(string companionID)` → CompanionData

---

## 데이터 클래스

### QuestData (ScriptableObject 예정, 현재 일반 클래스)
- `string _questID`, `string _title`, `string _description`, `int _goalCount`
- `QuestGoalType _goalType`, `string _targetID`, `string _npcID`
- `string _startDialogueID`, `string _progressDialogueID`, `string _completedDialogueID`
- `string _nextQuestID`
- `QuestReward _reward`

### QuestState (런타임 상태)
- `QuestData _data`, `int _currentProgress`, `bool _isCompleted`
- `AddProgress(int amount = 1)` → bool (완료 여부 반환, 이미 완료면 false)

### QuestReward
- `int _gold`, `int _exp`, `string _companionID`, `string _itemID`

### QuestGoalType (enum)
- None, Kill, Collect, Talk, Explore, AcquireItem

### PlayerInfo (struct)
- `string _name`, `int _level`, `int _exp`, `int _gold`

### DialogueData (코드 미공유)
- `string _dialogueID`, `string _speakerName`, `GetLines()` → string[]

---

## 세이브 시스템

### GameSystem (static 유틸) - GameSystem.cs
- 역할: JSON 파일 I/O 전담, GameManager가 진입점
- 저장 경로: `Application.persistentDataPath/save.json`
- WebGL: IndexedDB 자동 사용, 별도 처리 불필요
- 주요 메서드:
  - `Save(SaveData data)` → JsonUtility.ToJson → File.WriteAllText
  - `Load()` → File.ReadAllText → JsonUtility.FromJson → SaveData (파일 없으면 null)
  - `Exists()` → bool
  - `Delete()`

### SaveData / 직렬화 클래스들 - GameSystem.cs 내부, GameSystem class 바깥에 정의

#### SaveData (직렬화 루트)
- `PlayerInfoSaveData playerInfo`
- `QuestSaveData questData`

#### PlayerInfoSaveData
- `string name`, `int level`, `int exp`, `int gold`

#### QuestSaveData
- `List<string> startedQuests`
- `List<string> completedQuests`
- `List<ActiveQuestEntry> activeQuests`

#### ActiveQuestEntry (class)
- `string questID`, `int currentProgress`, `bool isCompleted`
- QuestState 직렬화 전용 대체 클래스 (QuestData 참조 제거, JsonUtility List 안정성을 위해 class 유지)

---

## To Do

| 번호 | 내용 | 상태 |
|---|---|---|
| 9 | 게임 시작화면 (Start 씬) | ✅ 완료 |
| 10 | 로딩화면 + 게임데이터 로드 | ✅ 완료 |
| 11 | 첫번째 맵에서 플레이어 생성 + 맵별 스폰포인트 | ✅ 완료 |
| 12 | 게임 진행 정보 저장/종료 (JSON) | ✅ 완료 |
| 13 | WebGL 빌드하여 웹에 배포 | 보류 |
| 14 | 모바일/WebGL 가상 조이스틱 입력 | ✅ 완료 |
| 15 | 엑셀 → 게임 데이터 파싱 시스템 | 미구현 |
| 16 | 다이얼로그 시스템 확장 (독백/시스템 메시지) | 미구현 |
| 17 | ReportKill/ReportReach 완성 + 전투 시스템 기초 | 미구현 |
| 18 | ReportCollect/AcquireItem 추가 | 미구현 |
| 19 | AudioManager (BGM/SFX) + 옵션 UI 연동 | 미구현 |
| 20 | 인벤토리 아이템 획득/사용/장착 + _itemID 보상 지급 | 미구현 |

---

## 설계 원칙
- 이벤트는 매니저에 선언, UI가 구독
- UI 슬롯은 매니저 직접 참조 안 함 → 부모 UI가 콜백으로 중개
- DontDestroyOnLoad 매니저는 현재 Main 씬에 배치 (나중에 Boot 씬 분리 가능)
- **모든 매니저는 Main 씬 Hierarchy에 오브젝트로 배치되어야 함** (누락 시 Instance null 에러 발생)
- Save/Load 진입점은 GameManager, 실제 파일 I/O는 GameSystem(static 유틸)에 위임
- 저장 대상 클래스는 필드명과 타입을 MD에 기록 (파일 재요청 방지)
- 런타임 클래스(QuestState 등)는 직렬화 전용 클래스로 분리 (QuestData 등 참조 타입 제거)
- Inspector에서 Dictionary 직렬화 불가 → List로 등록 후 Awake에서 Dictionary 변환
- 플랫폼 분기는 #if 전처리기 사용, 조이스틱은 PC에서 자동 비활성화
- 매니저 간 단순 메서드 호출은 허용 (ex. GameManager.Instance.SaveGame()), 내부 데이터 직접 조작은 금지 (ex. GameManager.Instance._someData = value)

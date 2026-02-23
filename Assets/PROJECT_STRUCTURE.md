# PROJECT STRUCTURE

## 개요
2D 탑다운 RPG 게임 (Unity, uGUI)
씬 구조: Start → Main (2씬, 나중에 Boot 씬 분리 가능)

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
- 역할: 게임 전역 상태, 맵 전환, 입력 잠금, 대화 시작/종료
- 주요 메서드: `RequestMapTransition()`, `StartDialogue()`, `EndDialogue()`, `SetLockInput()`
- 참조: CameraController, PlayerController, MapController
- TODO: SaveGame() / LoadGame() 추가 예정 (12번)

### UIManager
- 역할: 모든 UI 관리 진입점
- 주요 메서드: `RequestFadeTransition()`, `StartDialogue()`, `ShowConfirmPopup()`, `ToggleInventory()`, `ToggleQuest()`
- 참조: NPCPromptUI, DialogueController, InventoryUI, QuestUI, fadeImage

### QuestManager
- 역할: 퀘스트 등록/진행/보상 수령
- 데이터: `_dicActiveQuest` (Dictionary<string, QuestState>), `_setStartedQuest`, `_setCompletedQuest`
- 주요 메서드: `TryQuestStart()`, `ClaimReward()`, `ReportTalktoNPC()`, `ReportKill()`, `ReportReach()`
- 이벤트: `OnQuestListChanged`, `OnQuestProgressUpdate`, `OnQuestRewardClaimed`
- 보상 흐름: ClaimReward → GiveReward → PlayerInfoManager.AddGold/AddExp + CompanionManager.AddCompanion
- TODO: ReportKill/ReportReach 완성, ReportCollect/AcquireItem 추가

### PlayerInfoManager
- 역할: 플레이어 레벨/경험치/골드 관리
- 데이터: PlayerInfo 구조체 (_level, _exp, _gold)
- 주요 메서드: `AddGold()`, `AddExp()`, `AddLevel()`, `GetMaxCompanionCount()`
- 이벤트: `OnGoldChanged`, `OnLevelChanged`, `OnExpChanged`

### CompanionManager
- 역할: 언락된 동료 관리, 생성, 줄 배치
- 주요 메서드: `AddCompanion()`, `SpawnCompanion()`, `SetFacingDirection()`
- 데이터: _ownedCompanions, _lineA, _lineB

---

## 데이터베이스 (싱글톤)

### QuestDatabase
- `GetQuestByID(string)` → QuestData
- `GetAllQuests()` → IEnumerable<QuestData>
- Inspector에서 List<QuestData> 등록, Dictionary로 매핑

### DialogueDatabase
- `GetDialogueByID(string)` → DialogueData
- Inspector에서 List<DialogueData> 등록

### CompanionDatabase (코드 미공유)
- `GetCompanionByID(string)` → CompanionData

---

## 데이터 클래스

### QuestData (ScriptableObject 예정, 현재 필드)
- _questID, _title, _description, _goalCount
- _goalType (QuestGoalType enum), _targetID, _npcID
- _startDialogueID, _progressDialogueID, _completedDialogueID
- _nextQuestID
- _reward (QuestReward)

### QuestState (런타임 상태)
- _data (QuestData), _currentProgress, _isCompleted
- AddProgress(int) → bool (완료 여부 반환)

### QuestReward
- _gold, _exp, _companionID, _itemID

### QuestGoalType (enum)
- None, Kill, Collect, Talk, Explore, AcquireItem

### PlayerInfo (struct)
- _level, _exp, _gold

### DialogueData (코드 미공유)
- _dialogueID, _speakerName, GetLines()

---

## UI 클래스

### QuestUI
- 역할: 퀘스트 패널 (슬롯 리스트 관리)
- 구독: OnQuestListChanged, OnQuestProgressUpdate, OnQuestRewardClaimed
- 슬롯 생성 시 OnClaimButtonClicked 콜백 연결 → ClaimReward 중개

### QuestSlotController
- 역할: 개별 퀘스트 슬롯 프리팹
- UI: 제목, 설명, 진행도, 보상받기 버튼
- 이벤트: Action<string> OnClaimButtonClicked (QuestUI가 구독)
- 버튼: 완료 시 활성(금색), 미완료 시 비활성(회색)

### SceneLoader
- 역할: 씬 전환 + 로딩 오버레이
- DontDestroyOnLoad, Start 씬에 배치
- TODO: SaveSystem 연동 (12번)

### LoadingUI
- 역할: 로딩바 + 퍼센트 + 상태 텍스트
- SceneLoader 하위 Canvas

### TitleUI
- 역할: 시작화면 버튼 (시작/이어하기/옵션/끝내기)
- TODO: 이어하기 버튼 세이브 파일 존재 여부로 활성화 (12번)

---

## To Do

| 번호 | 내용 | 상태 |
|---|---|---|
| 9 | 게임 시작화면 (Start 씬) | ✅ 완료 |
| 10 | 로딩화면 + 게임데이터 로드 | ✅ 완료 (SaveSystem 연동 대기) |
| 11 | 첫번째 맵에서 플레이어 생성 | 기존 GameManager.Start에서 처리 중 |
| 12 | 게임 진행 정보 저장/종료 (JSON) | 미구현 - GameManager.SaveGame/LoadGame + SaveSystem |
| 13 | WebGL 빌드하여 웹에 배포 | 미구현 |
| - | ReportKill / ReportReach 완성 | 미구현 (13번 이후) |
| - | ReportCollect / AcquireItem 추가 | 미구현 |
| - | _itemID 보상 지급 로직 | 미구현 (ItemManager 필요) |

---

## 설계 원칙
- 이벤트는 매니저에 선언, UI가 구독
- UI 슬롯은 매니저 직접 참조 안 함 → 부모 UI가 콜백으로 중개
- DontDestroyOnLoad 매니저는 현재 Main 씬에 배치 (나중에 Boot 씬 분리 가능)
- Save/Load 진입점은 GameManager, 실제 파일 I/O는 SaveSystem(static 유틸)에 위임

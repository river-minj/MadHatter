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
  - `StartDialogue(DialogueData data, Action onComplete)` → 입력잠금 + UIManager.StartDialogue(data._lines, onComplete)
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
  - `DialogueUI _dialogueUI`
  - `InventoryUI _inventoryUI`
  - `QuestUI _questUI`
  - `Image _fadeImage`
- 주요 메서드:
  - `RequestFadeTransition(float targetAlpha, Action onFadeComplete, Action onComplete)`
  - `StartDialogue(List<DialogueLine> lines, Action onComplete)` → DialogueUI.StartDialogue 호출
  - `AdvanceDialogue()` → DialogueUI.AdvanceDialogue 호출 (PlayerController에서 중개용)
  - `IsDialogueOpen()` → bool (DialogueUI.IsDialogueRunning)
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
  - `ClaimReward(string questID)` → 완료 확인 → GetReward → 이벤트 발행
  - `GetReward(QuestData qd)` → _rewards 리스트 순회하며 gold/exp/companion/item 지급
  - `ReportTalktoNPC(string npcID)` → Talk 타입 퀘스트 진행 (_targetId로 매칭)
  - `ReportKill(string enemyID)` → Kill 타입 퀘스트 진행 (_targetId로 매칭)
  - `ReportReach(string locationID)` → Explore 타입 퀘스트 진행 (_targetId로 매칭)
  - `GetSaveData()` → QuestSaveData
  - `ApplyData(QuestSaveData data)` → 세이브 데이터로 상태 복원 후 OnQuestListChanged 발행
- 이벤트:
  - `Action OnQuestListChanged`
  - `Action<QuestState> OnQuestProgressUpdate` (QuestState)
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
  - `AddCompanion(CompanionData data)` → 동료 언락 + SpawnCompanion
  - `SpawnCompanion(CompanionData data)` → Resources.Load(data._companionPrefabPath)로 프리팹 로드 후 Instantiate
  - `SetFacingDirection(Vector2 direction)` → 동료들 방향 전환

### DataManager (싱글톤, MonoBehaviour) - DataManager.cs
- 역할: JSON 로드 + 역직렬화 + Database 인스턴스 생성 + 데이터 분배
- Main 씬 Hierarchy에 배치
- 필드:
  - `bool IsLoaded` (읽기전용 프로퍼티)
- 주요 메서드:
  - `LoadAllDataAsync(Action<float, string> onProgress)` → 코루틴, Database 인스턴스 생성 → JSON 로드 → ApplyData → 프로그레스 콜백
  - `LoadTable<T>(string tableName)` → Resources.Load로 JSON 로드 → Newtonsoft로 List<T> 역직렬화
- 로드 순서: DialogueTable → QuestTable → RewardTable → NpcTable → CompanionTable
- 라이브러리: Newtonsoft Json.NET (com.unity.nuget.newtonsoft-json)

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
- 대화 중 입력: IsInputLock 상태에서 Space/E → `UIManager.AdvanceDialogue()` 호출
- 전투: AutoAttack 컴포넌트를 Inspector에서 추가 (코드 수정 불필요)

---

## 전투 시스템

### 전투 방식
- 근접 자동공격: 범위 내 가장 가까운 적을 쿨타임 기반으로 자동 타격
- 플레이어 + 동료 모두 동일한 AutoAttack 컴포넌트로 전투 참여
- 적 사망 시 QuestManager.ReportKill(enemyId) 호출로 Kill 퀘스트 연동

### IDamageable (인터페이스) - IDamageable.cs
- `void TakeDamage(int damage)`
- `bool IsDead { get; }`
- 역할: HP를 가진 모든 대상의 공통 인터페이스 (확장 시 플레이어 피격에도 사용 가능)

### EnemyController - EnemyController.cs
- 역할: 적 데이터 보유 + 실행 담당 (HP, 이동, 공격, 사망)
- 구현: IDamageable
- 필드:
  - `string _enemyId` (SerializeField, 퀘스트 _targetId와 매칭)
  - `int _maxHp` (SerializeField)
  - `int _currentHp` (런타임)
  - `EnemyFSM _fsm` (GetComponent)
  - `Rigidbody2D _rb` (GetComponent)
  - `Animator _animator` (GetComponent)
- 프로퍼티:
  - `string EnemyId` (읽기전용)
  - `bool IsDead` (읽기전용, _currentHp <= 0)
- 이벤트:
  - `Action<EnemyController> OnDeath`
- 주요 메서드:
  - `TakeDamage(int damage)` → HP 감소 → 0 이하 시 Die 직접 호출, 아니면 FSM.OnDamaged(currentHp)
  - `MoveTo(Vector2 direction)` → Rigidbody2D.velocity로 이동 (State에서 호출)
  - `StopMove()` → velocity 초기화 (State에서 호출)
  - `ApplyKnockback(Vector2 direction, float force)` → Impulse 넉백 (HitState에서 호출)
  - `Attack(Transform target)` → 공격 실행 (AttackState에서 호출, TODO: IDamageable.TakeDamage)
  - `Die()` → OnDeath 발행 → QuestManager.ReportKill(_enemyId) → Destroy
- 초기화: Awake에서 컴포넌트 참조, Start에서 FSM.Init(this) 호출
- 배치 방식: 맵 프리팹에 직접 배치, Inspector에서 _enemyId/_maxHp 설정
- 필수 컴포넌트: EnemyFSM, Rigidbody2D (Gravity Scale 0), "Enemy" 레이어

### EnemyFSM - EnemyFSM.cs
- 역할: FSM 상태 전환 관리자, 공유 데이터 보유 (MonoBehaviour)
- 필드:
  - `float _detectRange = 7f` (SerializeField, 감지 범위)
  - `float _attackRange = 1.5f` (SerializeField, 공격 범위)
  - `float _moveSpeed = 2f` (SerializeField)
  - `int _attackDamage = 3` (SerializeField)
  - `float _attackCooldown = 1.0f` (SerializeField)
  - `float _knockbackForce = 3f` (SerializeField)
  - `float _hitStunDuration = 0.3f` (SerializeField)
  - `List<Transform> _patrolPoints` (SerializeField, 선택적 순찰 경로)
  - `Vector3 OriginPosition` (Awake에서 저장, 읽기전용 프로퍼티)
  - `Transform Target` (플레이어, 읽기전용 프로퍼티)
  - `IEnemyState _currentState` (현재 활성 상태)
- 상태 인스턴스 (읽기전용 프로퍼티):
  - `IdleState`, `ChaseState`, `AttackState`, `HitState`, `ReturnState`
- 주요 메서드:
  - `Init(EnemyController controller)` → State 인스턴스 생성 (FSM+Controller 참조 전달), 플레이어 태그 Find, 초기 상태 Idle
  - `ChangeState(IEnemyState newState)` → Exit → 교체 → Enter
  - `OnDamaged(int currentHp)` → HP 0 이하면 무시(Controller가 Die 처리), 아니면 HitState 전환
  - `GetDistanceToTarget()` → 타겟과의 거리 반환 (State 공용)
- Update에서 `_currentState?.Update()` 호출 (switch문 없음)

### IEnemyState (인터페이스) - IEnemyState.cs
- `void Enter()` — 상태 진입 시 1회 호출
- `void Update()` — 상태 활성 중 매 프레임 호출
- `void Exit()` — 상태 퇴장 시 1회 호출

### EnemyStates - EnemyStates.cs (5개 상태 클래스 통합)
- 모든 State는 일반 C# 클래스 (MonoBehaviour 아님)
- 생성자에서 EnemyFSM + EnemyController 참조를 받음
- 참조 방향: State → FSM (ChangeState 요청, 공유 데이터 읽기), State → Controller (실행 요청)

#### EnemyIdleState (Idle + Patrol 통합)
- 순찰 포인트가 있으면 웨이포인트 순회, 없으면 제자리 대기
- 감지 범위에 플레이어 진입 시 → ChaseState 전환
- 순찰: 웨이포인트 도착 → 대기(1초) → 다음 포인트 (순환)

#### EnemyChaseState
- 플레이어 방향으로 이동
- 공격 범위 도달 시 → AttackState 전환
- 감지 범위 이탈 시 → ReturnState 전환

#### EnemyAttackState
- 쿨타임 기반 공격, 진입 즉시 첫 공격 가능
- Controller.Attack(target) 호출 (TODO: IDamageable 데미지 적용)
- 공격 범위 이탈 시 → ChaseState, 감지 이탈 시 → ReturnState

#### EnemyHitState
- Enter에서 넉백 적용 (플레이어 반대 방향)
- 경직 시간(_hitStunDuration) 동안 행동 불가
- 경직 해제 후 감지 범위 내면 → ChaseState, 밖이면 → ReturnState

#### EnemyReturnState
- 원래 위치(OriginPosition)로 이동
- 도착 시 → IdleState 전환
- 복귀 중 플레이어 재감지 시 → ChaseState 전환

### 적 AI 상태 흐름도
```
Idle/Patrol ──감지 범위 진입──→ Chase ──공격 범위 도달──→ Attack
    ↑                            ↓                         ↓
    └──── Return ←──감지 범위 이탈──┴─────감지 범위 이탈────┘
              ↑
        Hit (어떤 상태에서든 피격 시 진입) → 경직 해제 후 Chase 또는 Return
```

### 참조 방향 정리
```
EnemyController → EnemyFSM : OnDamaged 알림, Init 호출
EnemyFSM → EnemyStates : 현재 상태 Update 호출
EnemyStates → EnemyFSM : ChangeState 요청, 공유 데이터 읽기
EnemyStates → EnemyController : MoveTo, StopMove, Attack, ApplyKnockback 실행 요청
```
- FSM → Controller 직접 참조 없음 (피격 알림은 Controller→FSM 방향, 사망은 Controller가 직접 처리)

### AutoAttack - AutoAttack.cs
- 역할: 범위 내 적 자동 탐지 + 쿨타임 공격 (플레이어/동료 공용 컴포넌트)
- 필드:
  - `float _attackRange` (SerializeField, 기본 1.5f)
  - `int _attackDamage` (SerializeField, 기본 3)
  - `float _attackCooldown` (SerializeField, 기본 1.0f)
  - `LayerMask _enemyLayer` (SerializeField)
  - `float _lastAttackTime` (런타임)
- 주요 메서드:
  - `Update()` → InputLock 체크 → 쿨타임 체크 → FindClosestEnemy → Attack
  - `FindClosestEnemy()` → Physics2D.OverlapCircleAll로 범위 내 적 탐색 → 가장 가까운 적 반환
  - `Attack(EnemyController target)` → 쿨타임 갱신 → TakeDamage 호출
- Inspector 설정:
  - Player 오브젝트에 AutoAttack 추가 → _enemyLayer에 "Enemy" 레이어 지정
  - Companion 프리팹에 AutoAttack 추가 → 동일 설정
- IsInputLock 상태에서 공격 중단

---

## NPC 시스템

### NPCController - NPCController.cs
- 역할: 맵에 배치된 NPC의 상호작용 처리
- 상속: InteractionController
- 필드:
  - `string _npcId` (SerializeField, Inspector에서 ID 입력)
  - `NpcData _npcData` (런타임에 NpcDatabase에서 조회)
  - `string _npcName`
- 주요 메서드:
  - `Init()` → NpcDatabase.Instance.GetNpcById(_npcId)로 데이터 조회
  - `OnInteract()` → _npcData가 null이면 Init 호출 (지연 초기화) → 퀘스트 시작/대화 처리
- NPC 배치 방식: 맵 프리팹에 NPC 오브젝트 직접 배치, Inspector에서 _npcId만 입력
- 데이터(이름, 대화, 퀘스트)는 엑셀/NpcDatabase에서 관리

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

## 다이얼로그 시스템

### DialogueType (enum) - DialogueData.cs 상단 정의
- NPC: NPC 대화 (이름 표시)
- Monologue: 독백 (이름 표시, 플레이어 이름 등)
- System: 시스템 메시지 (이름 영역 숨김)

### DialogueLine (class) - DialogueData.cs 상단 정의
- `string _speakerName` (화자 이름)
- `DialogueType _dialogueType` (줄 단위 타입)
- `string _line` ([TextArea] 대사 텍스트)
- 역할: 한 줄의 대사 데이터, 줄마다 화자/타입 변경 가능 (NPC↔플레이어 교차 대화 지원)

### DialogueData (일반 클래스) - DialogueData.cs
- `string _dialogueId`
- `List<DialogueLine> _lines`

### DialogueUI - DialogueUI.cs
- 역할: 대화 UI 표시/숨김 + 대화 진행 제어 (줄 큐 관리, 타이핑 연출) 통합
- 필드:
  - `GameObject _dialoguePanel` (SerializeField, 대화창 패널)
  - `TextMeshProUGUI _dialogueText` (SerializeField, 대사 텍스트)
  - `TextMeshProUGUI _name` (SerializeField, 화자 이름 텍스트)
  - `GameObject _nameRoot` (SerializeField, 이름 영역 루트 — 없으면 _name.gameObject 폴백)
  - `Button _touchButton` (SerializeField, 전체화면 투명 버튼 — 터치/클릭 대화 넘기기용)
  - `float _typingSpeed = 0.05f` (SerializeField, 한 글자 출력 간격)
  - `Queue<DialogueLine> _lines` (대사 큐)
  - `Coroutine _typingCoroutine` (타이핑 코루틴 참조)
  - `string _fullText` (현재 줄 전체 텍스트)
  - `bool _isTyping` (타이핑 연출 진행 중 여부)
  - `bool _isDialogueRunning` (읽기전용 프로퍼티 IsDialogueRunning)
  - `bool IsVisible` (읽기전용 프로퍼티)
- 주요 메서드:
  - `StartDialogue(IEnumerable<DialogueLine> lines, Action onComplete)` → 큐 적재 → ShowNextLine
  - `AdvanceDialogue()` → 외부 입력 진입점 (PlayerController 키보드, 터치 버튼 공용) → HandleAdvance 호출
  - `ShowNextLine()` → 큐에서 DialogueLine Dequeue → Show(빈 텍스트) → TypeLine 코루틴 시작
  - `Show(string name, string line, DialogueType dialogueType)` → 패널/터치버튼 활성화, System이면 이름 영역 숨김 (private)
  - `TypeLine(string fullText)` → 코루틴, 한 글자씩 _dialogueText 갱신 (WaitForSeconds)
  - `HandleAdvance()` → _isTyping이면 CompleteTyping, 아니면 ShowNextLine (private)
  - `CompleteTyping()` → 코루틴 중단 → _fullText로 즉시 전체 표시 (private)
  - `OnDialogueClicked()` → 터치 버튼 onClick에서 호출 → AdvanceDialogue (private)
  - `EndDialogue()` → 코루틴 정리 → Hide → onComplete 콜백 (private)
  - `Hide()` → 패널/터치버튼 비활성화 (private)
- Awake에서 _touchButton.onClick에 OnDialogueClicked 리스너 등록 + Hide
- Update 없음 — 키보드 입력은 PlayerController가 담당, 터치 입력은 _touchButton이 담당
- 대화 넘기기 흐름:
  - 타이핑 중 입력 → 즉시 전체 표시 (CompleteTyping)
  - 타이핑 완료 후 입력 → 다음 줄 (ShowNextLine)
  - 마지막 줄 이후 입력 → 대화 종료 (EndDialogue)
- Inspector 설정:
  - _touchButton: Image(alpha=0), Raycast Target=ON, Stretch 전체화면, Button Transition=None
  - _touchButton은 _dialoguePanel과 함께 활성화/비활성화
  - DialogueUI가 속한 Canvas의 Sort Order를 조이스틱 Canvas보다 높게 설정

### 대화 호출 흐름
```
GameManager.StartDialogue(DialogueData, onComplete)
  → SetLockInput(true)
  → UIManager.StartDialogue(data._lines, onComplete)
    → DialogueUI.StartDialogue(lines, onComplete)
      → ShowNextLine() → Show() → TypeLine 코루틴
        → 입력(터치/키보드) → HandleAdvance()
          → 타이핑 중 → CompleteTyping()
          → 타이핑 완료 → ShowNextLine() 또는 EndDialogue()
            → EndDialogue() → Hide() → onComplete
              → GameManager.EndDialogue() → SetLockInput(false)
```

---

## 데이터 파싱 시스템 (엑셀 → JSON → 게임 데이터)

### 파일 구조
```
Assets/
├── Data/
│   └── Excel/                    ← 엑셀 원본 (.xlsx, 에디터 전용)
│       ├── DialogueTable.xlsx
│       ├── QuestTable.xlsx
│       ├── RewardTable.xlsx
│       ├── NpcTable.xlsx
│       └── CompanionTable.xlsx
├── Resources/
│   └── Json/                     ← 자동 생성되는 JSON (빌드 포함)
├── Plugins/
│   └── Editor/
│       └── EPPlus.dll            ← 엑셀 파싱 라이브러리 (Editor Only, v4.5.3.3 LGPL)
└── Scripts/
    ├── Data/
    │   ├── TableData.cs          ← 모든 테이블 데이터 클래스 통합
    │   └── DataManager.cs        ← JSON 로드 + Database 생성/분배
    └── Editor/
        ├── ExcelToJsonConverter.cs  ← 엑셀 → JSON 변환 + 필드명 검증
        └── ExcelPostprocessor.cs    ← 엑셀 변경 감지 → 자동 변환
```

### 에디터 타임 흐름
```
엑셀 수정 → Unity로 돌아옴
  → ExcelPostprocessor(AssetPostprocessor)가 Excel/ 폴더 변경 감지
    → ExcelToJsonConverter.ConvertAll() 자동 실행
      → 첫 행 변수명과 C# TableData 클래스 필드명 리플렉션 비교 검증
      → 불일치 시 Console에 경고
      → JSON 파일 생성 (Resources/Json/ 폴더에 저장)
      → enum 값은 StringEnumConverter로 문자열 저장 ("NPC", "Talk" 등)
수동 변환: Unity 메뉴 → Tools → Convert All Excel to JSON
```

### 런타임 흐름
```
로딩 씬 (SceneLoader.LoadSceneRoutine)
  → 매니저 Awake/Start 완료 대기
  → DataManager.LoadAllDataAsync() 호출
    → Database 인스턴스 생성 (CreateInstance)
    → JSON 파일 로드 (Resources.Load<TextAsset>)
    → Newtonsoft JsonConvert로 List<TableData> 역직렬화
    → 각 Database에 ApplyData로 전달
      → Database가 자체 가공 (그룹핑, 매칭 등) → Dictionary 적재
  → Main 씬 진입
```

### TableData 클래스 (TableData.cs 통합 파일)
- `DialogueTableData`: uniqueId, dialogueId, speakerName, dialogueType(enum), line
- `QuestTableData`: uniqueId, title, description, goalType(enum), goalCount, questGiverNpcId, targetId, startDialogueId, progressDialogueId, completedDialogueId, nextQuestId, rewardGroupId
- `RewardTableData`: uniqueId, rewardGroupId, gold, exp, companionId, itemId, itemCount
- `NpcTableData`: uniqueId, npcName, defaultDialogueId, questId
- `CompanionTableData`: uniqueId, companionName, skinName, companionPrefabPath, followSpeed, followDistance

### ExcelToJsonConverter - ExcelToJsonConverter.cs (#if UNITY_EDITOR)
- 역할: 엑셀 → JSON 변환 + 필드명 검증
- `TableTypeMap`: Dictionary<string, Type> — 엑셀 파일명과 TableData 클래스 매핑, 새 테이블 추가 시 여기에 등록
- `ConvertAll()` → [MenuItem("Tools/Convert All Excel to JSON")], Excel 폴더 순회 → ConvertExcel 호출
- `ConvertExcel()` → EPPlus로 엑셀 로드 → 첫 행 변수명 추출 → ValidateHeaders → 행 단위 파싱 → JSON 저장
- `ConvertValue()` → 셀 값을 C# 필드 타입(string/int/float/bool/enum)에 맞게 변환, enum은 대소문자 무시
- `ValidateHeaders()` → 엑셀 칼럼명 ↔ C# 클래스 필드명 양방향 검증, 불일치 시 경고
- 라이브러리: EPPlus 4.5.3.3 (LGPL, 에디터 전용, 빌드 미포함)

### ExcelPostprocessor - ExcelPostprocessor.cs (#if UNITY_EDITOR)
- 역할: Assets/Data/Excel/ 폴더의 .xlsx 변경 감지 → ConvertAll 자동 실행
- AssetPostprocessor 상속, Unity Editor 시작 시 자동 등록

---

## 데이터베이스 (일반 C# 클래스 싱글톤)

Database는 MonoBehaviour가 아닌 일반 C# 클래스. Hierarchy 배치 불필요. DataManager가 CreateInstance로 생성.

### DialogueDatabase - DialogueDatabase.cs
- 역할: DialogueData 조회
- `CreateInstance()` → static, Instance 생성
- `ApplyData(List<DialogueTableData>)` → dialogueId로 그룹핑 → DialogueData 생성 → Dictionary 적재
- `GetDialogueById(string dialogueId)` → DialogueData

### QuestDatabase - QuestDatabase.cs
- 역할: QuestData 조회
- `CreateInstance()` → static, Instance 생성
- `ApplyData(List<QuestTableData>, List<RewardTableData>)` → rewardGroupId로 보상 그룹핑 → QuestData 생성 + 보상 매칭 → Dictionary 적재
- `GetQuestByID(string questId)` → QuestData
- `GetAllQuests()` → IEnumerable<QuestData>

### NpcDatabase - NpcDatabase.cs
- 역할: NpcData 조회
- `CreateInstance()` → static, Instance 생성
- `ApplyData(List<NpcTableData>)` → NpcData 생성 → Dictionary 적재
- `GetNpcById(string npcId)` → NpcData

### CompanionDatabase - CompanionDatabase.cs
- 역할: CompanionData 조회
- `CreateInstance()` → static, Instance 생성
- `ApplyData(List<CompanionTableData>)` → CompanionData 생성 → Dictionary 적재
- `GetCompanionById(string companionId)` → CompanionData

---

## 데이터 클래스 (일반 C# 클래스)

### QuestData - QuestData.cs
- `string _questId`, `string _questGiverNpcId`, `string _title`, `string _description`
- `string _startDialogueId`, `string _progressDialogueId`, `string _completedDialogueId`
- `string _targetId`, `QuestGoalType _goalType`, `int _goalCount`
- `string _rewardGroupId`, `List<QuestReward> _rewards`
- `string _nextQuestId`

### QuestState (런타임 상태)
- `QuestData _data`, `int _currentProgress`, `bool _isCompleted`
- `AddProgress(int amount = 1)` → bool (완료 여부 반환, 이미 완료면 false)

### QuestReward - QuestManager.cs 내부 정의
- `int _gold`, `int _exp`, `string _companionId`, `string _itemId`, `int _itemCount`

### QuestGoalType (enum) - QuestData.cs 상단 정의
- None, Kill, Collect, Talk, Explore, AcquireItem

### NpcData - NpcData.cs
- `string _npcId`, `string _npcName`, `string _defaultDialogueId`, `string _questId`

### CompanionData - CompanionData.cs
- `string _companionId`, `string _companionName`, `string _skinName`
- `string _companionPrefabPath` (Resources 기준 경로, 런타임에 Resources.Load로 프리팹 로드)
- `float _followSpeed`, `float _followDistance`

### PlayerInfo (struct)
- `string _name`, `int _level`, `int _exp`, `int _gold`

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
  - `OnPointerDown` → IsInputLock 체크 → 터치 위치에 _joystickRoot 배치 후 활성화
  - `OnDrag` → 핸들 위치 계산(ClampMagnitude) → 방향값 정규화 → SetJoystickInput 호출
  - `OnPointerUp` → _joystickRoot 비활성화, 핸들 초기화, SetJoystickInput(Vector2.zero)
- 플랫폼 처리: `#if !UNITY_WEBGL && !UNITY_ANDROID && !UNITY_IOS` → gameObject.SetActive(false)
- Inspector 설정:
  - JoystickArea: Image(alpha=0), Raycast Target=ON, 전체화면 RectTransform
  - JoystickRoot: 기본 비활성, Background(반투명 원) + Handle(작은 원) 자식으로 구성
- 주의: 부모 Canvas의 Render Mode가 반드시 **Screen Space - Overlay**여야 함
- 주의: 대화 중 조이스틱 비주얼이 뜨지 않도록 OnPointerDown에서 `GameManager.Instance.IsInputLock` 체크 추가 권장

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
- 새 게임 시: Main 씬 로드 → 매니저 Awake 대기 → DataManager.LoadAllDataAsync → LoadFirstMap
- 이어하기 시: `GameSystem.Load()` → Main 씬 로드 → DataManager.LoadAllDataAsync → `GameManager.LoadGame(data)` 호출
- 로딩 순서: 로딩UI 표시 → 세이브 로드(이어하기) → 씬 비동기 로드 → 씬 활성화 → 매니저 대기 → **게임 데이터 로드 (DataManager)** → 세이브 적용(이어하기) → 완료

### LoadingUI
- 역할: 로딩바 + 퍼센트 텍스트 + 상태 텍스트
- SceneLoader 하위 Canvas (Sort Order 100)

### TitleUI
- 역할: 시작화면 버튼 (시작/이어하기/옵션/끝내기)
- Start()에서 `GameSystem.Exists()`로 이어하기 버튼 interactable 결정
- OnQuit() → GameManager.SaveGame() → Application.Quit()

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
| 15 | 엑셀 → 게임 데이터 파싱 시스템 | ✅ 완료 |
| 16 | 다이얼로그 시스템 확장 (독백/시스템 메시지/타이핑 연출/터치 입력) | ✅ 완료 |
| 17 | ReportKill/ReportReach 완성 + 전투 시스템 기초 | ✅ 완료 |
| 17-1 | 적 AI FSM (Idle/Chase/Attack/Hit/Return) | ✅ 완료 |
| 18 | ReportCollect/AcquireItem 추가 | 미구현 |
| 19 | AudioManager (BGM/SFX) + 옵션 UI 연동 | 미구현 |
| 20 | 인벤토리 아이템 획득/사용/장착 + _itemID 보상 지급 | 미구현 |
| 21 | Resource.Load 방식을 Addressable 시스템으로 변경 | 미구현 |

---

## 설계 원칙
- 이벤트는 매니저에 선언, UI가 구독
- UI 슬롯은 매니저 직접 참조 안 함 → 부모 UI가 콜백으로 중개
- DontDestroyOnLoad 매니저는 현재 Main 씬에 배치 (나중에 Boot 씬 분리 가능)
- **DataManager만 Main 씬 Hierarchy에 배치** (MonoBehaviour, 코루틴 사용)
- **Database는 일반 C# 클래스 싱글톤** — DataManager가 CreateInstance로 생성, Hierarchy 배치 불필요
- MonoBehaviour 사용 기준: Unity 엔진 기능(렌더링, 물리, 코루틴, Inspector)이 필요한 경우만
- Save/Load 진입점은 GameManager, 실제 파일 I/O는 GameSystem(static 유틸)에 위임
- 저장 대상 클래스는 필드명과 타입을 MD에 기록 (파일 재요청 방지)
- 런타임 클래스(QuestState 등)는 직렬화 전용 클래스로 분리 (QuestData 등 참조 타입 제거)
- 게임 데이터(불변)는 엑셀 → JSON → Database 경로로 관리, ScriptableObject 미사용
- TableData 클래스는 엑셀 행과 1:1 매핑 (원본), 게임용 데이터 클래스는 Database가 가공하여 생성
- 새 테이블 추가 시: TableData.cs에 클래스 추가 → ExcelToJsonConverter.TableTypeMap에 등록 → DataManager.LoadAllDataAsync에 로드 코드 추가 → Database 구현
- 플랫폼 분기는 #if 전처리기 사용, 조이스틱은 PC에서 자동 비활성화
- 매니저 간 단순 메서드 호출은 허용, 내부 데이터 직접 조작은 금지
- NPC는 맵 프리팹에 직접 배치, Inspector에서 _npcId만 입력, 데이터는 NpcDatabase에서 지연 초기화로 조회
- 프리팹 참조가 필요한 경우 Resources 경로 문자열로 관리 (CompanionData._companionPrefabPath)
- 전투는 AutoAttack 컴포넌트 기반 — 플레이어/동료 코드 수정 없이 Inspector에서 추가
- 퀘스트 목표 대상 매칭은 _targetId 필드로 통일 (Talk=NPC ID, Kill=Enemy ID, Explore=Location ID)
- 적은 맵 프리팹에 직접 배치, "Enemy" 레이어 필수
- 적 AI는 FSM 패턴 사용 — EnemyController(데이터+실행) + EnemyFSM(상태 관리) + EnemyStates(행동 판단) 분리
- FSM → Controller 직접 참조 금지, State가 Controller 실행 메서드를 호출하는 구조
- State는 일반 C# 클래스 (MonoBehaviour 아님), FSM이 new로 생성

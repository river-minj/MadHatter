# CLAUDE.md

이 파일은 이 저장소에서 작업할 때 Claude Code(claude.ai/code)에게 제공되는 가이드입니다.

## 프로젝트 개요

**MadHatter**는 Unity 2022.3.55f1로 제작된 2D 탑다운 RPG입니다. 4방향 이동, 적 AI 전투, 동료 추종 시스템, 퀘스트, 인벤토리, 대화 기능을 갖추고 있으며 세이브/로드 및 모바일(조이스틱) 지원도 포함합니다.

## 빌드 및 개발

Unity Hub에서 Unity 2022.3.55f1로 프로젝트를 열어야 합니다. CLI 빌드 명령은 없으며 Unity 에디터를 사용합니다. 스크립트는 `Assets/Script/`에 위치합니다.

WebGL 빌드: Unity 에디터 내 **Tools > Build WebGL (720x1280)** 메뉴 사용 (`Assets/Script/Editor/WebGLBuilder.cs`). Gzip 압축 + Decompression Fallback 설정으로 GitHub Pages 호환.

데이터 테이블을 수정한 후 JSON으로 변환하려면: Unity 에디터 내 **Tools > Excel To Json Converter** 도구를 사용합니다.

## 아키텍처

### 씬 흐름

```
Start 씬 → (비동기 로드) → Main 씬
```

`SceneLoader`(DontDestroyOnLoad)가 비동기 로딩을 주도하며 `LoadingUI`를 표시한 후:
1. `DataManager`가 모든 JSON 테이블을 Database 싱글턴에 로드
2. 이어하기 시: `GameSystem.Load()`로 `saveData.json`을 읽고 `GameManager.LoadGame()`으로 적용
3. 첫 번째 맵 프리팹 인스턴스화

### 매니저 레이어 (싱글턴, DontDestroyOnLoad)

모든 매니저는 `public static Instance` 싱글턴 패턴을 따릅니다:

| 매니저 | 역할 |
|---|---|
| `GameManager` | 전역 총괄: 입력 잠금, 맵 전환, 대화 흐름, 세이브 트리거 |
| `UIManager` | 모든 UI의 허브: 페이드 전환, NPC 프롬프트, 대화, 팝업 |
| `DataManager` | JSON 비동기 로드 → Database 싱글턴에 저장 |
| `GameSystem` | 파일 I/O — `Application.persistentDataPath/saveData.json`에 JSON 직렬화 |
| `PlayerInfoManager` | 플레이어 스탯: HP, EXP, 레벨, 골드, ATK(기본값 + 장착 무기 보너스) |
| `InventoryManager` | 아이템 수집, 장착/사용, `OnInventoryChanged` / `OnEquipChanged` 이벤트 발행 |
| `QuestManager` | 퀘스트 상태 머신: 시작 → 수락 → 진행 → 완료 → 보상 |
| `CompanionManager` | 보유 동료 목록, 소환, 2열 대형, 스케일 전파 |
| `ShopManager` | 상점 재고 상태(`_stockMap`) 관리, 구매 트랜잭션 처리, 유한 재고 Save/Load |

### 데이터 파이프라인

```
Excel (외부) → ExcelToJsonConverter (에디터 도구)
  → Assets/Resources/Json/*.json
  → DataManager.LoadAllDataAsync()
  → Database 싱글턴 (non-MonoBehaviour, CreateInstance 패턴)
  → 매니저/컨트롤러에서 public 메서드로 접근
```

Database 싱글턴 목록: `DialogueDatabase`, `QuestDatabase`, `NpcDatabase`, `ItemDatabase`, `CompanionDatabase`, `DropDatabase`, `ShopDatabase`

`GameDatabase`는 위 7개 Database의 단일 진입점 파사드(Facade)입니다. 기존 싱글턴에 위임하며 동작은 동일합니다. 신규 코드에서는 `GameDatabase.Instance.Items.GetItem(...)` 패턴을 사용합니다. `DataManager.LoadAllDataAsync()` 완료 직후 `GameDatabase.Initialize()`가 호출되어 인스턴스가 생성됩니다.

### 맵 시스템

맵은 전환 시 프리팹을 인스턴스화/파괴하는 방식입니다. `MapController`(기반 클래스)가 정의하는 것:
- 스폰 포인트 (Default, Left, Right, Up, Down)
- 맵별 플레이어 스케일 및 이동 속도
- `CameraController` 클램핑을 위한 카메라 경계
- 서브클래스 커스터마이징용 가상 메서드 `OnMapEnter()` / `OnMapExit()`

맵 전환 흐름: `MapController.RequestMapTransition()` → `GameManager`가 입력 잠금 → 페이드 → 기존 맵 파괴 → 새 맵 인스턴스화 → 플레이어/동료 재배치 → 자동 세이브

### 동료 추종 시스템

`PlayerTrailRecorder`가 플레이어 이동 위치를 0.15 유닛 간격으로 원형 버퍼(최대 1000개)에 기록합니다. `CompanionController`는 이 버퍼를 설정된 간격으로 읽어 동일한 경로를 따라 이동합니다. 동료는 A/B 두 열로 나뉘며 맵 전환 시 `CompanionManager`를 통해 플레이어 기준 상대 스케일이 업데이트됩니다.

### 적 FSM

`EnemyFSM`은 `IEnemyState` 객체를 사용합니다: `Idle → Chase → Attack → Hit → Return → Die`. 상태 전환은 탐지 범위, 공격 범위, 넉백에 의해 결정됩니다. `EnemyController`는 사망 시 `QuestManager.ReportKill()`을 호출하고 `DropDatabase`로 아이템 드롭을 처리합니다.

### 퀘스트 시스템

퀘스트 목표 유형: Kill, Collect, Talk, Explore. 진행 보고 경로:
- `EnemyController.OnDeath()` → `QuestManager.ReportKill(enemyId)`
- `NPCController.OnInteract()` → `QuestManager.ReportTalkToNPC(npcId)`
- `InventoryManager.AddItem()` → `QuestManager.ReportCollect(itemId, count)` (로드 복원 시 _isLoading 플래그로 차단)
- `QuestLocationTrigger.OnTriggerEnter2D()` → `QuestManager.ReportReach(locationId)`

`_nextQuestId`를 통해 퀘스트를 연쇄 구성할 수 있습니다. 보상으로 골드, EXP, 아이템, 동료 해금이 지급됩니다.

### UI 아키텍처

`TabController`가 탭 상태(Normal/Selected/Locked)와 연결된 `TabPage` GameObject를 관리합니다. `InfiniteScrollView`는 인벤토리와 퀘스트 UI에서 리스트를 가상화합니다. 팝업(`CommonConfirmPopup`, `ItemDetailPopup`)은 sort order로 다른 UI 레이어 위에 쌓입니다.

### 입력

Legacy Input Manager 사용. 키보드: WASD/방향키(이동), E(상호작용/대화 진행), I(인벤토리), Q(퀘스트 UI). `JoystickUI`는 모바일/WebGL용 가상 조이스틱을 제공하며 데스크탑에서는 자동 비활성화됩니다.

### 자동 세이브 트리거

다음 시점에 자동 저장이 실행됩니다: 맵 전환 완료, 퀘스트 보상 수령, 인벤토리 변경, 장비 변경

## 주요 패턴

- **싱글턴 매니저** — `Awake()`에서 `public static Instance` 설정
- **이벤트 델리게이트** — `OnInventoryChanged`, `OnEquipChanged`로 UI 업데이트 및 세이브 연동
- **설정 기반 구조** — Inspector의 직렬화 필드 + JSON 데이터 테이블 사용, 하드코딩 지양
- **템플릿 메서드** — `MapController`, `InteractionController`가 서브클래스용 가상 훅 제공
- **IAnimator 인터페이스** — Spine(`SpineAnimator`)과 스프라이트 시트(`SpriteAnimator`) 애니메이션 추상화

## 설계 원칙

- 이벤트는 매니저에 선언, UI가 구독
- UI 슬롯은 매니저 직접 참조 안 함 → 콜백(Action)으로 중개
- UI 슬롯은 부모 UI(InventoryUI 등)도 직접 참조 안 함 → Action 콜백으로 클릭 전달
- DontDestroyOnLoad 매니저는 현재 Main 씬에 배치 (나중에 Boot 씬 분리 가능)
- DataManager만 Main 씬 Hierarchy에 배치 (MonoBehaviour, 코루틴 사용)
- Database는 일반 C# 클래스 싱글톤 — DataManager가 CreateInstance로 생성, Hierarchy 배치 불필요
- MonoBehaviour 사용 기준: Unity 엔진 기능(렌더링, 물리, 코루틴, Inspector)이 필요한 경우만
- Save/Load 진입점은 GameManager, 실제 파일 I/O는 GameSystem(static 유틸)에 위임
- LoadGame은 반드시 DataManager 로드 완료 후 호출 (Database 인스턴스 필요, InventorySlot 등에서 참조)
- LoadGame 시 ApplyData 호출 순서 주의: 이벤트 기반 자동저장 구독이 있는 매니저(Inventory 등)는 다른 매니저 복원 이후에 호출, 현재 순서: - PlayerInfo → Quest → Companion → Inventory
- GameManager.Start() SceneLoader 분기: SceneLoader 존재 → 정상 흐름, 미존재 → DevModeInit 코루틴 (개발 모드)
- 저장 대상 클래스는 필드명과 타입을 MD에 기록 (파일 재요청 방지)
- 런타임 클래스(QuestState 등)는 직렬화 전용 클래스로 분리 (QuestData 등 참조 타입 제거)
- 게임 데이터(불변)는 엑셀 → JSON → Database 경로로 관리, ScriptableObject 미사용
TableData 클래스는 엑셀 행과 1:1 매핑 (원본), 게임용 데이터 클래스는 Database가 가공하여 생성
- 새 테이블 추가 시: TableData.cs에 클래스 추가 → ExcelToJsonConverter.TableTypeMap에 등록 → DataManager.LoadAllDataAsync에 로드 코드 추가 → Database 구현 → GameDatabase에 프로퍼티 추가
- Database 접근은 GameDatabase.Instance.XXX 패턴을 신규 코드 기준으로 사용 — 기존 Database.Instance 직접 참조는 점진적으로 교체, 과도기 중 혼재 허용
- 플랫폼 분기는 #if 전처리기 사용, 조이스틱은 PC에서 자동 비활성화
- 매니저 간 단순 메서드 호출은 허용, 내부 데이터 직접 조작은 금지
- NPC는 맵 프리팹에 직접 배치, Inspector에서 _npcId만 입력, 데이터는 NpcDatabase에서 지연 초기화로 조회
- NPC 상호작용 우선순위: Talk 퀘스트 타겟 → 퀘스트 제공자 → 상점(_shopId) → 기본 대화
- 상점 NPC는 NpcTableData._shopId에 shopId 입력, ShopDatabase에서 상품 목록 조회
- ShopManager는 유한 재고(_stockMap)만 저장 — 무한 재고(-1)는 저장 불필요, 엑셀 값 그대로 사용
- ShopManager 저장 트리거는 OnShopStockChanged 이벤트로 통일 (GameManager가 구독) — 다른 매니저와 동일한 패턴
- ReadOnlyAttribute는 PropertyAttribute 상속이므로 #if UNITY_EDITOR 없이 정의, DrawerOnly가 에디터 전용
- 프리팹 참조가 필요한 경우 Resources 경로 문자열로 관리 (CompanionData._companionPrefabPath)
팝업은 프리팹 로드 방식 — UIManager가 Instantiate, 사용처가 SetPopup으로 데이터 세팅
- 전투는 AutoAttack 컴포넌트 기반 — 플레이어/동료 코드 수정 없이 Inspector에서 추가
- ATK의 단일 소유자는 PlayerInfoManager (기본 _baseAtk + 장비 보정 합산)
- AutoAttack.GetFinalDamage(): Player → PlayerInfoManager.Atk 참조, 동료 → Inspector _attackDamage 사용
- HudUI는 독립 동작 (UIManager 미등록, 항시 표시, Start/OnDestroy에서 이벤트 구독/해제)
- 퀘스트 목표 대상 매칭은 _targetId 필드로 통일 (Talk=NPC ID, Kill=Enemy ID, Explore=Location ID)
- 적은 맵 프리팹에 직접 배치, "Enemy" 레이어 필수
- 적 AI는 FSM 패턴 사용 — EnemyController(데이터+실행) + EnemyFSM(상태 관리) + EnemyStates(행동 판단) 분리
- FSM → Controller 직접 참조 금지, State가 Controller 실행 메서드를 호출하는 구조
- State는 일반 C# 클래스 (MonoBehaviour 아님), FSM이 new로 생성
- 애니메이션 재생/방향전환은 SpineAnimator 공용 컴포넌트에 위임 — 개별 Controller에 Spine 코드 직접 작성 금지
- 애니메이션 이름은 범용으로 통일 (idle, run, attack, hit, die) — 향후 Spine→Sprite 교체 대비
- SpineAnimator.Skeleton 프로퍼티로 스킨 변경 등 특수 접근 허용 (CompanionController 등)
- 탭 시스템은 범용 컴포넌트(TabController/Tab/TabPage)로 분리, 데이터 로직은 사용처(InventoryUI 등)가 담당
- 드롭 테이블은 엑셀 데이터로 관리, 가중치 기반 랜덤 선택
- 모든 키보드 입력은 PlayerController에서 통합 관리 — InteractionTrigger 등 개별 컴포넌트에서 키 입력 감지 금지
- 인터랙션 감지와 실행의 분리: InteractionTrigger는 범위 감지만 담당 (SetInteractable/ClearInteractable), 실행(Interact 호출)은 PlayerController가 E키로 처리
- IInteractable 인터페이스로 다형적 인터랙션: PlayerController는 상대가 NPC/DroppedItem/상자 등 무엇인지 모르고 Interact()만 호출, 새 인터랙션 대상 추가 시 PlayerController 수정 불필요
- 팝업 중복 방지: UIManager._currentPopup으로 1개만 허용, CommonConfirmPopup.ClosePopup에서 ClearCurrentPopup 호출
- 대화~팝업 InputLock 세션 관리: StartDialogue의 onComplete 유무로 분기 — onComplete 없으면 자동 EndDialogue, 있으면 호출측(QuestManager 등)이 EndDialogue 책임
- 퀘스트 대화 역할 분리: startDialogueId/progressDialogueId/completedDialogueId는 giver NPC 전용, targetDialogueId는 target NPC 전용
- ClaimReward는 보상만 지급: completedDialogue 재생과 nextQuest 자동 시작을 하지 않음, NPC에게 직접 가야 완료 대사와 다음 퀘스트 진행
- 체인 퀘스트 giver 제한: NPCController 체인 탐색 시 questGiverNpcId != _npcId이면 체인 중단, 다른 NPC의 퀘스트를 주지 않음
- 디자인 패턴 적용 기준: 런타임 교체가 필요 없으면 전략 패턴을 쓰지 않음, 인터페이스 기반 컴포넌트 조합으로 충분
- 맵 제작은 다중 Tilemap 레이어 구조: Ground(시각) + Road(시각) + Walls(충돌 전용, 렌더러 비활성) + Decoration(플레이어 위 장식), MapBounds는 카메라 제한 전용이며 이동 제한은 Walls가 담당
- 맵별 캐릭터 스케일: MapController._playerScale로 관리, ChangeMap에서 플레이어 + 동료에 일괄 적용
- 동료 스케일은 비율 기반: CompanionController._scaleRatio로 프리팹 원본 스케일과 플레이어 스케일의 비율을 저장, 맵 전환 시 비율 유지 (동료마다 개별 비율)
- 맵 제작 방식은 2가지 공존: 타일맵 방식(맵 1)과 배경 이미지+콜라이더 방식(맵 2), 맵 특성에 따라 선택
- 카메라 고정 맵: MapBounds 크기를 카메라 뷰보다 작게 설정하면 카메라 이동 없이 고정
- 인벤토리 UI 디자인 방향: 판타지 RPG풍, 하단 절반 패널, 장비/소비 탭은 그리드형(보유 아이템만 표시), 동료 탭은 컬렉션/도감 스타일(획득/미발견/잠금 3상태)
- InfiniteScrollView 풀링 정책: 매 SetData마다 풀 사이즈를 Min(theoretical_pool, dataCount)로 동기화 (Instantiate/Destroy 양방향). theoretical_pool은 뷰포트 크기 기반 상한 ((visibleRows + 2 buffer) × columns)이라 데이터가 아무리 많아도 일정 크기 이상 늘지 않음. "풀은 줄지 않는다(레이지 풀)" 안도 검토했으나 다음 이유로 현재 방식 채택:
- 
- invariant가 단순함 ("풀 사이즈 = Min(theoretical, dataCount)")
- theoretical_pool이 작아서 (~20) Instantiate/Destroy 비용이 hitch로 체감될 정도가 아님
- 인벤토리 슬롯 프리팹이 가벼움 (Image + TMP + Button)
- 탭 전환이 빈번한 작업이 아님
- 조기 최적화 회피 — hitch가 실제 체감되면 그때 레이지 풀로 전환
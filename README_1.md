# MadHatter — 포트폴리오 상세 설명

> Unity 2022.3 / C# / 1인 개발 / 개발 중

---

## 이 문서를 쓰는 이유

GitHub 링크만 드리면 코드가 "왜 이렇게 생겼는지"가 전달되지 않습니다.  
설계 의도, 선택한 이유, 아쉬운 부분까지 직접 설명하는 것이 맞다고 생각해 작성했습니다.

---

## 개발 배경

아웃게임 UI/시스템 개발을 8년간 해오면서, 실무에서는 대부분 **이미 설계된 프레임워크 위에 컨텐츠를 붙이는 역할**을 했습니다.

이 프로젝트는 그 반대 방향의 시도입니다.  
**프레임워크 자체를 직접 설계하고**, 그 위에 아웃게임(인벤토리, 퀘스트, 대화)과 인게임(전투, AI, 맵)을 모두 올려보는 것이 목표였습니다.

---

## 구조 개요

```
Start 씬 → (비동기 로드) → Main 씬

SceneLoader (DontDestroyOnLoad)
  └─ DataManager: JSON → Database 싱글턴 적재
  └─ GameManager.LoadGame(): SaveData → 각 Manager에 주입

Manager Layer (싱글턴, DontDestroyOnLoad)
  GameManager / UIManager / InventoryManager / QuestManager
  PlayerInfoManager / CompanionManager / DataManager

Game Logic Layer
  PlayerController / EnemyController / MapController
  NpcController / CompanionController / CameraController

Database Layer
  ItemDatabase / QuestDatabase / NpcDatabase / DialogueDatabase
  DropDatabase / CompanionDatabase

Data Layer
  *Data (불변 기획 데이터) / *State (런타임 상태) / SaveData
```

---

## 설계 결정과 선택 이유

### 1. 싱글톤 매니저 구조

**선택한 이유**  
1인 개발에서 빠른 이터레이션이 우선이었습니다. DI 프레임워크(Zenject 등) 도입은 초기 구성 비용이 크고, 이 규모에서 얻는 이득이 비용보다 작다고 판단했습니다.

**실제로 발생한 문제**  
- `InventoryManager.UseItem()` → `PlayerInfoManager.AddHp()` 호출
- `PlayerInfoManager.Atk` → `InventoryManager.GetEquippedWeapon()` 호출

두 매니저가 서로를 직접 참조하는 순환 구조가 생겼습니다.  
아이템 사용 효과를 이벤트로 발행하고 PlayerInfoManager가 구독하는 형태로 분리했어야 한다고 판단하고 있습니다.

**팀 프로젝트였다면**  
매니저마다 인터페이스를 두고 인터페이스에 의존하는 구조로 시작했을 것입니다.  
Unity에서 생성자 주입이 불가능한 만큼 `Initialize(IDependency dep)` 형태의 수동 주입이나 VContainer 같은 경량 DI를 선택했을 것입니다.

---

### 2. 데이터 파이프라인

```
Excel → ExcelToJsonConverter (에디터 툴) → JSON
  → DataManager.LoadAllDataAsync() → Database 싱글턴
  → Manager/Controller에서 접근
```

ScriptableObject 대신 JSON + 에디터 변환 툴을 선택한 이유는 **기획자가 Excel을 직접 수정할 수 있는 환경**을 유지하기 위해서입니다.  
TableData 클래스가 Excel 행과 1:1 매핑되어 필드명이 양방향으로 검증됩니다.

Database는 순수 C# 클래스(non-MonoBehaviour)로 DataManager가 `CreateInstance()`로 생성합니다.  
MonoBehaviour를 쓰지 않는 이유는 **Unity 생명주기에 묶이지 않기** 위해서입니다.

---

### 3. 적 AI — FSM 분리 구조

```
EnemyController  — 데이터 보유 + 실행 메서드 제공
EnemyFSM         — 상태 전환 판단
EnemyState       — 각 상태별 행동 (순수 C# 클래스, new로 생성)
```

State가 Controller를 직접 참조하는 것을 허용했습니다.  
FSM → Controller 단방향으로 제한하고, Controller는 FSM을 모릅니다.  
State를 MonoBehaviour가 아닌 순수 C# 클래스로 만든 덕분에 상태 객체 자체는 Unity 없이 단독으로 테스트 가능한 구조입니다.

---

### 4. UI 설계 원칙

- **슬롯은 매니저를 직접 모릅니다.** 클릭 이벤트는 Action 콜백으로 상위 UI에 위임합니다.
- **InfiniteScrollView** 는 데이터 타입을 모릅니다. `InfiniteScrollData` 추상 클래스와 `InfiniteScrollCell<T>` 제네릭으로 인벤토리·퀘스트 모두 같은 컴포넌트를 재사용합니다.
- **EmptyStateViewer** 는 InventoryUI를 모릅니다. 부모 컨테이너의 활성 자식 수만 감시하는 독립 컴포넌트로, 어떤 리스트에도 붙일 수 있습니다.
- **탭 시스템(TabController/Tab/TabPage)** 은 인벤토리·퀘스트 등 특정 컨텐츠와 무관한 범용 컴포넌트입니다.

---

### 5. 저장/로드 설계

```
진입점: GameManager.SaveGame() / LoadGame()
파일 I/O: GameSystem (static 유틸)
```

각 매니저가 `GetSaveData()` / `ApplyData()` 를 구현하고, GameManager가 수집·주입합니다.  
`ApplyData` 호출 순서가 중요합니다. InventoryManager는 다른 매니저 복원 이후에 호출해야 합니다.  
이벤트 기반 자동 저장 구독이 InventoryManager에 걸려 있어, 먼저 복원하면 복원 도중 불필요한 저장이 발생하기 때문입니다.

이 의존 순서가 코드에 명시되지 않고 주석으로만 남아 있는 점은 아쉬운 부분입니다.  
복원 단계를 명시적인 단계 객체로 분리하거나, 최소한 enum으로 순서를 강제하는 편이 나았을 것입니다.

---

## 스스로 평가하는 아쉬운 부분

| 문제 | 원인 | 개선 방향 |
|------|------|----------|
| 매니저 간 순환 참조 | 직접 싱글톤 호출 | 이벤트 발행으로 분리 |
| 게임 로직이 매니저에 직접 의존 | 인터페이스 없음 | 매니저 인터페이스화 + 주입 |
| LoadGame 호출 순서가 암묵적 | 순서 강제 메커니즘 없음 | 복원 단계 명시화 |
| 단위 테스트 불가 | 싱글톤 전역 의존 | 핵심 로직의 Unity 비종속 분리 |

---

## 잘 됐다고 생각하는 부분

**데이터와 로직의 분리**  
불변 기획 데이터(Database)와 런타임 상태(Manager)가 명확히 나뉩니다.  
새 테이블 추가 흐름(TableData → Converter 등록 → DataManager 로드 → Database 구현)이 일관성 있게 정해져 있습니다.

**이벤트 기반 UI 갱신**  
UI가 Manager를 폴링하지 않습니다. `OnInventoryChanged`, `OnEquipChanged` 등 이벤트를 구독하고, 변경이 발생할 때만 갱신합니다.

**AutoAttack 컴포넌트 기반 전투**  
플레이어와 동료가 동일한 AutoAttack 컴포넌트를 사용합니다. 전투 주체가 추가되어도 PlayerController·CompanionController 수정 없이 컴포넌트만 붙이면 됩니다.

**IAnimator 추상화**  
Spine(`SpineAnimator`)과 스프라이트 시트(`SpriteAnimator`)를 동일한 인터페이스로 구동합니다.  
애니메이션 방식이 바뀌어도 Controller 코드는 변경 없습니다.

---

## 앞으로 할 것

- EditMode 테스트 도입 (QuestManager 상태 전환, DropDatabase 가중치 검증)
- 매니저 인터페이스화 + 순환 참조 제거
- 적·드롭 아이템 오브젝트 풀링 (현재 Instantiate/Destroy 반복)
- 상점/우편 시스템 (기존 팝업·탭·인벤토리 재사용성 검증)

---

## 기술 스택

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 2022.3.55f1 |
| 언어 | C# |
| 애니메이션 | Spine-Unity |
| 데이터 | Excel → JSON (자체 에디터 툴) |
| 저장 | JSON 직렬화 (Newtonsoft.Json) |
| 버전관리 | Git / GitHub |
| 개발 기간 | 2025년~ (진행 중) |

# MadHatter — 포트폴리오 상세 설명 v2

> Unity 2022.3 / C# / 1인 개발 / 개발 중

---

## 이 문서를 쓰는 이유

GitHub 링크만 드리면 코드가 "왜 이렇게 생겼는지"가 전달되지 않습니다.  
설계 의도, 선택한 이유, 아쉬운 부분까지 직접 설명하는 것이 맞다고 생각해 작성했습니다.

---

## 개발 배경

아웃게임 UI/시스템 개발을 8년간 해오면서, 실무에서는 대부분 **이미 설계된 프레임워크 위에 컨텐츠를 붙이는 역할**을 했습니다.

이 프로젝트는 그 반대 방향의 시도입니다.  
**프레임워크 자체를 직접 설계하고**, 그 위에 아웃게임(인벤토리, 퀘스트, 상점, 대화)과 인게임(전투, AI, 맵)을 모두 올려보는 것이 목표였습니다.

---

## 구조 개요

```
Start 씬 → (비동기 로드) → Main 씬

SceneLoader (DontDestroyOnLoad)
  └─ DataManager: JSON → Database 싱글턴 적재
  └─ GameManager.LoadGame(): SaveData → 각 Manager에 주입

Manager Layer (싱글턴, DontDestroyOnLoad)
  GameManager / UIManager / InventoryManager / QuestManager
  PlayerInfoManager / CompanionManager / ShopManager / DataManager

Game Logic Layer
  PlayerController / EnemyController / MapController
  NPCController / CompanionController / CameraController

Database Layer (일반 C# 싱글턴, GameDatabase 파사드)
  ItemDatabase / QuestDatabase / NpcDatabase / DialogueDatabase
  DropDatabase / CompanionDatabase / ShopDatabase

Data Layer
  *TableData (엑셀 행 1:1 매핑) / *Data (게임용 불변 데이터)
  *State (런타임 상태) / SaveData (직렬화 전용)
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
  → Manager/Controller에서 GameDatabase.Instance.XXX 패턴으로 접근
```

ScriptableObject 대신 JSON + 에디터 변환 툴을 선택한 이유는 **기획자가 Excel을 직접 수정할 수 있는 환경**을 유지하기 위해서입니다.  
TableData 클래스가 Excel 행과 1:1 매핑되어 필드명이 양방향으로 검증됩니다.  
엑셀 파일이 저장될 때 `ExcelPostprocessor`가 자동으로 JSON 변환을 실행하므로 수동 변환을 잊을 염려가 없습니다.

Database는 순수 C# 클래스(non-MonoBehaviour)로 DataManager가 `CreateInstance()`로 생성합니다.  
Unity 생명주기에 묶이지 않기 위해서입니다.

**GameDatabase 파사드**  
7개 Database 싱글턴에 대한 단일 진입점을 추가했습니다.  
`GameDatabase.Instance.Items.GetItemById(...)` 패턴으로 신규 코드를 작성하면 Database 종류가 늘어도 접근 방식이 통일됩니다.

---

### 3. 적 AI — FSM 분리 구조

```
EnemyController  — 데이터 보유 + 실행 메서드 제공
EnemyFSM         — 상태 전환 판단, 공유 데이터 보유
EnemyState       — 각 상태별 행동 (순수 C# 클래스, new로 생성)
```

State가 Controller를 직접 참조하는 것을 허용했습니다.  
FSM → Controller 단방향으로 제한하고, Controller는 FSM을 모릅니다.  
State를 순수 C# 클래스로 만든 덕분에 상태 객체 자체는 Unity 없이 단독으로 테스트 가능한 구조입니다.

**상태 흐름**
```
Idle/Patrol ──감지──→ Chase ──공격 범위──→ Attack
    ↑                  ↓                     ↓
    └─── Return ←── 감지 이탈 ─────────────┘
              ↑
        Hit (어떤 상태에서든 피격 시) → 경직 해제 후 Chase 또는 Return
        Die (HP 0) → 종착
```

**리스폰 컴포지션**  
`EnemyRespawner`(요청) + `MapController`(코루틴 소유)로 책임을 분리했습니다.  
맵 프리팹이 파괴될 때 MonoBehaviour 코루틴이 자동으로 취소되므로 맵 전환 후 오염된 리스폰이 발생하지 않습니다.  
`_linkedQuestId`로 퀘스트 완료 후 리스폰을 중단하며, `Start()`에서 이미 완료된 퀘스트라면 초기 배치 몬스터 자체를 제거합니다.

---

### 4. 애니메이션 추상화 — IAnimator

```csharp
interface IAnimator {
    void PlayAnimation(string animName, bool loop = true);
    void SetFacing(Vector2 direction);
    void DisableAutoIdle();
}
```

`SpineAnimator`(Spine-Unity)와 `SpriteAnimator`(스프라이트 시트 + Animator)가 동일한 인터페이스를 구현합니다.  
FSM State는 `_controller.Anim.PlayAnimation("attack", false)`처럼 표준 이름으로만 요청합니다.  
애니메이션 방식이 바뀌어도 Controller·FSM 코드는 수정할 필요가 없습니다.

---

### 5. 전투 — AutoAttack 컴포넌트

전투 로직을 컴포넌트로 분리했습니다.  
플레이어와 동료 모두 동일한 `AutoAttack` 컴포넌트를 사용하며, 전투 주체가 추가되어도 `PlayerController`·`CompanionController` 코드를 수정할 필요가 없습니다.

공격력(`ATK`)의 단일 소유자는 `PlayerInfoManager`입니다.  
`AutoAttack.GetFinalDamage()`에서 플레이어이면 `PlayerInfoManager.Atk`(기본 공격력 + 장착 장비 보정)를, 동료이면 Inspector에 설정한 `_attackDamage`를 사용합니다.

---

### 6. UI 설계 원칙

- **슬롯은 매니저를 직접 모릅니다.** 클릭 이벤트는 Action 콜백으로 상위 UI에 위임합니다.
- **InfiniteScrollView** 는 데이터 타입을 모릅니다. `InfiniteScrollData` 추상 클래스와 `InfiniteScrollItem` 제네릭으로 인벤토리·퀘스트·상점이 같은 컴포넌트를 재사용합니다.
- **탭 시스템(TabController/Tab/TabPage)** 은 인벤토리·퀘스트·상점 등 특정 컨텐츠와 무관한 범용 컴포넌트입니다. 상점 UI 추가 시 기존 탭·스크롤뷰 구조를 그대로 재사용했습니다.
- **팝업**은 UIManager가 Resources에서 프리팹을 로드해 Instantiate합니다. `_currentPopup`으로 중복을 막고, `ClosePopup`에서 `ClearCurrentPopup`을 호출해 정리합니다.

---

### 7. 퀘스트 시스템

4종 목표 유형(Kill/Collect/Talk/Explore)과 `_nextQuestId` 체인을 지원합니다.

**보상 수령 흐름**  
목표 달성 후 즉시 보상을 받는 구조가 아니라, 퀘스트를 준 NPC에게 직접 방문해야 완료 대사와 보상이 지급됩니다. `ClaimReward`는 보상 지급만 담당하고, 완료 대사 재생은 NPCController가 맡습니다. 이 역할 분리로 `ClaimReward`가 대화 흐름을 알 필요가 없어졌습니다.

**체인 제약**  
NPCController는 체인 탐색 시 `questGiverNpcId != _npcId`이면 즉시 중단합니다. 다른 NPC의 퀘스트를 자동으로 시작하는 것을 방지합니다.

---

### 8. 저장/로드 설계

```
진입점: GameManager.SaveGame() / LoadGame()
파일 I/O: GameSystem (static 유틸)
```

각 매니저가 `GetSaveData()` / `ApplyData()` 를 구현하고, GameManager가 수집·주입합니다.  
`ApplyData` 호출 순서가 중요합니다: PlayerInfo → Quest → Companion → **Inventory**.  
InventoryManager는 `OnInventoryChanged` 이벤트에 자동 저장이 연결되어 있어, 다른 매니저 복원이 끝나기 전에 호출하면 복원 도중 불필요한 저장이 발생합니다.

자동 저장은 이벤트 구독으로 처리됩니다.  
- `OnQuestRewardClaimed`, `OnShopStockChanged`, `OnInventoryChanged`, `OnEquipChanged` 발행 시 GameManager가 자동 저장합니다.
- 맵 전환 완료 시에도 자동 저장됩니다.

---

### 9. 맵 시스템

맵은 전환 시 프리팹을 인스턴스화/파괴하는 방식입니다.  
타일맵 방식(Ground/Road/Walls/Decoration 4레이어)과 배경 이미지+콜라이더 방식이 공존하며 맵 특성에 따라 선택합니다.

**카메라 서브픽셀 스냅**  
WebGL 빌드에서 Lerp 후 카메라 좌표가 소수점 위치에 놓이면 타일 경계에 검은 틈이 발생했습니다.  
`pixelSize = orthographicSize * 2 / Screen.height` 단위로 좌표를 반올림 스냅하여 해결했습니다. 화면 해상도에 무관하게 동작하는 공식입니다.

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
새 테이블 추가 흐름(TableData → Converter 등록 → DataManager 로드 → Database 구현 → GameDatabase 프로퍼티 추가)이 일관성 있게 정해져 있습니다.

**이벤트 기반 UI 갱신**  
UI가 Manager를 폴링하지 않습니다. `OnInventoryChanged`, `OnEquipChanged`, `OnQuestListChanged` 등 이벤트를 구독하고, 변경이 발생할 때만 갱신합니다.

**AutoAttack 컴포넌트 기반 전투**  
플레이어와 동료가 동일한 AutoAttack 컴포넌트를 사용합니다. 전투 주체가 추가되어도 PlayerController·CompanionController 수정 없이 컴포넌트만 붙이면 됩니다.

**IAnimator 추상화**  
Spine(`SpineAnimator`)과 스프라이트 시트(`SpriteAnimator`)를 동일한 인터페이스로 구동합니다.  
애니메이션 방식이 바뀌어도 Controller 코드는 변경 없습니다.

**컴포지션 패턴 활용**  
`EnemyRespawner` + `MapController` 분리, `AutoAttack` 독립 컴포넌트, `TabController/Tab/TabPage` 범용화 등 단일 책임을 유지하는 컴포지션 패턴을 적극 활용했습니다.

---

## 앞으로 할 것

- EditMode 테스트 도입 (QuestManager 상태 전환, DropDatabase 가중치 검증)
- 매니저 인터페이스화 + 순환 참조 제거
- 적·드롭 아이템 오브젝트 풀링 (현재 Instantiate/Destroy 반복)
- AudioManager (BGM/SFX) + 옵션 UI 연동
- Addressable 시스템 전환 (현재 Resources.Load 방식)
- 인벤토리 UI 비주얼 완성 (슬롯 프레임, 동료 도감 스타일, HUD 스프라이트)

---

## 기술 스택

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 2022.3.55f1 |
| 언어 | C# |
| 애니메이션 | Spine-Unity |
| 데이터 | Excel → JSON (자체 에디터 툴) |
| 저장 | JSON 직렬화 (JsonUtility + Newtonsoft.Json) |
| 버전관리 | Git / GitHub |
| 개발 기간 | 2025년~ (진행 중) |

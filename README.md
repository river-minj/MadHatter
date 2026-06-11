# 2D Top-Down RPG Project v2 (Work in Progress)

(개발 중인 2D 탑다운 프로젝트)

https://river-minj.github.io/MadHatter_Build/

## ▶ About me
아웃게임 컨텐츠를 중심으로 8년간 개발해온 클라이언트 개발자입니다.

이 포트폴리오는 실무에서 경험한 아웃게임 아키텍처 방식을 직접 설계해보고, 인게임 시스템까지 적용하여 확장하려는 시도입니다.

**실무 경험 기반 강점**
- UI/UX 상태 관리: 여러 UI가 같은 데이터를 공유할 때의 갱신 동기화
- 시스템 조합 기반 컨텐츠 확장: 기존 컴포넌트 재사용으로 신규 컨텐츠 구현

## ▶ About This Project
이 프로젝트의 목적은 두 가지입니다.

**1. 기존 강점을 스스로 설계한 구조에서 구현**

실무에서는 이미 설계된 프레임워크 안에서 컨텐츠를 붙이는 역할을 주로 수행했습니다.
이 프로젝트에서는 제가 직접 구조를 설계하고 그 위에 컨텐츠 영역을 구현해 보았습니다.

**2. 아웃게임에서 쌓은 설계 감각을 인게임 도메인에 적용**

- 전투 FSM: Controller(데이터+실행) / FSM(상태 관리) / State(행동 판단) 3분할
- 애니메이션 추상화: IAnimator 인터페이스로 Spine/Sprite 교체 대비
- 단일 AutoAttack 컴포넌트로 플레이어/동료 전투 통합
- 기획자 직접 수정 가능한 데이터 파이프라인 (Excel → JSON, 필드명 양방향 검증)

## ▶ 구현된 시스템

| 시스템 | 내용 |
|---|---|
| 씬 전환 | SceneLoader 비동기 로딩 + LoadingUI 오버레이 |
| 데이터 파이프라인 | Excel → JSON → Database 싱글턴, GameDatabase 파사드 패턴 |
| 세이브/로드 | JSON 직렬화 (JsonUtility), 자동 저장 트리거 |
| 퀘스트 시스템 | Kill / Collect / Talk / Explore 4종 목표, 체인 퀘스트, NPC 보상 수령 |
| 인벤토리 | 아이템 획득/장착/사용, 드롭 테이블 (가중치 기반) |
| 상점 | 유한/무한 재고, ShopManager, 탭·스크롤뷰 재사용 |
| 동료 시스템 | 경로 추종 (PlayerTrailRecorder + CompanionController), 2열 대형 |
| 적 AI | FSM (Idle/Chase/Attack/Hit/Return/Die), 리스폰, 퀘스트 연동 중단 |
| 전투 | AutoAttack 컴포넌트 (범위 자동 공격), IDamageable 인터페이스 |
| NPC 상호작용 | IInteractable 다형성, 퀘스트/상점/대화 우선순위 처리 |
| UI | 범용 탭 시스템 + InfiniteScrollView 풀링, 이벤트 기반 갱신 |
| 입력 | Legacy Input Manager, PC 키보드 + 모바일/WebGL 가상 조이스틱 |
| WebGL 빌드 | Custom Template (9:16 반응형), Gzip + GitHub Pages 배포 |

## ▶ What's Next
- Object Pooling: 적·드롭 아이템 Instantiate/Destroy 반복 해소
- EditMode 테스트 도입: QuestManager, DropDatabase 등 로직 검증
- 매니저 인터페이스화: 순환 참조 제거, 단위 테스트 가능 구조
- AudioManager: BGM/SFX + 옵션 UI 연동
- Addressable 전환: Resources.Load 방식 교체
- 인벤토리 UI 비주얼 완성: 슬롯 프레임, 동료 도감 스타일, HUD 스프라이트

## ▶ 기술 스택

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 2022.3.55f1 |
| 언어 | C# |
| 애니메이션 | Spine-Unity |
| 데이터 | Excel → JSON (자체 에디터 툴) |
| 저장 | JSON 직렬화 (JsonUtility / Newtonsoft.Json) |
| 버전관리 | Git / GitHub |

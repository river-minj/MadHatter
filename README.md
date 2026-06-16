# MadHatter — Unity 2D RPG 포트폴리오

> Unity 2022.3 / C# / 1인 개발

아웃게임 UI, 콘텐츠 및 피쳐 개발 8년차 클라이언트 개발자입니다. 실무에서 주로 주어진 프레임워크 위에 콘텐츠를 구현하는 역할을 담당했습니다. 이 프로젝트는 프레임워크 자체의 설계부터 인게임 구조까지 직접 구현해보는 것을 목적으로 진행했습니다.

---

## ▶ 플레이하기

**[🎮 WebGL 빌드 실행](https://river-minj.github.io/MadHatter_Build/)**

PC 브라우저 환경에서 플레이 가능합니다. (모바일 가상 조이스틱 지원)

---

## ▶ 구현된 시스템

| 시스템 | 내용 |
| --- | --- |
| 씬 전환 | SceneLoader 비동기 로딩 + LoadingUI 오버레이 |
| 데이터 파이프라인 | Excel → JSON → Database 싱글턴, GameDatabase 파사드 패턴 |
| 세이브 / 로드 | JSON 직렬화, 이벤트 기반 자동 저장 |
| 퀘스트 시스템 | Kill / Collect / Talk / Explore 4종 목표, 체인 퀘스트, NPC 보상 수령 |
| 인벤토리 | 아이템 획득 / 장착 / 사용, 가중치 기반 드롭 테이블 |
| 상점 | 유한 / 무한 재고, 탭·스크롤뷰 재사용 |
| 동료 시스템 | 경로 추종 (PlayerTrailRecorder + CompanionController), 2열 대형 |
| 적 AI | FSM (Idle / Chase / Attack / Hit / Return / Die), 리스폰, 퀘스트 연동 중단 |
| 전투 | AutoAttack 컴포넌트 (범위 자동 공격), IDamageable 인터페이스 |
| NPC 상호작용 | IInteractable 다형성, 퀘스트 / 상점 / 대화 우선순위 처리 |
| UI | 범용 탭 시스템 + InfiniteScrollView 풀링, 이벤트 기반 갱신 |
| 입력 | PC 키보드 + 모바일 / WebGL 가상 조이스틱 |
| WebGL 빌드 | 커스텀 템플릿 (9:16 반응형), Gzip + GitHub Pages 배포 |

---

## ▶ 설계 키워드

- **FSM 3분할** — Controller(데이터·실행) / FSM(상태 전환) / State(행동 판단)로 역할을 분리했습니다. State는 Unity에 종속될 필요가 없다고 판단해 순수 C# 클래스로 구현했습니다.
- **IAnimator 추상화** — Spine과 스프라이트 시트를 동일한 인터페이스로 구동합니다. 애니메이션 방식이 바뀌어도 Controller 코드는 그대로 유지됩니다.
- **데이터 파이프라인** — Excel을 저장하면 에디터 툴이 자동으로 JSON으로 변환합니다. 기획자가 별도의 개발 병목 없이 데이터를 바로 고칠 수 있는 환경입니다.
- **이벤트 기반 UI 갱신** — UI가 Manager 상태를 매번 확인(폴링)하는 대신 OnInventoryChanged 같은 이벤트를 구독해서, 변경이 일어날 때만 갱신됩니다.
- **컴포지션 우선** — AutoAttack을 독립 컴포넌트로 분리해 재사용성을 높이고, EnemyRespawner는 MapController에 코루틴 실행을 맡겨 책임을 나눴습니다. 동료가 플레이어를 따라가는 기능 구현을 위해 경로 기록(PlayerTrailRecorder)과 경로 추적(CompanionController)의 역할을 분리했습니다.

---

## ▶ 기술 스택

| 항목 | 내용 |
| --- | --- |
| 엔진 | Unity 2022.3.55f1 |
| 언어 | C# |
| 애니메이션 | Spine-Unity |
| 데이터 | Excel → JSON (자체 에디터 툴) |
| 저장 | JSON 직렬화 |
| 버전 관리 | Git / GitHub |

---

## ▶ 설계 의도 · 트레이드오프 분석

개발 과정에서 어떤 이유로 이런 구조를 선택했는지, 그리고 어떤 문제를 만났는지를 정리했습니다.

**[📄 About_Project.md 읽기](./About_Project.md)**

# 2D Top-Down RPG Project (Work in Progress)

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

## ▶ What's Next
- Object Pooling: 적·드롭 아이템 Instantiate/Destroy 반복 해소
- EditMode 테스트 도입: QuestManager, DropDatabase 등 로직 검증
- 상점/우편 시스템 추가: 기존 팝업/탭/인벤토리 재사용성 검증
- 매니저 싱글톤 통일: Singleton<T> 베이스 도입으로 7개 매니저 일관화

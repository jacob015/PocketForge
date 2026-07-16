# TASKS.md

## 현재 작업

현재 작업: 없음 — 다음 구현 Task 승인 대기

## 완료 작업

### Task 4 — 생성 리소스 기반 비주얼 개선

상태: 완료

- 생성 광석 아트와 강화 아이콘 시트를 투명 PNG 에셋으로 변환해 프로젝트에 추가
- URP Unlit 머티리얼과 광석 빌보드로 3D 채굴 오브젝트에 광석 아트 적용
- Pickaxe·Drill·Robot 강화 버튼에 아이콘 시트의 각 영역을 UGUI `RawImage`로 연결
- Editor Play Mode 콘솔 오류 0건, EditMode 테스트 6/6 통과, Android 개발 빌드 성공(오류 0건·경고 1건)
- 최신 APK의 실제 기기 설치·실행은 테스트 기기 미연결로 보류

### Task 3 — 확장 가능한 채굴 구조와 UGUI 전환

상태: 완료

- 순수 C# `MiningGameState`·`MiningGameService`로 채굴 규칙과 MonoBehaviour 수명주기를 분리
- `MiningGameConfig.asset` ScriptableObject로 광석·강화 수치의 조정 지점을 제공하고 씬에 연결
- `GameSaveMigrator`로 저장값 정규화와 현재 버전(`2`) 마이그레이션 진입점 추가
- `MineHudView`·`MineHudPresenter`로 IMGUI `OnGUI`를 UGUI 화면으로 전환
- 새 Input System용 `InputSystemUIInputModule`을 사용해 모바일 UI 입력 경로 구성
- EditMode 테스트 6/6 통과, Editor Play Mode 오류 0건, Android 개발 빌드 및 SM-S938N 설치·실행 확인

### Task 2 — 채굴 수직 슬라이스 검증 및 다음 구현 범위 결정

상태: 완료

- Editor Play Mode에서 수동 채굴·강화·저장 후 재기동 시 저장 상태 유지 확인
- Android 개발용 APK 빌드·SM-S938N 설치·기동·세로 화면 UI 표시 확인
- `CreatePrimitive` 코드 스트리핑으로 발생한 `SphereCollider` 누락 오류 수정 및 재검증
- 명시적 URP Lit 광석 Material을 추가·연결해 Android의 분홍색 광석 표시 수정
- 수정 후 새 Android 앱 프로세스에서 `SphereCollider`·`CreatePrimitive`·크래시 오류 0건 확인
- EditMode `MiningBalanceTests` 3/3 통과

### Task 1 — 기존 구현 현황 감사

상태: 완료

- `Mine` 씬, 경제·채굴·저장 스크립트, EditMode 테스트의 책임 확인
- 실제 폴더 구조, 씬 구성, 런타임 흐름을 `ARCHITECTURE.md`에 기록
- Unity 씬 검증에서 누락 스크립트·깨진 Prefab 0건 확인
- `MiningBalanceTests` EditMode 3개 통과

### Task 0 — 개발 기반 및 운영 문서 정비

상태: 완료

- 루트 운영·기획 문서의 역할과 참조 관계 정리
- Codex와 Claude Code 공통 작업 규칙·기록 흐름 정리
- 기존 Unity 6 URP 프로젝트 확인: Unity `6000.5.4f1`, URP `17.5.0`
- 제품명 `PocketForge` 및 세로 화면 고정 적용
- `Assets/PocketForge/Scenes/Mine.unity`가 Build Settings의 첫 씬으로 열리는지 확인
- Git 초기화 및 Unity용 `.gitignore` 추가

미결 사항:

- Android SDK·NDK·OpenJDK 설치 경로와 개발용 APK 빌드는 Task 2에서 확인했다.
- Android 애플리케이션 식별자는 기본 템플릿 값이며, 배포 전 소유 도메인 기준으로 결정해야 한다.

## 다음 작업 후보

1. UGUI 전환과 게임 상태·표시 책임 분리
2. ScriptableObject 기반 광석·강화 데이터 도입
3. 저장 데이터 버전 보강과 자동 채굴·희귀 광석 확장

다음 구현 우선순위 제안: Android 런타임 오류를 해결한 뒤, 현재 `OnGUI` 기반 프로토타입의 표시·입력 책임을 UGUI와 게임 상태로 분리한다. 별도 Task로 승인받아 착수한다.

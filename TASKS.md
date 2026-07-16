# TASKS.md

## 현재 작업

현재 작업: 없음 — 다음 구현 Task 승인 대기

## 완료 작업

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

# TASKS.md

## 현재 작업

### Task 2 — 채굴 수직 슬라이스 검증 및 다음 구현 범위 결정

상태: 진행 중 — Android 기기 검증 대기

현재 프로토타입의 수동·자동 채굴, 강화, 저장 흐름은 소스와 EditMode 경제 테스트로 확인했다. Editor Play Mode에서 `Mine` 씬의 기동과 런타임 오류 0건을 확인했고, 수동 입력으로 채굴·강화·저장 후 재기동 시 저장 상태 유지를 확인했다. Android Build Support, SDK·NDK Tools, OpenJDK 설치 후 개발용 APK 빌드에 성공했다. 연결된 Android 기기는 없어 설치·실기기 검증은 남아 있다.

이번 작업에 포함하지 않는 항목:

- 게임플레이 로직 변경 또는 리팩터링
- 외부 패키지·SDK 추가 또는 제거
- Android·배포·계정 설정 변경

다음 확인 사항:

- Android 기기 연결 후 APK 설치·기동·기본 플레이 흐름 검증
- 검증 결과를 바탕으로 UGUI 전환·데이터 분리·저장 보강 중 다음 구현 범위 결정

## 완료 작업

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

다음 구현 우선순위 제안: 현재 `OnGUI` 기반 프로토타입의 표시·입력 책임을 UGUI와 게임 상태로 분리한다. 실기기 검증이 끝난 뒤 별도 Task로 승인받아 착수한다.

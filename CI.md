# Pocket Forge 빌드 파이프라인

이 문서는 `Jenkinsfile`이 무엇을 왜 하는지 설명한다. 파이프라인 정의는 Jenkins GUI가 아니라
저장소의 `Jenkinsfile`에 있으므로 코드와 같은 리뷰·이력 관리를 받는다.

## 왜 필요했나

개발 중 실제로 두 번 발생한 사고가 도입 근거다.

이 프로젝트는 `.asmdef`를 쓰지 않아 `Assets/PocketForge/Tests/Editor`의 모든 테스트가 에디터 툴과
같은 `Assembly-CSharp-Editor`로 컴파일된다. 테스트 코드에 컴파일 오류가 생기면 Unity 에디터는
**직전에 성공한 DLL을 그대로 유지**하고, Test Runner는 그 옛 어셈블리의 테스트를 실행해
`184/184 통과`를 초록불로 보고했다. 새로 작성한 테스트는 실행조차 되지 않은 상태였다.
Console을 직접 열어 `error CS7036`을 확인하기 전까지 로컬에서는 성공으로 보였다.

즉 **로컬 초록불이 검증을 보장하지 못한다.** 파이프라인의 `Compile` 스테이지는 이 상태를
명시적으로 실패로 만든다.

## 스테이지 구성

| 스테이지 | 하는 일 | 실패 시 의미 |
|---|---|---|
| Prepare | 아티팩트 폴더 준비, `UNITY_EDITOR` 존재 확인 | 노드 설정 문제 |
| Compile | `PocketForgeCi.AssertScriptsCompiled` 실행 | 스크립트 컴파일 오류 또는 stale 어셈블리 |
| EditMode tests | `-runTests -testPlatform EditMode`, NUnit XML 발행 | 회귀 발생 |
| Signed AAB | Jenkins Credentials로 키스토어 주입 후 서명 빌드 | 빌드·서명 문제 |
| Report size | AAB 용량 측정, 50MiB 예산 초과 시 `UNSTABLE` | 용량 예산 초과 |

컴파일과 테스트를 분리한 이유는 실패 원인이 로그를 읽기 전에 스테이지 이름만으로 드러나게
하기 위해서다.

### Compile 스테이지가 검사하는 것

`PocketForgeCi.AssertScriptsCompiled`는 세 가지를 확인하고 하나라도 어긋나면 종료 코드 1을 반환한다.

1. `EditorUtility.scriptCompilationFailed`가 false인가
2. `Assembly-CSharp-Editor`가 로드되었는가
3. 그 어셈블리에서 카나리 타입(`SaveCompatibilityTests`)이 실제로 해석되는가

3번이 stale 어셈블리 탐지의 핵심이다. 어셈블리는 로드되었지만 최신 테스트 타입이 없다면
에디터가 옛 DLL을 쓰고 있다는 뜻이므로, 그 상태의 테스트 결과는 신뢰할 수 없다.

## 비밀정보 관리

키스토어와 비밀번호는 저장소에 두지 않는다. `Jenkinsfile`은 `withCredentials`로만 접근하며
빌드 스크립트는 환경변수에서 읽는다.

| Credential ID | 종류 | 주입되는 환경변수 |
|---|---|---|
| `pocketforge-keystore` | Secret file | `POCKETFORGE_KEYSTORE_PATH` |
| `pocketforge-keystore-pass` | Secret text | `POCKETFORGE_KEYSTORE_PASS` |
| `pocketforge-keyalias-pass` | Secret text | `POCKETFORGE_KEYALIAS_PASS` |

`withCredentials` 블록 안의 값은 콘솔 로그에서 자동 마스킹된다.

## 버전 관리

Play는 이미 사용한 `versionCode`의 업로드를 거부한다. `PocketForgeAndroidBuild`가
`POCKETFORGE_VERSION_CODE`를 읽도록 되어 있고 파이프라인이 `BUILD_NUMBER`를 넣으므로
빌드마다 값이 자동으로 올라간다. 환경변수가 없으면 체크인된 기본값을 쓰므로 로컬 빌드는 그대로다.

## 노드 요구사항

- Windows 에이전트, Unity `6000.5.4f1` + Android Build Support
- 환경변수 `UNITY_EDITOR` = `Unity.exe` 전체 경로
- Jenkins 플러그인: NUnit, Credentials Binding, Pipeline
- **Unity 에디터가 같은 프로젝트를 열고 있으면 배치모드가 실패한다.** 에이전트는 프로젝트를
  단독 점유해야 하므로 개발 PC와 빌드 워크스페이스를 분리한다.

## 현재 상태와 남은 작업

- 파이프라인 정의, CI 진입점, 버전 주입은 저장소에 반영됐다.
- 진입점의 판정 로직은 에디터에서 검증했다(카나리 타입 해석, 테스트 타입 45개 확인).
- **미검증**: Jenkins 설치와 실제 파이프라인 실행. 개발 PC의 Unity 에디터가 프로젝트를
  점유하고 있어 배치모드 실행을 확인하지 못했다. Jenkins 설치는 시스템 변경이므로 승인 후 진행한다.

## 개선 기록

파이프라인은 만든 뒤가 아니라 운영하면서 나아진다. 측정한 항목을 여기에 누적한다.

| 날짜 | 변경 | 근거 |
|---|---|---|
| 2026-08-06 | 컴파일 검증을 테스트와 분리하고 stale 어셈블리 카나리 추가 | 로컬에서 두 번 발생한 위양성 초록불 |
| 2026-08-06 | `versionCode`를 `BUILD_NUMBER`로 주입 | 수동 증가 시 Play 업로드 거부 위험 |
| 2026-08-06 | AAB 용량을 측정해 50MiB 초과 시 UNSTABLE | 직전 수동 빌드 57.82MB로 목표 초과 |

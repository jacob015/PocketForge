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

## 로컬 Jenkins 실행 (개발 PC)

서비스로 설치하지 않고 WAR로 띄운다. 관리자 권한이 필요 없고 되돌리기가 쉽다.

```bat
"C:\Program Files\Eclipse Adoptium\jdk-21.0.12.8-hotspot\bin\java.exe" -jar E:\Jenkins\jenkins.war --httpPort=8081
```

- **포트 8081을 쓴다.** 기본값 8080은 Unity MCP 브리지가 이미 점유하고 있어 그대로 쓰면 에디터 제어 연결이 끊긴다.
- **Java 21이 필요하다.** Jenkins LTS는 Java 17을 더 이상 지원하지 않으므로 Unity 동봉 JDK(17)로는 기동하지 않는다. Temurin 21을 별도 설치했다.
- Unity의 Android 빌드는 Preferences에 지정된 자체 JDK를 쓰므로 시스템 JDK 21과 무관하다.
- `JENKINS_HOME`은 기본값 `C:\Users\<user>\.jenkins`다. 제거하려면 이 폴더와 `jenkins.war`만 지우면 된다.
- 중지: 실행 중인 `java.exe` 프로세스를 종료한다. WAR 실행은 서비스가 아니므로 재부팅 후 자동으로 뜨지 않는다.

### 파이프라인이 처음으로 잡아낸 결함

빌드 #2는 체크아웃과 컴파일을 통과하고 테스트에서 종료 코드 2로 실패했다. 213개 중 212개 통과,
1개 실패다. **로컬 에디터에서는 213개 전부 통과하던 상태였다.**

```
Korean MinerRankButton/MinerExperience resolved to 10.
  Expected: greater than or equal to 18
  But was:  10
```

원인은 두 가지였고, 둘 다 실재하는 문제였다.

1. **실제 결함**: XP 라벨의 `resizeTextMinSize`가 12로, Task 15 P3에서 정한 가독성 하한 18보다
   낮았다. 에디터에서는 문자열이 짧아 마침 18 이상으로 렌더돼 통과했을 뿐, 설정상으로는 12까지
   줄어들 수 있는 상태였다. 하한을 `MinimumReadableFontSize`로 올려 해결했고,
   `398 / 935` 기준 preferredWidth 73.5 < 박스 폭 110으로 잘리지 않음을 확인했다.
2. **테스트의 환경 의존성**: CI는 `-nographics`로 실행하므로 글리프가 생성되지 않고
   `fontSizeUsedForBestFit`가 의미 없는 값(설정 최솟값보다도 작은 10)을 돌려준다. 그래픽 장치가
   있으면 실제 렌더 크기를, 없으면 라벨이 허용하는 최소 설정값을 검사하도록 바꿨다. 후자는 환경과
   무관하게 성립하는 불변식이다.

로컬에서 초록불이던 상태에서 파이프라인이 잠재 결함을 드러낸 첫 사례다.

### 같은 결함이 한 건이 아니었다

빌드 #3은 다른 라벨(`EquipmentSlotPickaxe/SlotLabel`, 최솟값 14)에서 같은 이유로 실패했다.
한 건씩 고치며 CI를 반복 실행하는 대신 전수 조사를 했고, **BestFit 최솟값이 18 미만인 라벨이
68개**임을 확인했다. Task 15 P3에서 정한 하한은 *렌더된 크기*만 검사했기 때문에, *설정상*
하한 아래로 줄어들 수 있는 라벨들이 그대로 남아 있었던 것이다.

측정 결과 67개는 18로 올려도 여백이 남았고, 실제로 문제가 되는 것은 스타터 패키지 값
(`2K C + 20 ◆ + 6 코어`, 120폭 칸에 228 필요) 하나뿐이었다. 조치는 다음과 같다.

- 리터럴 최솟값 10곳을 `MinimumReadableFontSize`로 일괄 상향
- 값을 인자로 받는 헬퍼 3곳(`ConfigureMissionText`, `ConfigureCommerceText`,
  V5 `ConfigureText`)에서 하한을 `Mathf.Clamp`로 강제 — 호출부가 무엇을 넘기든 하한이 지켜진다
- 상점 보상 문자열을 `" + "` 대신 줄바꿈으로 연결하고 값 칸 높이를 70→100으로 확대
- 덤으로 발견한 영어 잘림 2건(`MINER Lv. 63`, `Blueprint Cores 0`)에 BestFit을 켜 해결

최종 확인: 4개 언어 전수 검사에서 하한 미달 0개, 실제 글자 잘림 0개.

호출부마다 값을 넣는 구조에서는 규칙이 지켜졌는지 알 수 없다. 헬퍼에서 강제하는 편이 옳다.

## 트리거

`main`을 10분 간격으로 폴링해 커밋이 실제로 움직였을 때만 빌드한다.

```groovy
triggers { pollSCM('H/10 * * * *') }
```

웹훅이 아니라 폴링을 쓰는 이유는 이 컨트롤러가 로컬에서만 돌아 GitHub에서 접근할 수 없기 때문이다.
`H`는 잡마다 실행 분을 분산시켜 정각에 부하가 몰리지 않게 한다. 폴링은 ref만 조회하므로
변경이 없으면 빌드가 시작되지 않는다. 컨트롤러를 외부에 노출하게 되면 웹훅으로 바꾸는 편이
지연과 낭비 모두에서 낫다.

## 용량 추이

`Report size` 스테이지가 매 빌드 AAB 크기를 한 줄짜리 CSV로 남기고 Plot 플러그인이 이를
빌드별 그래프로 누적한다. 잡 페이지의 **Plots** 링크에서 볼 수 있다.

추정이 아니라 실측이 쌓인다는 점이 핵심이다. 텍스처를 정리했을 때 실제로 몇 MiB가 줄었는지는
빌드해 보기 전에는 알 수 없었다.

### 로컬 저장소를 SCM으로 쓸 때

빌드 #1은 체크아웃 단계에서 실패했다.

```
ERROR: Checkout of Git remote 'E:\Unity Projects\DrillProject' aborted because it
references a local directory, which may be insecure.
```

Git 플러그인은 로컬 디렉터리를 원격으로 지정하는 것을 기본 차단한다. 공용 Jenkins에서는 잡 설정에
임의 경로를 넣어 서버 파일을 읽어낼 수 있기 때문이다. 1인 로컬 인스턴스에서는 해당 위험이 없으므로
기동 옵션으로 허용한다.

```bat
java -Dhudson.plugins.git.GitSCM.ALLOW_LOCAL_CHECKOUT=true -jar jenkins.war --httpPort=8081
```

**2026-08-07에 해소했다.** 커밋 17개를 GitHub에 푸시하고 잡의 `url`을
`https://github.com/jacob015/PocketForge.git`으로 바꾼 뒤, `ALLOW_LOCAL_CHECKOUT` 없이
기동하도록 되돌렸다. 우회 옵션이 필요했던 것은 로컬 경로를 원격으로 쓰던 임시 구성 때문이었다.

### 플러그인 설치가 실패할 때

Jenkins 업데이트 센터에서 NUnit 플러그인을 받을 때 TLS 핸드셰이크가 리셋되는 경우가 있다.

```
java.net.SocketException: Connection reset
Caused: java.io.IOException: Failed to download from
  https://updates.jenkins.io/download/plugins/nunit/... → https://get.jenkins.io/plugins/nunit/...
```

같은 URL을 `curl`로 받으면 200이 떨어지므로 네트워크 문제가 아니라 Jenkins의 Java HTTP 클라이언트가
미러 리다이렉트 체인에서 막히는 것이다. 우회 절차는 다음과 같다.

1. `curl -sL -o nunit.hpi <플러그인 URL>`로 직접 받는다.
2. 의존성을 먼저 확인한다. `unzip -p nunit.hpi META-INF/MANIFEST.MF`의 `Plugin-Dependencies`와
   `Jenkins-Version`을 읽고, 이미 설치된 플러그인·Jenkins 버전이 이를 만족하는지 본다.
   (NUnit 648은 `junit`, `commons-lang3-api`, `structs`를 요구하고 Jenkins 2.528.3 이상이 필요하다.)
3. `%JENKINS_HOME%\plugins\nunit.jpi`로 복사한다.
4. Jenkins를 재기동한다. 로드에 성공하면 `plugins\nunit\` 폴더로 압축이 풀린다.

의존성 확인을 건너뛰고 파일만 넣으면 기동 시 조용히 로드에 실패하므로 2번을 생략하지 않는다.

## 노드 요구사항

- Windows 에이전트, Unity `6000.5.4f1` + Android Build Support
- 환경변수 `UNITY_EDITOR` = `Unity.exe` 전체 경로
- Jenkins 플러그인: NUnit, Credentials Binding, Pipeline
- **Unity 에디터가 같은 프로젝트를 열고 있으면 배치모드가 실패한다.** 에이전트는 프로젝트를
  단독 점유해야 하므로 개발 PC와 빌드 워크스페이스를 분리한다.

## 현재 상태와 남은 작업

- 파이프라인 정의, CI 진입점, 버전 주입은 저장소에 반영됐다.
- Jenkins 2.568.2를 WAR로 기동하고 `PocketForge` 잡을 등록했다.
- 빌드 #2에서 체크아웃 → Compile(Unity 임포트 약 2.5분) → EditMode 테스트 213개 실행까지
  도달했고, 파이프라인이 실제 결함 1건을 검출했다.
- 빌드 #5가 5개 스테이지를 모두 완주했다. `BUILD_ANDROID` 파라미터로 서명 AAB까지 생성했고
  `jarsigner -verify` 결과는 `jar verified`다.

### 빌드 #5 실측

| 스테이지 | 소요 |
|---|---:|
| Prepare / Compile | 0초 (Library 캐시 적중) |
| EditMode tests | 37초 (213/213 통과) |
| Signed AAB | 262초 |
| 전체 | 5분 3초 |

용량은 Task 16의 텍스처 정리 효과를 처음으로 실측한 값이다.

| | bytes | MiB |
|---|---:|---:|
| 이전 수동 빌드 | 57,822,260 | 55.14 |
| CI 빌드 #5 | 54,550,979 | 52.02 |
| 차이 | −3,271,281 | **−3.12 (−5.7%)** |

Task 16에서 계산한 추정 절감은 약 4MB였고 실측은 3.12MiB다. AAB가 이미 압축된 컨테이너라
ASTC 전환 효과가 그대로 반영되지 않은 것으로 보인다. **추정치를 실측으로 대체했다는 점이 핵심이다.**

50MiB 예산을 2.02MiB 초과해 빌드는 의도대로 `UNSTABLE`로 표시됐다. 실패가 아니므로 아티팩트는
정상 보관된다.

### 남은 과제

- 용량 2.02MiB 추가 감축(50MiB 목표 달성)
- 실패 알림(메일 또는 웹훅) 연결
- 실기기 프레임·발열·메모리 측정
- 저장소 용량 관리 — `.git`이 498MB이고 스크린샷·중간 산출물이 계속 쌓인다

## 개선 기록

파이프라인은 만든 뒤가 아니라 운영하면서 나아진다. 측정한 항목을 여기에 누적한다.

| 날짜 | 변경 | 근거 |
|---|---|---|
| 2026-08-06 | 컴파일 검증을 테스트와 분리하고 stale 어셈블리 카나리 추가 | 로컬에서 두 번 발생한 위양성 초록불 |
| 2026-08-06 | `versionCode`를 `BUILD_NUMBER`로 주입 | 수동 증가 시 Play 업로드 거부 위험 |
| 2026-08-06 | AAB 용량을 측정해 50MiB 초과 시 UNSTABLE | 직전 수동 빌드 57.82MB로 목표 초과 |
| 2026-08-07 | Jenkins를 8081로 기동 | 기본 8080을 Unity MCP 브리지가 점유해 충돌 |
| 2026-08-07 | Temurin JDK 21 도입 | Jenkins LTS가 Java 17 지원을 중단, Unity 동봉 JDK로는 기동 불가 |
| 2026-08-07 | NUnit 플러그인 수동 설치 절차 문서화 | 업데이트 센터 다운로드가 TLS 리셋으로 반복 실패 |
| 2026-08-07 | `ALLOW_LOCAL_CHECKOUT=true`로 기동 | 빌드 #1이 로컬 경로 SCM 차단으로 체크아웃 실패 |
| 2026-08-07 | XP 라벨 BestFit 최솟값 12→18, 폰트 검사를 환경 인지형으로 변경 | 빌드 #2가 로컬 초록불 상태의 잠재 결함을 검출 |
| 2026-08-07 | 하한 미달 라벨 68개 전수 교정, 공통 헬퍼에서 하한 강제 | 빌드 #3이 같은 결함의 다른 사례를 검출 |
| 2026-08-07 | AAB 용량 측정을 샌드박스 허용 스텝으로 교체 | `new File()`은 Groovy 샌드박스가 차단 |
| 2026-08-08 | SCM을 GitHub 원격으로 전환하고 `ALLOW_LOCAL_CHECKOUT` 제거 | 보안 가드를 끄지 않아도 되고, 별도 워크스페이스라 개발 PC의 에디터 잠금과 무관하게 빌드된다 |
| 2026-08-08 | Prepare 단계에서 이전 `*.aab`·`size.csv` 삭제 | 빌드 #6 아카이브에 5·6번 AAB가 함께 담김 |
| 2026-08-08 | `pollSCM('H/10 * * * *')` 자동 트리거 | 수동 실행만 가능해 푸시와 검증 사이가 벌어짐. 컨트롤러가 GitHub에서 도달 불가라 웹훅 대신 폴링 |
| 2026-08-08 | Plot 플러그인으로 AAB 용량 추이 기록 | 용량이 로그 한 줄로만 남아 회귀를 알아채기 어려움 |
| 2026-08-08 | 실기기 스모크 테스트로 검출한 중복 라벨 3건에 회귀 테스트 추가 | 특정 게임 상태에서만 나타나 캡처 검수로는 놓친 결함 |
| 2026-08-09 | Jenkins를 시작프로그램 런처로 기동 | WAR를 셸에서 직접 띄워 터미널이 닫히면 CI가 조용히 내려갔다. 푸시가 쌓여도 빌드가 돌지 않고, 알림을 보낼 주체가 죽어 있어 실패 통보조차 오지 않는다 |
| 2026-08-09 | 런처가 `UNITY_EDITOR`를 직접 설정 | 파이프라인이 Jenkins를 띄운 셸의 환경변수에 의존하고 있었다. 다른 셸에서 재기동하자 빌드 #27이 `UNITY_EDITOR is not set`으로 실패했다. Prepare 가드가 원인을 이름으로 드러낸 사례이기도 하다 |
| 2026-08-08 | `failure`/`unstable`/`fixed` 상태 전이 알림 추가 | 폴링으로 빌드가 자동 시작되므로 아무도 보고 있지 않을 때 실패가 묻힌다. 상태 전이에서만 보내 초록불 구간은 조용하게 유지 |
| 2026-08-08 | 알림 JSON을 직접 조립 | `JsonOutput`도 Groovy 샌드박스가 차단 |
| 2026-08-08 | 릴리스 빌드가 런처 아이콘을 매번 재적용 | 에디터 UI를 한 번도 연 적 없는 CI 워크스페이스에서도 실제 아이콘이 들어가도록 |
| 2026-08-08 | `when { branch 'main' }`을 `shouldBuildAndroid()`로 교체 | `branch`는 멀티브랜치 파이프라인에서만 평가된다. 단일 브랜치 잡이라 `BRANCH_NAME`이 비어 `Signed AAB`·`Report size`가 빌드 #7~#15 동안 건너뛰어졌는데 전부 SUCCESS로 보고됐다. 빌드 소요시간이 302초에서 45초로 떨어진 것으로 발견 |
| 2026-08-08 | 패키징 단계 미실행 시 UNSTABLE 처리 | 같은 종류의 조용한 누락이 다시 SUCCESS로 지나가지 않도록 |
| 2026-08-07 | Library 캐시 재사용 확인 (첫 임포트 150초 → 22초, −85%) | 빌드 #2~#3 Compile 스테이지 실측 |
| 2026-08-07 | 텍스처 정리 효과 실측 (AAB 55.14 → 52.02 MiB, −5.7%) | 빌드 #5, Task 16 추정치를 실측으로 대체 |
| 2026-08-07 | SCM을 GitHub 원격으로 전환하고 `ALLOW_LOCAL_CHECKOUT` 제거 | 로컬 경로 우회는 임시 구성이었고 보안 가드를 되돌려야 함 |
| 2026-08-07 | `pollSCM('H/10 * * * *')` 자동 트리거 추가 | 수동 실행만 가능하면 호출되지 않은 변경은 검사되지 않음 |
| 2026-08-07 | Plot 플러그인으로 AAB 용량 추이 시각화 | 빌드마다 쌓이는 실측값을 한눈에 비교하기 위해 |

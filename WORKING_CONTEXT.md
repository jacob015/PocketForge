# WORKING_CONTEXT.md

## 수정사항 1

- 기록 시각: 2026-07-16 15:08:11
- 작업 요청 요약: Unity 모바일 게임 프로젝트를 Codex와 Claude Code가 동일한 규칙·작업 기록으로 이어서 작업할 수 있는 문서 기반을 구성한다.
- 수정 전 상태: Unity 프로젝트와 프로젝트 루트 운영 문서가 없었다. 제공된 이전 작업 기록 파일은 비어 있었다.
- 수정한 내용: `AGENTS.md`, `CLAUDE.md`, `RULES.md`, `TASKS.md`, `PROJECT_PLAN.md`, `AI_USAGE.md`, `WORKING_CONTEXT.md`를 생성했다.
- 수정 후 상태: Codex는 `AGENTS.md`, Claude Code는 `CLAUDE.md`를 진입 지침으로 사용하며, 두 도구는 공통 규칙과 작업 기록 파일을 참조할 수 있다.
- 테스트 결과: 문서 파일 생성 및 내용 확인만 수행했다. Unity 프로젝트, 코드, 외부 SDK, Android 빌드는 아직 생성하거나 테스트하지 않았다.
- 남은 작업: Unity 6 URP 프로젝트 생성 승인 후 개발 기반을 구성한다.

## 수정사항 2

- 기록 시각: 2026-07-16 15:55:28
- 작업 요청 요약: Claude Code와 병렬로 작업하는 상황에서 루트 Markdown 문서를 향후 기술·아키텍처 문서화에 적합하도록 정리한다.
- 수정 전 상태: 초기 운영 문서에는 공통 규칙과 계획이 있었지만, 문서별 기준 정보와 문서화 확장 기준이 한곳에 정리되어 있지 않았다. Unity 프로젝트와 Git 저장소는 아직 생성되지 않았다.
- 수정한 내용: `DOCUMENTATION.md`를 추가하고, `AGENTS.md`·`CLAUDE.md`에 문서 색인 참조를 추가했다. `TASKS.md`에 문서 정비 완료 상태와 남은 Unity 기반 작업을 구분했으며, `PROJECT_PLAN.md`에 계획·구현 사실의 구분과 아키텍처 문서화 기준을 추가했다. `AI_USAGE.md`에 이번 AI 문서 작업 기록을 추가했다.
- 수정 후 상태: 운영 규칙, 현재 작업, 제품 계획, 작업 이력, AI 활용 기록의 기준 위치가 명확해졌고, 구현 이후 작성할 아키텍처 문서가 계획과 혼동되지 않도록 기준을 마련했다.
- 테스트 결과: UTF-8 원문으로 모든 루트 Markdown 파일을 읽고, 문서 간 참조 대상 파일의 존재 여부를 확인했다. Unity 프로젝트, 코드, 외부 SDK, Android 빌드는 생성하거나 테스트하지 않았다.
- 남은 작업: Unity 6 URP 프로젝트 생성에 필요한 로컬 경로·정확한 Unity 버전·Android SDK/JDK 설치 여부를 승인과 함께 확인한다.

## 수정사항 3

- 기록 시각: 2026-07-16 16:02:47
- 작업 요청 요약: 이미 생성된 Unity 프로젝트를 PocketForge 기준으로 정리하고, Android 세로 화면·Git·시작 씬 상태를 확인한다.
- 수정 전 상태: 문서에는 Unity 프로젝트가 아직 생성되지 않은 것으로 기록되어 있었지만, 실제 경로에는 Unity `6000.5.4f1`·URP `17.5.0` 프로젝트와 `Assets/PocketForge/Scenes/Mine.unity`가 존재했다. 프로젝트명은 `DrillProject`였고 화면 방향은 세로 고정이 아니었다.
- 수정한 내용: Unity Editor에서 제품명을 `PocketForge`로 설정하고 화면 방향을 Portrait로 고정했다. Git 저장소를 초기화하고 Unity 생성 파일·IDE 파일·Android 산출물을 제외하는 `.gitignore`를 추가했다. 실제 시작 씬과 작업 상태를 반영하도록 `TASKS.md`와 `AI_USAGE.md`를 갱신했다.
- 수정 후 상태: `Mine.unity`가 Build Settings의 첫 씬으로 등록되어 있고 Editor에서 열려 있다. 제품명과 화면 방향 설정이 적용되었으며 Git 저장소가 초기화되었다. 기존 게임플레이 구현은 수정하지 않았다.
- 테스트 결과: Unity MCP로 현재 씬과 Build Settings를 확인하고 Unity Console 오류 0건을 확인했다. Git ignore가 `Library`, `Temp`, `.idea`, 생성된 `.csproj` 파일을 제외하는지 `git check-ignore`로 확인했다. Android SDK/JDK 경로 조회는 Unity MCP의 인메모리 코드 컴파일에서 Android Editor API를 찾지 못해 확인하지 못했으며, Android 빌드·기기 테스트는 실행하지 않았다.
- 남은 작업: Android SDK/JDK 설치 경로와 실제 Android 빌드 확인, Play 배포 전 Android 애플리케이션 식별자 결정, 기존 `Mine` 씬·스크립트의 구현 현황 감사가 남아 있다.

## 수정사항 4

- 기록 시각: 2026-07-16 16:05:54
- 작업 요청 요약: 기존 `Mine` 씬과 관련 스크립트의 구현 현황을 코드 변경 없이 감사하고, 향후 기술·아키텍처 문서의 기준선을 맞춘다.
- 수정 전 상태: 문서는 Unity 기반만 기록하고 있었지만, 프로젝트에는 채굴·경제·저장·EditMode 테스트를 포함한 수직 슬라이스 프로토타입이 존재했다.
- 수정한 내용: `ARCHITECTURE.md`에 실제 폴더 책임, 씬 구성, 런타임 흐름, 저장 경계, 기획 대비 차이를 기록했다. `DOCUMENTATION.md`, `PROJECT_PLAN.md`, `TASKS.md`, `AI_USAGE.md`에 아키텍처 기준과 감사 완료 상태를 반영했다.
- 수정 후 상태: 현재 구현 사실은 `ARCHITECTURE.md`, 제품 목표는 `PROJECT_PLAN.md`, 다음 작업 범위는 `TASKS.md`로 구분된다. 게임플레이 코드와 Unity 씬 오브젝트는 변경하지 않았다.
- 테스트 결과: Unity MCP에서 `Mine` 씬의 누락 스크립트·깨진 Prefab 0건을 확인했다. `MiningBalanceTests` EditMode 테스트는 3/3 통과했다. 테스트 후 Unity Console에는 `TestResults.xml` 저장 경로를 알리는 Exception 형식 로그 1건이 남았으며 stack trace는 없었다. Play Mode, Android 빌드, Android 기기 테스트는 실행하지 않았다.
- 남은 작업: Editor Play Mode와 Android 환경의 실제 플레이 흐름 검증, UGUI 전환·데이터 분리·저장 보강 중 다음 구현 범위 결정이 남아 있다.

## 수정사항 5

- 기록 시각: 2026-07-16 16:15:08
- 작업 요청 요약: GitHub 계정을 연동하고 PocketForge 프로젝트의 공개 원격 저장소를 생성한다.
- 수정 전 상태: 로컬 Git 저장소는 `main` 브랜치만 존재했고, 원격 저장소·커밋·GitHub CLI 인증이 없었다.
- 수정한 내용: GitHub CLI를 설치하고 `jacob015` 계정으로 인증했다. 공개 저장소 `jacob015/PocketForge`를 생성하고, 프로젝트 전체를 `Initial PocketForge project` 초기 커밋으로 `origin/main`에 푸시했다.
- 수정 후 상태: 로컬 `main`은 `origin/main`을 추적하며, 프로젝트는 `https://github.com/jacob015/PocketForge`에 공개되어 있다.
- 테스트 결과: GitHub CLI 인증과 `git push -u origin main` 성공을 확인했다. Unity·Android 테스트는 이 작업에서 실행하지 않았다.
- 남은 작업: Task 2의 Editor Play Mode·Android 플레이 흐름 검증과 다음 구현 범위 결정이 남아 있다.

## 수정사항 6

- 기록 시각: 2026-07-16 16:19:04
- 작업 요청 요약: 원래 Task 2로 돌아가 채굴 수직 슬라이스의 Editor Play Mode와 Android 환경을 검증한다.
- 수정 전 상태: `Mine` 씬은 EditMode 검증과 경제 계산 테스트만 완료된 상태였고, Editor Play Mode와 Android 모듈 설치 여부는 미확인이었다.
- 수정한 내용: Unity Editor에서 `Mine` 씬을 Play Mode로 실행하고 Main Camera 화면 캡처를 확인한 뒤 Play Mode를 종료했다. Unity 설치 경로에서 Android Playback Engine 존재 여부를 확인했다. `TASKS.md`와 `AI_USAGE.md`에 진행 상태를 반영했다.
- 수정 후 상태: `Mine` 씬은 Editor Play Mode에서 기동되고 새 Unity Console 오류 없이 종료됐다. 설치된 Unity `6000.5.4f1`에는 Android Playback Engine이 없어 Android 빌드·기기 검증을 실행할 수 없다.
- 테스트 결과: Editor Play Mode 기동·화면 캡처·종료와 런타임 Console 오류 0건을 확인했다. 수동 채굴·강화·저장 입력, Android 빌드, Android 기기 테스트는 실행하지 않았다.
- 남은 작업: Android Build Support, SDK/JDK 설치 승인 후 Android 검증; Editor의 수동 입력 검증; 다음 구현 범위 결정이 남아 있다.

## 수정사항 7

- 기록 시각: 2026-07-16 16:44:53
- 작업 요청 요약: Android Build Support, SDK·NDK Tools, OpenJDK 설치 완료 후 Android 빌드 검증을 재개한다.
- 수정 전 상태: Unity `6000.5.4f1`에는 Android Playback Engine이 없어 Android 플랫폼 전환 시 Burst 패키지 컴파일 오류가 발생했다.
- 수정한 내용: Unity Hub로 설치된 AndroidPlayer 경로를 확인했다. `SDK`, `NDK`, `OpenJDK` 폴더가 모두 존재하는 것을 확인했다. Unity Editor 재시작 후 Android 컴파일 로그와 MCP 연결 상태를 확인했다.
- 수정 후 상태: Android 종속성은 설치됐지만, Unity MCP 브리지가 `127.0.0.1:8080`에서 응답하지 않아 자동 Android 빌드 도구를 사용할 수 없다. 이 상태에서는 Android 빌드 성공 여부를 확인하지 못했다.
- 테스트 결과: Android 모듈 디렉터리 존재 여부를 확인했다. Editor 로그에는 Android ADB 장치 스캔이 기록됐다. Android APK/AAB 빌드와 기기 테스트는 실행하지 못했다.
- 남은 작업: Unity Editor를 하나의 인스턴스로 열고 MCP 브리지를 재연결한 뒤 Android 개발용 APK 빌드와 기기 검증을 수행한다. Editor 수동 입력 검증과 다음 구현 범위 결정도 남아 있다.

## 수정사항 8

- 기록 시각: 2026-07-16 17:09:34
- 작업 요청 요약: Codex 앱 재시작으로 Unity MCP 연결을 복구한 뒤 Android 개발용 APK 빌드를 검증한다.
- 수정 전 상태: Android SDK·NDK·OpenJDK는 설치됐지만 Unity MCP 연결 실패로 실제 APK 빌드를 실행하지 못했고, Android 기기 연결 여부도 미확인이었다.
- 수정한 내용: Unity MCP 연결을 복구해 Android 대상, `Mine` 빌드 씬, 빌드 전 Console 상태를 확인하고 개발용 APK를 `Builds/Android/PocketForge-dev.apk`로 생성했다. 게임 코드·Unity 프로젝트 설정·배포 설정은 변경하지 않았다.
- 수정 후 상태: Android 개발용 APK가 생성됐으며, 빌드 도구 결과는 성공(오류 0건, 경고 1건), Unity 로그는 `Build Successful`이다. Unity Console에는 URP 관련 경고와 Diagnostics Data의 디버그 심볼 안내가 남았으나 빌드를 중단시키지 않았다.
- 테스트 결과: Android APK 빌드 성공(192.15초), APK 파일 존재 및 크기 44,837,960바이트 확인, `adb devices`에서 연결된 기기 0대를 확인했다. 따라서 APK 설치·기동·실기기 플레이 검증은 실행하지 않았다.
- 남은 작업: Android 기기를 연결해 APK 설치·기동·기본 플레이 흐름을 검증하고, Editor 수동 입력 검증 결과를 바탕으로 다음 구현 범위를 결정한다.

## 수정사항 9

- 기록 시각: 2026-07-16 17:20:36
- 작업 요청 요약: Android 빌드 검증 후 Editor에서 수동 입력으로 채굴·강화·저장 흐름을 확인한다.
- 수정 전 상태: `Mine` 씬의 Play Mode 기동과 APK 생성은 확인됐지만, 실제 UI 입력과 저장 데이터 재적용은 미확인이었다.
- 수정한 내용: Play Mode에서 `MINE`과 `PICKAXE` 버튼을 노출하고, 사용자가 채굴·강화 후 Play Mode 재기동으로 저장 상태 유지를 직접 확인했다. 게임 코드·Unity 씬·프로젝트 설정은 변경하지 않았다.
- 수정 후 상태: Editor 수동 채굴·강화·저장 흐름이 확인됐고, Android 실기기 설치·기동 검증만 남아 있다. 다음 구현 우선순위는 `OnGUI` 프로토타입을 UGUI와 게임 상태 책임으로 분리하는 작업으로 제안한다.
- 테스트 결과: 사용자가 Editor Play Mode에서 Credits 확보, Pickaxe 강화 1회, 재기동 후 Credits와 Pickaxe 레벨 유지 여부를 확인했다고 보고했다. Play Mode 진입 직후 Unity Console 오류·경고 0건을 확인했다. Android 기기는 연결되지 않아 실기기 테스트는 실행하지 않았다.
- 남은 작업: Android 기기를 연결해 APK 설치·기동·기본 플레이 흐름을 검증한다. 완료 후 UGUI 전환·게임 상태 분리 작업의 범위와 승인 여부를 결정한다.

## 수정사항 10

- 기록 시각: 2026-07-16 17:59:49
- 작업 요청 요약: Android 실기기에 개발용 APK를 설치·기동해 기본 플레이 화면과 런타임 로그를 검증한다.
- 수정 전 상태: 개발용 APK는 생성됐지만, 연결된 Android 기기가 없어 설치·기동 검증을 수행하지 못했다.
- 수정한 내용: Samsung SM-S938N 기기를 ADB로 인증한 뒤 개발용 APK를 설치하고 UnityPlayerGameActivity를 기동했다. 기기 화면 캡처와 logcat을 수집했다. 게임 코드·Unity 프로젝트 설정은 변경하지 않았다.
- 수정 후 상태: APK 설치와 앱 기동, 세로 화면 UI 표시는 성공했다. 그러나 `MineGameController.CreateOreVisual()`의 `GameObject.CreatePrimitive(PrimitiveType.Sphere)`가 Android에서 `SphereCollider` 클래스를 찾지 못해 오류를 기록한다. 앱 프로세스는 오류 후에도 실행 중이지만, 광석 시각화 경로는 정상으로 볼 수 없다.
- 테스트 결과: `adb install -r` 성공, UnityPlayerGameActivity 포그라운드·프로세스 유지 확인, 기기 화면에서 Credits·Depth·MINE·강화 버튼 표시 확인. logcat에서 `Can't add component because class 'SphereCollider' doesn't exist!`와 `GameObject.CreatePrimitive` → `MineGameController.CreateOreVisual` 호출 경로를 확인했다.
- 남은 작업: Android에서 필요한 primitive 의존 컴포넌트가 보존되도록 광석 시각화 생성 코드를 수정하고, APK 재빌드·재설치·실기기 재검증을 수행한다. 이 코드 변경은 현재 검증 범위 밖이므로 사용자 승인을 받은 뒤 진행한다.

## 수정사항 11

- 기록 시각: 2026-07-16 18:13:45
- 작업 요청 요약: Android 실기기에서 발견한 광석 생성·표시 오류를 수정하고 재검증한다.
- 수정 전 상태: Android에서 `GameObject.CreatePrimitive(PrimitiveType.Sphere)`가 `SphereCollider` 누락 오류를 내고, Default-Material 누락으로 광석이 분홍색으로 표시됐다.
- 수정한 내용: `MineGameController`에 Unity 공식 문서가 요구하는 primitive 의존 컴포넌트 private 속성 4개를 추가했다. `Assets/PocketForge/Materials/MineOre.mat` URP Lit Material을 생성하고 `MineGameController.oreMaterial`에 연결해 런타임 광석에 할당했다. `Mine.unity` 씬을 저장했다.
- 수정 후 상태: Android 개발용 APK는 명시적 광석 Material과 primitive 의존 컴포넌트를 포함한다. 광석은 Editor와 실기기에서 주황색으로 표시되며, 기존 채굴·강화·저장 계산 로직은 변경하지 않았다.
- 테스트 결과: Unity 스크립트 컴파일 성공, Editor Play Mode 화면 및 Console 오류 0건 확인, Android APK 2회 재빌드 성공(마지막 빌드 오류 0건), SM-S938N에 재설치·기동 성공. 새 앱 프로세스 logcat에 `SphereCollider`, `CreatePrimitive`, `AndroidRuntime`, `FATAL EXCEPTION` 오류 0건을 확인했고 기기 화면에서 주황색 광석을 확인했다. EditMode `MiningBalanceTests` 3/3 통과했다.
- 남은 작업: 현재 검증 Task 2는 완료됐다. 다음 구현 후보인 UGUI 전환·게임 상태와 표시 책임 분리는 별도 Task 승인 후 진행한다.
-
## 수정사항 12

- 기록 시각: 2026-07-16
- 작업 요청 요약: 작은 프로젝트에도 확장성을 보여 줄 수 있도록 채굴 규칙, 콘텐츠 수치, 저장 마이그레이션, UGUI 표시 책임을 분리한다.
- 수정 전 상태: `MineGameController`가 `OnGUI`, 채굴 규칙, 런타임 상태, 저장 호출을 함께 담당했고, 밸런스는 정적 계산 클래스에 고정되어 있었다.
- 수정한 내용: `MiningGameState`, `MiningGameService`, `MineHudPresenter`, `MineHudView`, `MiningGameConfig`, `GameSaveMigrator`를 추가했다. `MiningGameConfig.asset`을 생성해 씬의 `MineGameController`에 연결했고, UGUI 입력은 프로젝트의 새 Input System에 맞춰 `InputSystemUIInputModule`을 사용한다.
- 수정 후 상태: 컨트롤러는 Unity 조립·광석 시각화·저장 연결만 맡고, 순수 서비스는 채굴과 강화 규칙을 맡는다. 저장 로드는 버전 2 정규화 진입점을 거치며, 화면은 UGUI 뷰와 프레젠터로 표시된다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 테스트 6/6 통과, Editor Play Mode 콘솔 오류 0건, Android 개발 APK 빌드 성공(오류 0건·경고 1건), SM-S938N에 설치 후 `UnityPlayerGameActivity` 전면 실행 및 관련 치명 로그 없음.
- 다음 작업: ScriptableObject를 광석·강화 개별 콘텐츠 정의로 세분화하거나, 오프라인 보상·자동 채굴 확장 전에 저장 마이그레이션 단계를 추가한다.

## 수정사항 13

- 기록 시각: 2026-07-16 19:03:12
- 작업 요청 요약: 밋밋한 화면을 개선하기 위해 생성 도구와 Unity MCP로 광석·UI 리소스를 만들고 실제 프로젝트에 적용한다.
- 수정 전 상태: 단색 Sphere 광석과 텍스트 중심의 단색 UGUI 강화 버튼만 사용했다.
- 수정한 내용: 생성 광석 아트와 Pickaxe·Drill·Robot 아이콘 시트를 크로마키 제거로 투명 PNG로 변환했다. 광석 아트는 `ForgeOreBillboard.mat`을 가진 전면 Quad로, 아이콘은 `MineHudView`의 UGUI `RawImage`로 연결했다.
- 수정 후 상태: 기존 3D Sphere와 Android 스트리핑 대응은 유지하면서 전면 광석 아트가 표시되고, 세 강화 버튼은 각 역할을 구분하는 아이콘을 표시한다.
- 테스트 결과: Unity 컴파일 오류 0건, Editor Play Mode 콘솔 오류 0건, EditMode 테스트 6/6 통과, Android 개발 APK 빌드 성공(오류 0건·경고 1건). 최신 APK 기기 설치는 ADB에서 연결 기기 없음으로 실행하지 못했다.
- 다음 작업: 테스트 기기 연결 후 최신 APK의 비주얼 표시를 재확인하거나, 다음 단계로 광석·스테이지별 아트 변형을 추가한다.

## 수정사항 14

- 기록 시각: 2026-07-16 19:10:00
- 작업 요청 요약: 연결된 Android 기기에서 생성 비주얼이 포함된 최신 APK를 테스트한다.
- 수정 전 상태: Android 개발 빌드는 성공했지만 Task 4의 최신 APK는 기기 미연결로 설치·실행하지 못했다.
- 수정한 내용: 프로젝트 코드·Unity 설정은 변경하지 않았다. 최신 `PocketForge-dev.apk`를 SM-S938N에 설치하고 실행 화면·전면 Activity·프로세스 로그를 확인했다.
- 수정 후 상태: 기기에서 생성 광석 아트와 Pickaxe·Drill·Robot 아이콘이 표시된다.
- 테스트 결과: `adb install -r` 성공, `UnityPlayerGameActivity` 전면 실행, 기기 스크린샷으로 비주얼 표시 확인. 앱 프로세스에서 치명 예외·종료는 없었으나, Unity 서비스 연결 실패(Curl error 7)와 선택적 Play Asset Pack 클래스 부재 로그가 기록됐다.
- 다음 작업: 필요하면 Unity Diagnostics/Cloud 연결과 Play Asset Delivery 사용 여부를 배포 설정 단계에서 검토한다.

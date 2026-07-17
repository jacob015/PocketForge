# WORKING_CONTEXT.md

## Task 11 completion note

- 2026-07-16: Added Iron (stage 4) and Gold (stage 7) ore definitions to the existing content catalog, preserving Copper (stage 1) and Crystal (stage 10).
- Verification: Unity EditMode 18/18 passed, including the catalog milestone selection test, before the Task 12 visual redesign begins.

## Change item 21

- Recorded at: 2026-07-17 22:38:00
- Request: Replace the temporary code-built ore shape with a genuine AI-generated 3D model suitable for the casual mining presentation.
- Applied: Generated an original turquoise crystal ore concept, used Meshy image-to-3D to create a textured GLB, installed `com.unity.cloud.gltfast` 6.19.0 for GLB import, and assigned the imported model to `MineGameController.generatedOrePrefab`. The previous primitive/billboard ore remains as a null-reference fallback.
- Verification: Unity console errors 0, EditMode 18/18 passed, Android development build succeeded (errors 0, warnings 2), and the refreshed APK installed and displayed the colored 3D crystal ore on SM-S938N. No Unity, AndroidRuntime, or glTF runtime error was found in the captured log.
- Next: Continue Task 12 UI readability and visual polish; keep the generated model's mobile memory footprint under review before release builds.

## Change item 22

- Recorded at: 2026-07-17 23:10:00
- Request: Add several genuine ore-model variations while preserving the existing turquoise-crystal ore silhouette, and reduce the prototype feel of the mining HUD layout.
- Applied: Generated original copper, iron, and gold ore concepts, created textured Meshy GLB models, imported them through glTFast, and attached the model reference and scale to each `OreDefinition`. `MineGameController` now resolves the visual from the active ore definition and replaces the model only when the ore stage changes; the scene-level crystal reference remains a fallback. The HUD now uses a compact header, a clearer central mine stage, a broad bottom mine action, and three horizontal upgrade cards to leave the quarry backdrop readable.
- Verification: All four ScriptableObject visual references were validated through Unity serialization; the copper model was reviewed in Scene View; Unity console errors 0; EditMode 18/18 passed. Android development build succeeded (1059.92 MB, errors 0, warnings 2), installed on SM-S938N, launched successfully, and produced no Unity, AndroidRuntime, glTF, or exception log entries in the captured application log.
- Next: Continue Task 12 refinement with the portrait device layout; adjust scale or contrast only if a manual visual review requires it. The large development build size must be optimized before release.

## 수정사항 20

- 기록 시각: 2026-07-16 23:30:00
- 작업 요청 요약: 채굴·강화 결과를 플레이어가 즉시 알 수 있도록 HUD 피드백을 추가한다.
- 수정한 내용: `MiningGameResult`가 광석 파괴 보상과 강화 구매 실패를 명시적으로 전달하도록 확장했다. `MineHudPresenter`는 결과에 따라 보상·강화 성공·크레딧 부족 메시지를 요청하고 `MineHudView`는 짧은 토스트 표시를 담당한다.
- 테스트 결과: Unity 재컴파일 오류 없음, EditMode 17/17 통과. Android 개발 APK 빌드 성공(오류 0, 경고 2), SM-S938N 설치 및 실행 확인.
- 다음 작업: Task 11에서 기존 MiningContentCatalog에 스테이지별 광석 콘텐츠를 확장한다.

## 수정사항 19

- 기록 시각: 2026-07-16 23:15:00
- 작업 요청 요약: 이후 설정 화면 단계에서 구현하기로 미뤄 둔 4개 언어 선택 UI를 실제 게임 HUD에 연결한다.
- 수정 전 상태: `LanguageService`는 한국어·영어·일본어·중국어(간체) 선택값 저장과 화면 갱신 이벤트를 제공하지만, 사용자가 변경할 설정 UI는 없었다.
- 수정한 내용: `MineHudView`에 설정 버튼과 모달을 추가하고 네 개의 원어 언어 버튼을 `LanguageService.SetLanguage`에 연결했다. 설정·언어·닫기 UI 문구도 언어별로 제공하며, 언어 변경 이벤트가 HUD와 열린 설정 모달을 즉시 다시 그린다.
- 수정 후 상태: 선택 언어는 기존 PlayerPrefs 경로에 저장되고 다음 실행에도 유지된다. 별도 Inspector 또는 씬 연결은 필요 없다.
- 테스트 결과: Unity 재컴파일 오류 없음, EditMode 17/17 통과. Android 개발 APK 빌드 성공(오류 0, 경고 2), SM-S938N에서 설정 모달 표시와 English 선택 후 HUD·모달 문구 즉시 전환을 확인했다.
- 다음 작업: 저장 콘텐츠 확장 또는 게임 플레이 피드백을 별도 Task로 승인받아 진행한다.

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

## 수정사항 15

- 기록 시각: 2026-07-16 19:20:00
- 작업 요청 요약: 향후 설정창에서 한국어·영어·일본어·중국어 간체를 선택해 전체 UI를 전환할 수 있는 기반을 만든다.
- 수정한 내용: `com.unity.localization` 1.5.12를 추가하고, `LanguageService`에 네 언어의 코드 기반 UI 문자열 테이블·기기 언어 기본값·PlayerPrefs 저장·변경 이벤트를 구현했다. `MineHudView`는 키를 사용하며 변경 이벤트 때 즉시 다시 렌더링한다.
- 수정 후 상태: 설정창은 `LanguageService.SetLanguage(SupportedLanguage)`만 호출하면 선택값 저장과 현재 화면 갱신을 수행할 수 있다. 실시간 번역 API는 사용하지 않는다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 테스트 11/11 통과. 네 언어의 채굴 버튼 문자열을 테스트로 검증했다.
- 다음 작업: 설정창 UI를 만들 때 언어 버튼을 `LanguageService.SetLanguage`에 연결하고, Unity Localization String Table 에셋으로 카탈로그를 분리한다.
## 수정사항 16

- 기록 시각: 2026-07-16 22:32:21
- 작업 요청 요약: 설정창은 보류하고, 채굴 핵심 루프를 콘텐츠 카탈로그 구조로 확장한다.
- 수정 전 상태: 광석·강화 수치가 `MiningGameConfig` 하나에 집중되어 있었고, 게임 서비스는 해당 수치를 직접 조회했다.
- 수정한 내용: `OreDefinition`, `UpgradeDefinition`, `MiningContentCatalog`를 추가하고, 구리/크리스털 광석 및 세 강화 자산을 생성했다. `MiningGameService`는 단계에 맞는 광석 정의와 강화 정의를 카탈로그에서 조회하며, `MineGameController`는 씬에 연결된 카탈로그를 composition root로 사용한다.
- 수정 후 상태: 10단계부터 크리스털 광석 정의(내구도 55, 보상 배율 3/7, 보라색 시각 색상)가 선택된다. 설정 언어 선택 UI는 만들지 않았고 기존 다국어 기반만 유지했다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 12/12 통과, Editor Play Mode 진입·종료 후 Console 오류/경고 0건, Android 개발 APK 빌드 성공(오류 0건/경고 2건, 142.41초), SM-S938N `adb install -r` 성공 및 앱 프로세스 실행·화면 표시 확인, Unity/AndroidRuntime 오류 logcat 0건.
- 다음 작업: 자동 채굴·보상 확장 또는 저장 데이터 버전 보강을 별도 Task로 승인받아 진행한다. 설정창은 해당 구현 단계까지 보류한다.
## 수정사항 17

- 기록 시각: 2026-07-16 22:47:58
- 작업 요청 요약: 자동 채굴을 오프라인 보상 루프로 확장하되, 전체 UI 리디자인과 설정창은 보류한다.
- 수정 전 상태: 드릴은 앱 실행 중에만 자동 채굴력을 제공했고, 저장 데이터에는 마지막 저장 시각이 없었다.
- 수정한 내용: 저장 데이터 버전을 3으로 올려 Unix 저장 시각을 보존하고, `MiningGameService.ApplyOfflineReward`가 드릴의 초당 피해량과 현재 광석의 Credits/내구도 비율로 보상을 산정하게 했다. 카탈로그의 `maxOfflineRewardSeconds`는 14,400초(4시간)로 설정했다. 재접속 보상은 HUD 상단에 표시한다.
- 수정 후 상태: 드릴 레벨 0이거나 저장 시각이 없으면 보상은 없으며, 보상 시간은 4시간을 초과하지 않는다. 전체 UI 폴리싱은 후속 Task로 분리한다.
- 테스트 결과: Unity 컴파일 오류 수정 후 관련 Console 오류 0건, EditMode 13/13 통과, Editor Play Mode 진입·종료 확인, Android 개발 APK 빌드 성공(오류 0건/경고 2건, 112.40초), SM-S938N 설치·실행 및 Unity/AndroidRuntime 오류 logcat 0건.
- 다음 작업: 게임 화면의 시각 계층·버튼·패널을 상용 게임 수준으로 다듬는 UI 폴리싱 Task를 별도 승인받아 진행한다.
## 수정사항 18

- 기록 시각: 2026-07-16 23:00:00
- 작업 요청 요약: 프로토타입 HUD를 모바일 게임처럼 읽기 쉬운 시각 계층으로 다듬는다.
- 수정한 내용: 상단 정보, 채굴 행동, 강화 목록에 별도 표면 패널·그림자·버튼 외곽선을 적용하고 오프라인 보상 안내를 강조 패널로 변경했다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 13/13 통과, Android APK 빌드 성공(오류 0건), SM-S938N 설치·실행 및 화면 캡처로 레이아웃 확인.
- 다음 작업: 설정창과 언어 선택 UI 또는 성장 콘텐츠 확장을 별도 승인받아 진행한다.

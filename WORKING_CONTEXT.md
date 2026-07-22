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
## 변경사항 19

- 기록 시각: 2026-07-18
- 작업 요청 요약: AI 생성 UI 이미지를 실제 HUD에 적용하고, 약 157MB였던 Android APK를 50MB 이하로 최적화한다.
- 작업 전 상태: 4개 Meshy GLB가 각 27~30MB의 빌드 데이터를 차지했고, 런타임 HUD는 단색 프로토타입 패널을 사용했다.
- 변경 내용: UnityMeshSimplifier 3.1.1 기반 재생성 가능한 `OreAssetOptimizer`를 추가했다. 4개 광석을 약 17,196~22,630 triangles, 512px ASTC 6x6 텍스처, URP Lit 네이티브 prefab으로 변환하고 OreDefinition 참조를 교체했다. AI 생성 HUD 레퍼런스와 투명 UI 키트를 추가하고 `MineHudView`에서 9-slice 아틀라스로 적용했다. 원본 GLB fallback 참조와 배경을 가리던 프로토타입 glow 레이어를 제거했다.
- 검증 결과: 씬·광석 정의의 GLB 의존성 0건, Unity 컴파일 오류 0건, EditMode 18/18 통과. 비개발 IL2CPP Android APK는 37,146,673바이트(35.43MiB)이며 APK v2 서명 검증과 SM-S938N 설치·실행에 성공했다. 기기에서 채굴 버튼 2회 입력 후 광석 체력이 62에서 56으로 감소했다.
- 알려진 로그: Unity 6 Android 런타임이 선택적 Play Asset Pack 클래스 `AssetPackManager`를 탐색하며 ClassNotFoundException 한 건을 기록하지만, 앱 기동·렌더링·입력은 정상이다. 배포 설정 단계에서 Play Asset Delivery 사용 여부와 함께 정리한다.
- 다음 작업: Task 12의 설정 버튼·모달을 새 UI 언어에 맞게 폴리싱하고 사용자 화면 피드백을 반영한다.

## 변경사항 20

- 기록 시각: 2026-07-18
- 작업 요청 요약: AI로 만든 HUD 레퍼런스의 완성도를 실제 게임에 그대로 반영하고, Free Aspect/Game View 대신 휴대폰 비율의 Device Simulator를 기준으로 검수한다.
- 작업 전 상태: 실제 HUD는 레퍼런스의 시각 계층을 일부만 사용해 광석이 작고 화면 중앙이 비었으며, 배경 UI가 3D 광석과 겹쳤다. Device Simulator는 일반 휴대폰이 아닌 Foldable Device(960×2658)를 선택하고 있었다.
- 변경 내용: HUD를 상단 코인·크레딧·심도 바, 큰 중앙 광석, 광석 상태와 진행 바, 대형 주황 채굴 버튼, 대형 장비 이미지와 레벨 핍·녹색 액션 스트립을 갖춘 강화 카드 3개로 재배치했다. 설정 버튼·설정 모달에도 같은 UI 아틀라스를 적용했다. 채굴 배경은 카메라 뒤쪽 3D 쿼드로 옮기고 화면비 변화에 맞춰 자동 리사이즈하며, 배경을 가리던 기존 Ground 메시를 비활성화했다. 광석 표시 위치와 배율은 직렬화된 presentation 값으로 분리했다.
- 검증 결과: 중앙 펀치홀 Android Device Simulator(1440×3088) 세로 화면에서 레퍼런스 구성을 확인했다. Unity 컴파일 오류 0건, EditMode 18/18 통과. 비개발 IL2CPP APK는 37,146,409바이트이며 오류 0건으로 빌드됐다. SM-S938N에 설치·실행해 전체 HUD와 설정 모달을 확인했고, 채굴 버튼 2회 입력 후 광석 체력이 62에서 56으로 감소했다. 검증 구간의 Unity/AndroidRuntime 오류 로그는 0건이다.
- 다음 작업: 사용자 비주얼 확인 후 Task 12를 확정하고 PROJECT_PLAN 순서에 따라 테스트 광고 보상·실패 처리 Task로 진행한다.

## 수정사항 21

- 기록 시각: 2026-07-18 01:55:59
- 작업 요청 요약: 애니메이션·이펙트·전용 폰트는 기능 구현 뒤로 미루고, 업그레이드 버튼 전용 이미지와 HUD의 겹침·잘림·불명확한 비율을 먼저 수정한다.
- 수정 전 상태: 설정 버튼이 크레딧·심도 패널과 겹쳤고, 광석 내구도 바의 동전 장식이 화폐 UI로 오해될 수 있었다. 채굴 버튼은 가로로 길며 텍스트와 작은 아이콘을 함께 사용했고, 강화 액션은 코드 색상 패널이 카드 하단 경계에 붙어 있었다. 휴대폰 Safe Area 전용 루트가 없었다.
- 수정한 내용: 기존 UI 키트와 HUD 레퍼런스를 입력으로 글자 없는 라임색 업그레이드 버튼을 AI 생성하고 투명 PNG로 정리해 Android 512px ASTC 6x6 설정으로 적용했다. `MineHudView`에 Safe Area 루트를 추가하고 상단 자원 패널과 설정 버튼을 분리했다. 동전 장식형 진행 바를 일반 광석 내구도 바로 교체하고 수치 라벨을 바로 위에 배치했다. 채굴 버튼은 세로 높이를 늘리고 텍스트를 제거한 뒤 중앙 대형 곡괭이 아이콘만 표시한다. 강화 카드 폭·높이·핍·액션 버튼 여백을 조정해 화면 밖 돌출을 제거했다.
- 수정 후 상태: 중앙 펀치홀 Android와 SM-S938N 화면에서 상단·중앙·하단 UI가 안전 영역 안에 들어가며, 설정 모달은 상단 정보 바 아래에서 열린다. 클릭 애니메이션, 업그레이드 성공 이펙트, 전용 폰트는 의도적으로 변경하지 않았다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 18/18 통과. 비개발 IL2CPP APK는 37,178,105바이트로 오류 0건·경고 2건 빌드됐다. SM-S938N 설치·실행, 설정 모달 열기·닫기, 채굴 입력, 곡괭이 강화 Lv.2→Lv.3 및 새 버튼 표시를 확인했으며 검증 구간 Unity/AndroidRuntime 오류 로그는 0건이다.
- 남은 작업: PROJECT_PLAN의 테스트 광고 보상·실패 처리를 다음 기능 Task로 진행하고, 모든 기능 완료 후 클릭 애니메이션·업그레이드 이펙트·폰트를 최종 폴리싱한다.

## 수정사항 22

- 기록 시각: 2026-07-22 16:24:00
- 작업 요청 요약: Google Play 배포 준비를 위해 Android 패키지명과 버전을 확정하고, 출시용 키스토어를 바탕화면에 생성해 서명 AAB를 검증한다.
- 수정 전 상태: Android 패키지명은 Unity URP 템플릿 기본값이었고 출시 키스토어가 없었다. 버전명은 `0.1.0`, 버전 코드는 `1`이었다.
- 수정한 내용: Android 패키지명을 `com.jacob015.pocketforge`로 변경하고 버전 `0.1.0 (1)`을 확정했다. 바탕화면의 `PocketForge-Signing` 폴더에 RSA 4096 JKS 키스토어와 비공개 복구 정보를 생성했다. `PocketForgeAndroidBuild`를 추가해 키 경로와 비밀번호를 환경변수로만 주입하고 서명 AAB를 만들 수 있게 했다. 키스토어와 비밀번호 파일은 프로젝트 및 Git에 포함하지 않았다.
- 수정 후 상태: 로컬 Unity Android 설정은 `pocketforge` 별칭의 사용자 키스토어를 가리킨다. `Builds/Android/PocketForge-0.1.0.aab`이 생성됐으며 파일 크기는 37,091,264바이트다.
- 테스트 결과: 별도 임시 Unity 프로젝트에서 Release AAB 빌드 성공을 확인했다. Bundletool 검증 성공, 매니페스트 패키지 `com.jacob015.pocketforge`, 버전명 `0.1.0`, 버전 코드 `1`을 확인했다. `jarsigner` 검증이 성공했고 AAB 인증서와 키스토어 인증서의 SHA-256 지문이 일치했다.
- 남은 작업: 키스토어와 복구 정보를 비밀번호 관리자 또는 오프라인 저장소에 추가 백업한다. 다음 구현 작업은 PROJECT_PLAN의 테스트 광고 보상·실패 처리다.

## 수정사항 23

- 기록 시각: 2026-07-22 17:50:00
- 작업 요청 요약: PROJECT_PLAN 순서에 따라 테스트 광고의 보상·실패 처리를 구현하고 서명 Android 빌드와 실제 기기에서 검증한다.
- 수정 전 상태: 프로젝트에 광고 SDK·광고 서비스 경계·보상형 광고 UI·전면 광고 노출 정책이 없었다.
- 수정한 내용: OpenUPM으로 Google Mobile Ads Unity Plugin 11.3.0을 고정하고 공식 테스트 앱 ID와 보상형·전면 광고 단위 ID를 적용했다. `IAdsService`와 `GoogleMobileAdsService`로 SDK 의존성을 격리하고 `MineAdCoordinator`가 HUD, 게임 서비스, 저장을 연결한다. 보상형 광고는 현재 일반 광석 보상의 5배를 완료 콜백에서만 지급하며, 로드 실패 시 버튼을 수동 재시도로 전환한다. 전면 광고는 광석 5개 파괴 및 180초 쿨다운을 모두 만족할 때만 시도하고 실패해도 진행을 막지 않는다. 네 언어 광고 상태 문구와 정책·보상 단위 테스트를 추가했다.
- 수정 후 상태: 광고 구현은 게임 규칙·노출 정책·SDK 어댑터·UI 조정 계층으로 분리됐다. 테스트 ID만 포함하며 실제 광고 ID, UMP 동의 흐름, Play Console 데이터 공개, 광고 제거 상품은 후속 배포·결제 Task 범위다.
- 테스트 결과: 별도 Android 타깃 Unity 프로젝트에서 컴파일 성공 및 EditMode 21/21 통과. Release IL2CPP AAB는 41,534,886바이트(39.61MiB)로 50MiB 목표 이내이며 Bundletool 구조, `com.jacob015.pocketforge` 0.1.0(1), AdMob 테스트 앱 ID, 출시 키 인증서 일치를 확인했다. SM-S938N 설치·기동과 SDK 초기화·테스트 광고 요청·실패 시 `광고 다시 받기` UI 전환을 확인했다. 기기의 `dns.adguard.com` 사설 DNS 환경에서 Google SDK가 `Unable to obtain a JavascriptEngine`을 반환해 실제 광고 표시와 보상 완료 콜백은 DNS 해제 후 재검증이 필요하다.
- 다음 작업: 기기 네트워크 설정을 승인받아 테스트 광고 완료 콜백을 재검증한 뒤, PROJECT_PLAN의 인앱 결제 Task를 진행한다.

## 수정사항 24

- 기록 시각: 2026-07-22 19:35:00
- 작업 요청 요약: PROJECT_PLAN의 인앱 결제 단계로 진행해 광고 제거 비소모성 상품, 구매 복원, 다국어 설정 UI와 Android 기기 검증을 구현한다.
- 수정 전 상태: 광고 SDK와 전면·보상형 정책은 있었지만 결제 SDK, 상품 카탈로그, 영구 광고 제거 권한, 구매·복원 UI가 없었다.
- 수정한 내용: Unity IAP 5.4.1을 추가하고 `remove_ads`를 비소모성 상품으로 등록했다. `IIapService`, `UnityIapService`, `MineIapCoordinator`를 도입해 SDK와 게임 상태를 분리했다. 저장 데이터를 버전 4로 올려 `adsRemoved` 권한을 영속화하고, 권한 저장 성공 후에만 Pending 주문을 승인한다. 복원·실패·취소·보류 상태와 현지화 가격을 설정창에 네 언어로 표시한다. 광고 제거 권한은 강제 전면 광고만 비활성화하며 선택형 보상 광고는 유지한다. Unity Services Core와 Google Ads의 Kotlin 중복 클래스는 Base Gradle 템플릿에서 1.8.22로 정렬했다.
- 테스트 결과: 전체 자산이 포함된 격리 Unity 프로젝트에서 컴파일과 EditMode 29/29가 통과했다. 서명 Release AAB `Builds/Android/PocketForge-0.1.0-iap-release3.aab`은 48,608,517바이트(46.36MiB)이며 Bundletool 구조, `com.jacob015.pocketforge` 0.1.0(1), jarsigner 검증이 통과했다. SM-S938N에 기기별 APK 세트를 설치해 앱 프로세스 유지, 설정창의 현지화 상품 가격 `$0.01`, 구매·복원·닫기 버튼의 비겹침을 확인했다. 알려진 미사용 Play Asset Delivery 클래스 탐색 로그 1건 외 AndroidRuntime 크래시는 없었다.
- 남은 작업: 실제 구매 버튼 입력과 복원은 금전 및 Play Console 내부 테스트 설정과 관련되므로 별도 승인 후 수행한다. 환불·취소 권한 회수와 서버 영수증 검증은 운영 백엔드 범위로 남긴다.
- 다음 작업: Play Console 실제 구매·복원 검증을 승인받거나 PROJECT_PLAN의 Jenkins Android AAB 자동화 단계로 진행한다.

## 수정사항 25

- 기록 시각: 2026-07-22 20:10:15
- 작업 요청 요약: 광고·인앱 결제를 먼저 완료하고, 이후 UI 폴리싱, 게임 콘텐츠 추가, 밸런싱 순서로 진행하도록 남은 프로젝트 계획을 확정한다.
- 수정 전 상태: `PROJECT_PLAN.md`는 초기 2주 마일스톤 순서를 유지하고 있었고, `TASKS.md`의 현재 작업은 IAP 클라이언트 완료와 실제 구매·복원 대기를 가리켰다. UI 폴리싱·콘텐츠·밸런싱의 선후 관계는 현재 작업 기준으로 명시되지 않았다.
- 수정한 내용: `TASKS.md`의 현재 작업을 광고 실기기 완료 검증으로 지정하고 승인된 잔여 작업 9단계를 기록했다. `PROJECT_PLAN.md`에 광고·IAP 완료 후 UI 폴리싱, 콘텐츠 확장, 밸런싱, QA, 자동화, 비공개 테스트, 출시 준비 순서를 추가했다.
- 수정 후 상태: 광고와 IAP가 모두 완료되기 전에는 UI 폴리싱을 시작하지 않으며, 폴리싱 후 콘텐츠를 추가하고 콘텐츠 확정 후 밸런싱하는 순서가 기준 계획으로 확정됐다.
- 테스트 결과: Markdown 변경 내용과 작업 순서만 확인했다. Unity 코드·씬·패키지·Android 빌드는 변경하거나 테스트하지 않았다.
- 남은 작업: 현재 작업인 보상형 광고 실기기 재생·보상 지급과 전면 광고 정책을 검증한다.

## 수정사항 26

- 기록 시각: 2026-07-22 20:22:38
- 작업 요청 요약: 사설 DNS와 Codex 재시작 후 Unity MCP 연결 및 Android 실기기 광고 동작을 다시 검증한다.
- 수정 전 상태: 광고 SDK·보상·전면 광고 정책은 구현돼 있었지만 `dns.adguard.com` 차단으로 실제 광고 완료 콜백은 확인하지 못했다. Unity MCP 서버도 이전 Codex 세션에는 도구로 등록되지 않았다.
- 수정한 내용: Codex 재시작 후 Unity MCP의 단일 `DrillProject` 인스턴스와 준비 상태를 확인했다. SM-S938N에서 공식 보상형 테스트 광고를 완료하고 닫은 뒤 보상 지급과 재로딩을 확인했으며, 광석 5개 파괴·180초 조건으로 공식 전면 테스트 광고를 표시하고 게임으로 복귀했다.
- 수정 후 상태: 보상형 광고 로드·표시·완료·보상·재로딩과 전면 광고 정책·복귀가 실기기에서 정상 동작한다. 현재 작업은 IAP Play Console 내부 테스트 준비로 이동했다.
- 테스트 결과: 보상형 광고 완료 후 크레딧 `20 → 80 C`, 전면 광고 후 심도 `11` 진행 유지, 앱 프로세스 유지, AndroidRuntime 크래시와 `JavascriptEngine` 오류 없음, Unity MCP EditMode 29/29 통과를 확인했다. Resolver 이후 Unity Console에는 기존 obsolete API 경고와 테스트 러너의 결과 저장 로그만 남았다.
- 남은 작업: Play Console에서 `remove_ads` 비소모성 상품과 라이선스 테스터·내부 테스트 트랙을 구성한 뒤 실제 구매·복원·재설치 권한 복구를 검증한다.

## 수정사항 27

- 기록 시각: 2026-07-22 21:26:32
- 작업 요청 요약: Play Console 개발자 계정 인증을 기다리는 동안 IAP 복원 안정성과 업로드용 AAB 준비를 먼저 완료한다.
- 수정 전 상태: Unity IAP 5.4.1의 `RestoreTransactions`가 복원 결과를 `OnPurchasesFetched`로 전달하지만, 성공 콜백에서 `FetchPurchases`를 한 번 더 호출해 동일 세션에서 구매 목록을 중복 조회할 수 있었다. IAP 조정자 테스트는 구매 권한 저장 순서를 다뤘지만 복원 권한 저장, 중복 구매 차단, 이벤트 해제 회귀 사례는 없었다.
- 수정한 내용: `UnityIapService.RestorePurchases`의 성공 후 추가 `FetchPurchases`를 제거해 Unity IAP 5.4.1 복원 흐름에 맞췄다. `MineIapCoordinatorTests`에 복원 권한 저장, 기존 권한 보유자의 중복 구매 차단, Dispose 후 이벤트 무시 테스트를 추가했다. 기존 출시 키를 메모리에만 주입해 업로드 대기용 `PocketForge-0.1.0-iap-release4.aab`을 생성한 뒤 비밀번호를 Unity 메모리에서 비웠다.
- 수정 후 상태: 복원 요청은 Unity IAP가 전달하는 단일 구매 목록 이벤트를 사용하며 IAP EditMode 회귀 범위가 3개 늘었다. 새 AAB은 `com.jacob015.pocketforge` 버전 `0.1.0 (1)` 설정과 기존 출시 키 서명을 유지한다.
- 테스트 결과: 두 수정 스크립트 진단 경고·오류 0건, Unity Console 컴파일 오류 0건, EditMode 32/32 통과. Release AAB 빌드는 오류 0건·경고 4건으로 성공했으며 실제 파일은 48,610,452바이트(46.36MiB), SHA-256 `AAEE9D47B101A1173067E88005384B1C2C722BBDB841856380ADEDCDD5C81CA5`다. AAB의 `BundleConfig.pb`, base manifest·resources, 서명 파일 존재를 확인했고 생성된 release manifest는 `com.jacob015.pocketforge` 0.1.0(1), AAB 인증서 SHA-256은 기존 출시 키와 일치했다. `jarsigner -verify` 종료 코드는 0이었다.
- 남은 작업: Play Console 개발자 계정 인증 완료 후 `remove_ads` 상품을 게시하고 라이선스 테스터·내부 테스트 트랙을 설정한다. 이후 Play 설치본에서 실제 테스트 구매·복원·재설치 권한 복구·전면 광고 제거를 검증한다.

## 수정사항 28

- 기록 시각: 2026-07-22 23:15:00
- 작업 요청 요약: Play Console 인증 대기 중 UI·그래픽 폴리싱을 선행하고, 버튼 애니메이션·절제된 성공 이펙트·폰트·전용 메시지 배경·오디오/접근성 설정·모바일 화면비 대응을 구현한다.
- 수정 전 상태: HUD는 이전 AI 레퍼런스의 큰 구조를 반영했지만 상단 정보와 광고 버튼이 좁았고, 강화 성공은 텍스트 중심이었다. 버튼 피드백, BGM·효과음 설정, 진동·모션 감소, 전용 메시지 패널, 화면비별 실측 검증이 없었다.
- 수정한 내용: 실제 1440×3088 Simulator 화면과 현재 배경을 입력으로 HUD·설정창 완성 콘셉트를 생성했다. `MineHudView`를 Safe Area·0.5 화면비 매칭으로 조정하고 상단 정보·광고·채굴 버튼·강화 카드 비율을 정리했다. 모든 버튼에 unscaled 눌림/복귀 애니메이션을 붙이고 강화 성공에는 카드 펄스와 사전 생성된 8개 스파크를 적용했으며 기존 성공 텍스트는 제거했다. AI 생성 전용 피드백 패널을 투명 PNG와 런타임 9-slice로 적용했다. 네 언어를 표시할 수 있는 동적 OS 폰트와 텍스트 위계를 적용하고, 설정창에 BGM·효과음 볼륨/음소거, 진동, 모션 감소를 추가해 PlayerPrefs에 저장한다. 오디오 컨트롤러는 클립 슬롯을 분리하고 설정 변경을 즉시 반영한다.
- 수정 후 상태: HUD·설정·프레젠터·오디오·사용자 설정이 각 역할로 분리됐으며, 성공 효과는 오브젝트 풀을 사용해 반복 입력 시 할당을 억제한다. 오디오 생성 제공자가 구성되지 않아 라우팅과 UI는 동작하지만 실제 BGM·효과음 클립은 아직 할당되지 않았다.
- 테스트 결과: Unity Console 컴파일·20:9 Play Mode 오류 0건, EditMode 39/39 통과. Device Simulator를 각 기기로 전환하고 Play Mode를 재시작한 뒤 16:9 750×1334, 19.5:9 1080×2340, 20:9급 1440×3088에서 상단 정보·설정·광고·채굴·강화 카드가 Safe Area 안에 있음을 좌표로 확인했다. 설정 카드는 16:9와 19.5:9에서도 Safe Area 내부였다. PlayMode 등록 테스트는 0건이다. 새 서명 AAB 빌드는 보안상 비워 둔 키 비밀번호를 재주입하지 않아 완료하지 않았으며, 마지막 검증 AAB는 48,610,452바이트(46.36MiB)다.
- 남은 작업: 사용자의 실제 화면 검수에 따라 Task 12 비주얼을 추가 보정한다. BGM·효과음 클립은 라이선스가 명확한 소스 또는 구성된 생성 제공자가 확보되면 할당한다. Play Console 인증 완료 후 실제 구매·복원 검증을 재개하고, 새 서명 AAB는 배포 단계에서 환경변수로 키를 주입해 생성한다.

## 수정사항 29

- 기록 시각: 2026-07-23 01:58:00
- 작업 요청 요약: AI 레퍼런스와 실제 인게임 HUD의 차이를 최대한 줄이고, 휴대폰 화면비와 Android 실기기에서 최종 폴리싱 결과를 검증한다.
- 수정 전 상태: 1차 폴리싱으로 색상과 기본 프레임은 개선됐지만 상단 자원·광고 영역의 시각 밀도, 광석과 진행 바·채굴 버튼의 크기 관계, 강화 카드의 아이콘·레벨·비용 계층이 레퍼런스보다 단순했다. 화면비 검증은 수동 좌표 확인 중심이었다.
- 수정한 내용: `MineHudView`의 세로 기준 스케일을 너비 매칭으로 고정하고 상단 자원 바, 설정·광고 보상 버튼, 광석 상태 배지와 진행 바, 주황 채굴 버튼, 강화 카드 3개의 크기·간격·앵커를 다시 조정했다. 설정·추가·광고·크리스털 아이콘을 하나의 AI 생성 HUD 아이콘 시트로 교체하고 Android ASTC 6x6 설정을 적용했다. 1920·2340·2400 높이와 1080 폭에서 주요 HUD 영역의 순서·경계·비겹침을 검사하는 EditMode 테스트 5개를 추가했다.
- 수정 후 상태: 1440×3088 Simulator와 SM-S938N에서 상단 자원·설정·광고, 중앙 광석, 상태 바, 채굴 버튼, 하단 강화 카드가 Safe Area 안에 유지된다. 테스트 설치는 기존 릴리스 앱과 저장 데이터를 보존하기 위해 `com.jacob015.pocketforge.dev`로 별도 설치했으며, 프로젝트 패키지명과 Custom Keystore 사용 설정은 검증 후 원래 값으로 복구했다.
- 테스트 결과: Unity 컴파일 및 Play Mode 오류 0건, EditMode 44/44 통과. Android 개발 APK `PocketForge-hud-device-test.apk`는 오류 0건·경고 3건으로 빌드됐고 실제 파일은 63,042,742바이트이며 설치·기동에 성공했다. 빌드 경고는 개발 빌드 심볼 설정, Android 빌드에서 제외되는 iOS 광고 플러그인, Localization Android App Info 미설정 안내다. SM-S938N에서 채굴 입력 후 광석 내구도 `10/10 → 09/10`, 설정 모달 열기와 전체 화면 비겹침을 확인했다. AndroidRuntime 치명적 크래시는 없었다. ADB 사이드로드 `.dev` 패키지의 IAP 상품 조회 실패와 Unity의 선택적 Play Asset Pack 클래스 탐색 로그는 예상된 비치명 로그다. 마지막 서명 릴리스 AAB는 48,610,452바이트로 50MB 목표 안에 있다.
- 남은 작업: 사용자 최종 화면 검수에서 추가 보정 요청이 없으면 Task 12를 완료 처리하고 콘텐츠 확장 단계로 이동한다. Play Console 인증 완료 후 실제 Play 설치본에서 구매·복원 검증을 재개한다. 실제 BGM·효과음 클립은 라이선스가 명확한 리소스가 확보되면 할당한다.

# WORKING_CONTEXT.md

## 수정사항 33

- 기록 시각: 2026-07-26 18:34:07
- 작업 요청 요약: 승인된 HUD·설정 스냅샷과 실제 게임 화면의 크기·위치·간격을 픽셀 단위에 가깝게 동기화한다.
- 수정 전 상태: 분해한 UI 자산과 전체 구성은 적용됐지만 상단 HUD가 위로 치우치고, 광석 라벨·채굴 버튼·설정 카드·슬라이더·닫기 버튼의 비율과 위치가 원본과 어긋나 있었다.
- 수정한 내용: 853×1844 원본 좌표를 1080 기준 캔버스로 환산해 주요 HUD와 설정 `RectTransform`을 보정했다. 설정 카드의 상단은 유지하면서 높이와 하단 여백을 줄였고, 슬라이더 폭과 닫기 버튼 크기·위치를 원본 경계에 맞췄다. 승인 좌표 회귀 테스트도 추가했다.
- 수정 후 상태: 1440×3120 SM-S938N 캡처를 원본과 동일 해상도로 축소한 반투명 오버레이에서 HUD 외곽, 광고 버튼, 광석 게이지, 채굴 버튼, 강화 카드, 설정 외곽과 행 경계가 거의 포개진다. 실제 요구사항인 4개 언어 버튼은 원본의 지구본 중심 행과 의도적으로 다르게 유지했다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 45/45 통과. `.dev` 비개발 APK 50,050,511바이트(47.73MiB)를 빌드해 SM-S938N에 설치·기동했고 채굴 `10/10 → 09/10`, 설정창 표시, AndroidRuntime 치명적 크래시 없음 확인. 선택적 Play Asset Pack 클래스 탐색 로그는 기존과 동일하게 남는다.
- 다음 작업: 사용자 최종 화면 승인 후 Task 12를 닫고 게임 내 콘텐츠 확장으로 이동한다. 기능 범위가 확정되면 현재 비어 있는 상단 카운터 슬롯 등 의미 없는 UI를 제거하거나 실제 자원에 연결한다.

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

## 수정사항 30

- 기록 시각: 2026-07-26 15:46:41
- 작업 요청 요약: Play Console 인증과 결제 프로필 구성을 마친 뒤 `remove_ads` 비소모성 상품을 내부 테스트 트랙에서 실제 구매·복원하고, 재설치 권한 복구와 광고 동작을 SM-S938N에서 검증한다.
- 수정 전 상태: IAP 클라이언트와 업로드용 AAB은 준비돼 있었지만 Play Console 상품·테스터 구성이 끝나지 않아 실제 Play 구매, 복원, 재설치 후 권한 복구는 검증하지 못한 상태였다.
- 수정한 내용: Play Console에서 내부 테스트 릴리스 `0.1.0-internal-1`, 내부 라이선스 테스터 계정, 비소모성 상품 `remove_ads`와 구매 옵션 `standard`를 활성화했다. SM-S938N에 내부 테스트 링크를 통해 Play 배포본을 설치하고 Google Play의 무청구 라이선스 테스트 카드로 구매했다. 구매 후 보상형 광고를 완료했으며, 앱을 삭제하고 Play에서 다시 설치해 자동 권한 복구와 명시적 `구매 복원` 버튼을 확인했다. 쿨다운 경과 뒤 광석 5개를 파괴해 강제 전면 광고 제거도 확인했다.
- 수정 후 상태: Play 설치본의 설정창은 현지화 가격 `₩1,100`을 조회하고 구매 직후·명시적 복원 후·삭제 및 재설치 후 모두 `구매 완료`를 표시한다. 광고 제거 권한은 강제 전면 광고에만 적용되고 보상형 광고는 선택적으로 계속 사용할 수 있다.
- 테스트 결과: 패키지 `com.jacob015.pocketforge` 0.1.0(1)의 설치 출처가 `com.android.vending`임을 확인했다. Google Play 결제창에 `테스트 주문이므로 청구되지 않습니다`가 표시된 상태에서 구매가 완료됐다. 구매 후 공식 보상형 테스트 광고 표시·보상 지급, 재설치 후 자동 권한 복구, 명시적 복원 후 `구매 완료` 복귀를 확인했다. 앱 활성 180초 이상인 상태에서 보석 수가 `0 → 5`가 되도록 광석 5개를 파괴했으며 전면 광고 없이 `UnityPlayerGameActivity` 포커스와 게임 진행이 유지됐다. 캡처 구간에 AndroidRuntime 치명적 크래시와 Unity IAP 구매 조회·복원 실패 로그는 없었다.
- 남은 작업: 사용자의 Task 12 최종 화면 검수 결과에 따라 필요한 시각 보정을 수행하거나 Task 12를 닫고 게임 내 콘텐츠 확장으로 이동한다. 실제 BGM·효과음 클립은 라이선스가 명확한 리소스를 확보한 뒤 할당한다.

## 수정사항 31

- 기록 시각: 2026-07-26 15:51:32
- 작업 요청 요약: 공개 Git 저장소에 포함될 IAP 검증 기록에서 개인 테스터 이메일을 마스킹한 뒤 푸시한다.
- 수정 전 상태: 아직 원격에 푸시되지 않은 로컬 IAP 검증 커밋의 `WORKING_CONTEXT.md`에 테스터 이메일 원문이 1회 포함돼 있었다.
- 수정한 내용: 테스터 이메일 원문을 개인 식별이 불가능한 `내부 라이선스 테스터 계정`으로 대체했다.
- 수정 후 상태: 현재 작업 트리와 푸시 예정 커밋에는 해당 이메일 원문이 남아 있지 않으며 IAP 검증 사실은 유지된다.
- 테스트 결과: 추적 파일과 푸시 예정 커밋 전체에서 이메일 원문을 검색해 0건임을 확인한다. Unity 코드·씬·설정은 변경하지 않아 Unity 테스트는 실행하지 않는다.
- 남은 작업: 마스킹 검증 후 마지막 미푸시 커밋을 amend하고 `origin/main`에 푸시한다.

## 수정사항 32

- 기록 시각: 2026-07-26
- 작업 요청 요약: 제공된 HUD·설정 스냅샷의 UI 요소를 개별 이미지로 분해하고, 원본의 비율과 위치를 기준으로 실제 게임 화면에 전부 적용한다.
- 수정 전 상태: 이전 폴리싱으로 HUD의 전체 흐름은 레퍼런스와 가까워졌지만, 다수의 표면은 공용 아틀라스 또는 코드 색상 패널을 사용했고 설정창 행·언어 버튼·결제 영역은 레퍼런스보다 프로토타입에 가까웠다.
- 수정한 내용: imagegen으로 글자 없는 HUD 부품·설정 부품·아이콘 시트 3개를 생성하고 크로마키를 제거해 47개 투명 PNG로 분리했다. 런타임 로딩과 Sprite/9-slice 캐시를 담당하는 `MineUiSkin`을 추가하고 `MineHudView`가 상단 자원 바, 설정·광고 버튼, 광석 배지·진행 바, 채굴 버튼, 강화 카드·액션 버튼, 설정 모달·행·슬라이더·토글·네 언어·결제·복원·닫기 자산을 이 스킨으로 조립하도록 변경했다. 설정창 언어 버튼은 2×2에서 1열 4개로 바꾸고 진동·모션 행은 아이콘 중심으로 정리했다. 내부 크로마 잔상과 보라색 생성 노이즈를 제거하고 최종 HUD·설정 실기기 스크린샷을 `Assets/PocketForge/Art/Generated/UI/Review`에 남겼다.
- 수정 후 상태: 1440×3088 SM-S938N 화면에서 상단 HUD, 중앙 광석, 61% 지점의 내구도 바, 68% 지점의 채굴 버튼, 하단 강화 카드가 Safe Area 안에 유지된다. 설정 모달은 오디오·진동·모션 감소·네 언어·광고 제거·구매 복원·닫기를 한 화면에 표시하며 배경 HUD와 겹치거나 화면 밖으로 잘리는 요소가 없다. 출시 패키지명 `com.jacob015.pocketforge`, Custom Keystore, AAB 설정은 기기 검증 뒤 원래 값으로 복구했다.
- 테스트 결과: Unity 컴파일 오류 0건, EditMode 44/44 통과. 비개발 IL2CPP 기기 검증 APK `PocketForge-ui-final-device-test.apk`는 51,918,068바이트(49.51MiB)이며 `com.jacob015.pocketforge.dev`로 SM-S938N에 설치·기동했다. 설정 모달 열기·닫기와 채굴 입력 후 광석 내구도 `10/10 → 09/10`을 확인했다. AndroidRuntime 치명적 크래시는 없었고, 선택적 Play Asset Pack 클래스 탐색 로그는 기존과 동일한 비치명 로그다.
- 남은 작업: 사용자 최종 화면 승인 전까지 Task 12를 진행 중으로 유지한다. 승인되면 Task 12를 닫고 게임 내 콘텐츠 확장 단계로 이동한다. 이번 검증은 Play 설치본과 구매 권한을 보존하기 위한 별도 `.dev` APK이며 새 출시 AAB은 생성하지 않았다.

## 수정사항 33

- 기록 시각: 2026-07-26
- 작업 요청 요약: 설정창은 더 이상 수정하지 않고, 실기기 HUD에서 눌리거나 비율이 맞지 않는 요소를 정리하며 채굴 보상 문구를 배경 없는 캐주얼 텍스트로 바꾼다. Android 빌드 검증은 이번 작업에서 제외한다.
- 수정 전 상태: 강화 카드의 네 `RawImage`가 서로 다른 원본 종횡비에도 고정 사각형으로 늘어나 곡괭이·드릴·로봇이 변형됐다. 레벨·비용·업그레이드 버튼의 세로 영역이 겹쳤고, 광석 이름 배지가 진행 바보다 뒤에 그려져 겹친 테두리가 양옆 돌출처럼 보였다. 채굴 보상은 기존 생성 패널 위에 일반 HUD 폰트로 표시됐다.
- 수정한 내용: `ApplyRawTexture`가 원본 텍스처 종횡비를 계산해 지정된 바운드 안에 맞추도록 변경하고, 생성 잔상이 있던 곡괭이 상단 8px은 UV에서 제외했다. 강화 카드의 레벨 핍·레벨·비용·액션 버튼을 겹치지 않는 독립 행으로 배치했다. 광석 배지를 진행 바 위 렌더 순서로 옮겼다. `UiFontProvider.GetCasual`과 `CasualFeedbackText`를 추가해 Android의 굵은/둥근 시스템 글꼴을 우선하고, 패널 없이 두꺼운 외곽선·그림자·짧은 팝·상승 페이드로 보상과 상태 문구를 표시한다. 설정 모달의 좌표·스킨·동작은 변경하지 않았다.
- 수정 후 상태: HUD 외곽의 승인 좌표는 그대로 유지되며 내부 아이콘 비율과 카드 정보 계층만 정리됐다. 기존 피드백 패널 테마 입력은 호환성을 위해 남아 있지만 항상 투명·비활성 상태로 유지된다.
- 테스트 결과: Unity 스크립트 컴파일 오류 0건, EditMode 48/48 통과. 원본 종횡비 유지, 텍스트 전용 피드백, 강화 카드 행 비겹침을 회귀 테스트로 추가했다.
- 남은 작업: 사용자 요청에 따라 Android/APK 빌드와 실기기 화면 검증은 실행하지 않았다. 사용자가 돌아오면 기기 캡처에서 HUD 비율과 보상 텍스트를 확인한 뒤 Task 12 최종 승인 여부를 결정한다.

## 수정사항 34

- 기록 시각: 2026-07-27 00:22:38
- 작업 요청 요약: 사용자가 직접 맞춘 나머지 HUD 위치·크기는 그대로 유지하고, 비정상 원본 비율의 상단·장비 아이콘을 정사각형 자산으로 교정하며 OreBadge를 삭제하고 진행 트랙·레벨 핍을 새 이미지로 교체한다.
- 수정 전 상태: Unity의 NPOT 자동 스케일로 `IconGoldBadge`가 128×256, `IconGear`·`IconVideo`·`IconDrill`이 256×128 런타임 텍스처가 되어 RectTransform 안에서 눌리거나 좁아 보였다. `OreBadge`는 기능상 불필요했고 진행 트랙 배경과 레벨 핍은 최종 UI 키트에 비해 단순했다. 사용자가 Play Mode에서 직접 조정한 일부 좌표는 코드에 저장되지 않은 상태였다.
- 수정한 내용: 기존 그림 비율은 유지하면서 `IconGoldBadge`, `IconGear`, `IconVideo`, `IconPickaxe`, `IconDrill`을 256×256 투명 캔버스에 재배치했다. 현재 HUD 카드·프레임을 참조해 글자 없는 `HudProgressTrack`과 중립 색상의 `HudLevelPip`을 생성하고 크로마키 제거·크롭·리사이즈 뒤 9-slice로 적용했다. `OreBadge` 생성과 스킨 경로를 삭제했다. Play Mode 종료 전에 HUD RectTransform 142개의 값을 캡처해 크레딧·심도·광고 +·채굴 아이콘·업그레이드 액션처럼 코드와 달랐던 사용자 조정값만 영구 반영했다. 설정창은 변경하지 않았다.
- 수정 후 상태: 다섯 대상 아이콘은 런타임에서 모두 256×256 텍스처로 로드되고, `OreBadge`는 생성되지 않는다. 진행 트랙은 `HudProgressTrack`, 세 강화 카드의 레벨 핍은 `HudLevelPip`을 사용한다. 사용자가 직접 맞춘 나머지 HUD 외곽 좌표와 크기는 유지된다.
- 테스트 결과: Unity Console 컴파일·Play Mode 오류 0건, EditMode 50/50 통과. Play Mode 런타임 검사에서 대상 아이콘 256×256, `OreBadge` 부재, 새 트랙·핍 연결, 저장한 사용자 좌표 일치를 확인했다. 이번 요청에는 Android/APK 빌드와 실기기 검증을 포함하지 않았다.
- 남은 작업: 사용자가 최신 HUD 화면에서 새 아이콘·트랙·핍의 시각 결과를 확인한다. 추가 수정이 없으면 Task 12 HUD 승인 여부를 결정한다.

## 수정사항 35

- 기록 시각: 2026-07-27 00:40:33
- 작업 요청 요약: 새로 적용한 진행 트랙과 레벨 핍에 `Image.Type.Sliced`를 사용하지 않고 `Image.Type.Simple`을 사용한다.
- 수정 전 상태: `HudProgressTrack`과 아홉 개 `HudLevelPip`은 `MineUiSkin.Sliced`로 스프라이트를 생성하고 `ApplySlicedSprite`로 표시했다.
- 수정한 내용: 두 자산을 `MineUiSkin.Simple`과 `ApplySimpleSprite` 경로로 전환했다. 트랙과 핍이 현재 RectTransform 전체를 채우도록 `preserveAspect`는 끄고, 레벨 핍은 Simple 적용 전에 저장한 활성·비활성 틴트 색상을 복원하도록 했다. 기존 패널·버튼의 9-slice, 위치·크기, 설정창과 이미지 원본은 변경하지 않았다.
- 수정 후 상태: 진행 트랙과 모든 레벨 핍의 런타임 `Image.Type`은 `Simple`이며 기존 RectTransform 크기와 레벨 색상 표현을 유지한다.
- 테스트 결과: Unity 스크립트 컴파일 오류 0건, EditMode 50/50 통과. 회귀 테스트에서 트랙·핍의 자산 이름, `Image.Type.Simple`, `preserveAspect == false`를 확인한다.
- 남은 작업: 사용자가 최신 HUD 화면에서 Simple 렌더링 결과를 확인한다. 이번 변경에는 Android/APK 빌드와 실기기 검증을 포함하지 않았다.

## 수정사항 36

- 기록 시각: 2026-07-27 00:53:29
- 작업 요청 요약: 진행 트랙과 레벨 핍뿐 아니라 인게임 HUD의 다른 생성 이미지도 9-slice 왜곡이 없도록 `Image.Type.Simple`로 변경한다.
- 수정 전 상태: 트랙·핍은 Simple로 교정됐지만 헤더, 카운터 슬롯, 설정 버튼 외곽, 광고 보상 pill, 진행 프레임, 채굴 버튼, 크레딧 + 버튼, 강화 카드와 강화 버튼은 최종 스킨에서 `Image.Type.Sliced`를 사용했다.
- 수정한 내용: 최종 인게임 HUD 표면을 모두 `MineUiSkin.Simple`로 로드하고 `ApplyStretchedSimpleSprite`를 통해 `Image.Type.Simple`, `preserveAspect == false`로 적용했다. 레벨 핍 틴트를 포함한 기존 색상은 유지한다. 이전에 승인된 설정 모달과 그 내부 행·버튼·슬라이더의 9-slice는 변경하지 않았다. 위치·크기와 이미지 원본도 수정하지 않았다.
- 수정 후 상태: 인게임 HUD의 헤더·카운터·상단 버튼·진행 UI·채굴 버튼·강화 카드·강화 버튼·레벨 핍은 모두 Simple 렌더링이며 현재 RectTransform 전체를 채운다. 설정 모달은 기존 Sliced 렌더링을 유지한다.
- 테스트 결과: Unity 컴파일 및 Play Mode 오류 0건, EditMode 51/51 통과. Play Mode 런타임 검사에서 대상 HUD 14개 이름 그룹의 모든 인스턴스가 `Simple / preserveAspect false`이고 `SettingsCard`는 `Sliced`임을 확인했다.
- 남은 작업: 사용자가 최신 HUD 화면에서 Simple 렌더링 결과를 확인한다. 이번 변경에는 Android/APK 빌드와 실기기 검증을 포함하지 않았다.

## 수정사항 37

- 기록 시각: 2026-07-27 01:03:51
- 작업 요청 요약: 설정창에 남아 있는 `Image.Type.Sliced` 렌더링을 모두 `Simple`로 바꾸고 설정 관련 아이콘 크기를 조금 줄인다.
- 수정 전 상태: 인게임 HUD 표면은 Simple로 교정됐지만 설정 카드·제목·행·아이콘 웰·슬라이더 트랙·언어 버튼·토글·결제·복원·닫기 버튼은 Sliced를 사용했다. 상단 HUD 설정 톱니바퀴 그림은 112×112였고 설정창 기능 아이콘은 각 컨테이너를 크게 채웠다.
- 수정한 내용: 설정창의 모든 생성 표면과 동적 선택·음소거·토글 상태를 `MineUiSkin.Simple`과 `ApplyStretchedSimpleSprite` 경로로 전환했다. 설정창 기능 아이콘은 `SettingIcon` 60×60, `MuteIcon` 52×52, 국기 114×68, 결제·복원 아이콘 60×60, 닫기 아이콘 64×64로 중앙 고정했다. 상단 HUD 설정 톱니바퀴 그림은 버튼 외곽과 위치를 유지한 채 100×100으로 축소했다. 슬라이더 손잡이와 설정 카드·행·버튼의 RectTransform은 변경하지 않았다.
- 수정 후 상태: 런타임 `SettingsCard` 하위에 `Image.Type.Sliced`가 0개이며 설정 카드 외곽은 기존 900×1344를 유지한다. 상단 HUD 설정 아이콘은 100×100이고 버튼의 위치·클릭 영역은 그대로다.
- 테스트 결과: Unity 컴파일 및 Play Mode 오류 0건, EditMode 52/52 통과. Play Mode 런타임 검사에서 설정창 Sliced 이미지 0개, 설정 카드 900×1344, 상단 설정 아이콘 100×100과 각 설정 기능 아이콘 크기를 확인했다.
- 남은 작업: 사용자가 최신 화면에서 Simple 렌더링과 축소된 설정 아이콘의 시각 결과를 확인한다. 이번 변경에는 Android/APK 빌드와 실기기 검증을 포함하지 않았다.

## 수정사항 38

- 기록 시각: 2026-07-30 18:11:37
- 작업 요청 요약: Unity MCP 연결 상태와 루트 Markdown 기록 누락 여부를 확인하고, Task 13의 첫 콘텐츠 확장으로 챕터·스테이지·보스·최초 클리어 보상 기반을 구현한다.
- 수정 전 상태: Unity MCP는 연결 확인 전이었다. `WORKING_CONTEXT.md`와 `AI_USAGE.md`에는 상세 이력이 있었지만 `TASKS.md`의 현재 작업이 여러 번 중복됐고, `PROJECT_PLAN.md`와 `ARCHITECTURE.md`에는 이미 적용된 UGUI·ScriptableObject·광고·IAP가 예정 또는 초기 프로토타입 상태로 남아 있었다. 게임 규칙은 광석마다 전역 stage만 증가했고 챕터·보스·Gem·최초 클리어 기록이 없었다.
- 수정한 내용: `DrillProject@e76ef9cb0912bea1` Unity 인스턴스, `Assets/PocketForge/Scenes/Mine.unity`, Unity 6000.5.4f1, Android 타깃과 Editor 준비 상태를 MCP로 확인했다. `ChapterDefinition`과 카탈로그 조회를 추가하고 10스테이지 단위 보스, 챕터별 보스 내구도·보상·표시 크기 배율, 최초 클리어 Credits·Gem 보상을 구현했다. 저장 형식을 버전 5로 올려 `gems`와 `highestCompletedChapter`를 영속화했다. 카탈로그에는 Crystal Cavern, Magma Depths, Ancient City 임시 데이터 3종을 등록했다. 역할이 다른 루트 Markdown 9개를 감사하고, 현재 상태 문서인 `TASKS.md`, `PROJECT_PLAN.md`, `ARCHITECTURE.md`를 최신 기준으로 정리했다. 규칙·색인·도구 진입 문서는 변경 사유가 없어 유지했다.
- 수정 후 상태: 일반 stage 진행은 유지되며 각 챕터의 10번째 stage가 보스가 된다. 최초 보스 클리어만 챕터 보너스와 Gem을 지급하고 같은 챕터 재도전에는 보스 기본 보상만 지급한다. HUD 보상 피드백은 Gem을 함께 표시할 수 있다. 현재 세 챕터의 이름과 수치는 구조 검증용 임시 데이터이며 전용 배경·보스 모델·제한 시간·챕터 UI는 아직 없다.
- 테스트 결과: Unity 강제 Refresh 후 컴파일 오류 0건. 전체 EditMode 54/54 통과(실패 0건). 새 테스트로 10번째 stage 보스 판정·내구도, 최초 챕터 보상 1회 지급, 재도전 중복 방지, 저장 버전 5 음수 정규화를 검증했다. 기존 HUD·설정·광고·IAP·현지화 테스트도 모두 통과했다.
- 검증하지 않은 항목: 이번 단계에서는 Play Mode 시각 검수, Android APK/AAB 빌드, 실기기 테스트를 수행하지 않았다.
- 다음 작업: Task 13-1 후반부로 보스 제한 시간과 실패·재도전 규칙을 먼저 추가하고, 현재 HUD를 크게 재배치하지 않는 범위에서 챕터·보스 상태와 완료 보상 패널을 연결한다.

## 수정사항 39

- 기록 시각: 2026-07-30 18:29:43
- 작업 요청 요약: Task 13-1 후반부의 첫 단계로 보스 제한 시간, 실패·즉시 재도전 규칙과 최소한의 챕터·보스 HUD를 구현한다.
- 수정 전 상태: 보스는 내구도·보상·표시 크기 배율만 있었고 제한 시간과 실패 상태가 없었다. HUD에는 전역 stage만 표시돼 챕터 내 진행도와 보스 상태를 알 수 없었다.
- 수정한 내용: `ChapterDefinition`에 `bossTimeLimitSeconds`를 추가하고 임시 챕터 3종을 30초로 설정했다. `OreState`가 보스 남은 시간을 보유하며 `ResetBossAttempt`로 체력과 타이머를 함께 초기화한다. `MiningGameService.Tick`은 자동 채굴 레벨이 0이어도 보스 시간을 감소시키고, 시간 초과 시 보상·stage 증가 없이 같은 보스를 초기화해 즉시 재도전하게 한다. 자동 채굴이 없을 때는 표시되는 정수 초가 바뀌는 순간에만 HUD 갱신 결과를 반환한다. `MiningGameResult.BossFailed`를 통해 Presenter가 네 언어 시간 초과 피드백을 표시한다. 기존 HUD 좌표를 바꾸지 않고 진행 바 위에 600×52 `ChapterStatus`를 추가해 일반 stage에서는 챕터 진행도, 보스에서는 `BOSS mm:ss`를 표시한다.
- 수정 후 상태: 일반 광석은 기존 채굴 흐름과 동일하고 보스에서만 30초 타이머가 동작한다. 실패해도 별도 로딩이나 화면 전환 없이 같은 보스가 최대 체력과 30초로 다시 시작된다. 챕터 완료 패널, 챕터 선택·잠금·재도전 화면과 보스 전용 비주얼은 아직 없다.
- 테스트 결과: Unity 컴파일 오류 0건, 전체 EditMode 63/63 통과. 새 테스트로 보스 타이머 감소, 자동 채굴이 없는 상태의 시간 진행, 정수 초 변경 시에만 HUD 갱신, 시간 초과 시 무보상·동일 stage·체력/타이머 초기화, 일반 광석 타이머 미작동, 보스 HUD 텍스트·위치·크기와 네 언어 피드백을 확인했다. Play Mode 재실행 후 Console 오류 0건이며 런타임 `ChapterStatus`가 `챕터 1 • 07/10`, 600×52, 진행 바 기준 y 54로 생성되는 것을 확인했다. 최초 Console 조회에는 과거 시각이 불명확한 `MineGameController.Update` null 예외 기록이 남아 있었으나 Console 초기화 후 동일 Play Mode 절차를 두 번 수행해 재현되지 않았다.
- 검증하지 않은 항목: 보스 stage의 실제 Play Mode 화면, Android APK/AAB 빌드와 실기기 테스트는 수행하지 않았다.
- 다음 작업: 챕터 최초 클리어 결과를 별도 완료 보상 패널로 표시하고, 확인 후 다음 챕터로 진행하는 흐름을 추가한다.

## 수정사항 39

- 기록 시각: 2026-07-30 18:17:52
- 작업 요청 요약: 루트 Markdown 문서를 근거로 CLAUDE.md에 포트폴리오용 프로젝트 기술 문서(AI 활용, 아키텍처, 구조)를 상세히 기록한다.
- 수정 전 상태: CLAUDE.md는 Claude Code 작업 진입 지침만 담고 있었다. 기술 스택·아키텍처·AI 파이프라인 정보는 ARCHITECTURE.md, PROJECT_PLAN.md, AI_USAGE.md, WORKING_CONTEXT.md에 분산되어 있었다.
- 수정한 내용: 기존 Claude Code 진입 지침은 상단에 그대로 유지하고, 그 아래에 "프로젝트 기술 개요 (포트폴리오)" 절을 추가했다. 프로젝트 소개, 기술 스택 표, 계층 아키텍처와 설계 원칙(순수 C# 서비스, 어댑터·Coordinator 경계, MVP형 UI, 데이터 주도 콘텐츠), 런타임 데이터 흐름, 저장 마이그레이션, 수익화 경계, AI 도구별 역할과 2D/3D 생성 리소스 워크플로, 검증·품질 문화, 현재 진행 상태를 기존 루트 문서의 검증된 사실만 근거로 정리했다.
- 수정 후 상태: CLAUDE.md 하나로 프로젝트의 기술·AI 활용·아키텍처 전체를 소개할 수 있다. 코드·Unity 설정·다른 문서는 변경하지 않았다.
- 테스트 결과: 문서 작업이므로 코드·Unity 테스트는 해당 없음. 기록 내용은 ARCHITECTURE.md, PROJECT_PLAN.md, AI_USAGE.md, TASKS.md의 2026-07-30 기준 내용과 대조했다.
- 남은 작업: 포트폴리오 공개 시점에 진행 상태 절(Task 13 이후)을 최신화한다.

## 수정사항 40

- 기록 시각: 2026-07-30 18:50:51
- 작업 요청 요약: 채굴해도 중앙 광석 내구도 게이지 변화가 화면에 반영되지 않는 문제를 확인하고 수정한다.
- 수정 전 상태: `MiningGameService`와 HUD Render 경로는 채굴 1회에 게이지 비율을 `1.000000 → 0.951613`으로 정상 갱신했지만, `ProgressFill`이 Sprite가 없는 `Image.Type.Filled`여서 `fillAmount`가 실제 메시에 반영되지 않았다.
- 수정한 내용: `ProgressFill`을 프로젝트의 UI 정책에 맞는 `Image.Type.Simple`로 바꾸고 왼쪽 피벗과 기존 시작점 x=4를 유지한 채, 내구도 비율에 따라 600px 기준 RectTransform 폭을 직접 갱신하도록 변경했다. 자식 `ProgressShine`은 stretch anchor를 유지해 Fill 폭과 함께 줄어든다. 다른 HUD 위치·크기·이미지와 설정창은 변경하지 않았다.
- 수정 후 상태: 런타임 채굴 1회 후 Fill 폭이 `600 → 570.968`로 감소하고 광택 폭도 `600 → 570.968`로 함께 감소한다. 왼쪽 시작점과 전체 HUD 배치는 기존과 동일하다.
- 테스트 결과: Unity 스크립트 컴파일 오류 0건, 전체 EditMode 63/63 통과. Play Mode에서 `Simple`, pivot x=0, 시작점 x=4와 채굴 전후 Fill·광택 폭을 직접 확인했다.
- 검증하지 않은 항목: Android APK/AAB 빌드와 실기기 화면 검증은 이번 수정에 포함하지 않았다.
- 다음 작업: Task 13-1의 챕터 최초 클리어 보상 패널 구현을 계속한다.

## 수정사항 41

- 기록 시각: 2026-07-30 19:12:32
- 작업 요청 요약: 좌상단의 용도를 알 수 없는 UI를 정리하고, 얇고 프로토타입처럼 보이는 중앙 광석 체력바를 새 AI 생성 자산으로 교체한다.
- 수정 전 상태: 좌상단 `HeaderCoin`과 `HeaderCounterSlot`은 실제 데이터가 연결되지 않은 장식이었고, `CreditsRewardButton`은 우측 보상형 광고 pill과 같은 콜백을 호출하는 중복 진입점이었다. 중앙 체력바는 형태가 거의 같은 `HudProgressFrame`과 `HudProgressTrack`이 겹치고 단색 Fill을 사용해 얇고 평평하게 보였다.
- AI 생성 과정: 현재 전체 HUD 캡처와 기존 진행 UI를 참고해 좌상단 불필요 묶음을 제거하고 새 체력바를 배치한 완성 스냅샷을 먼저 생성했다. 승인 방향을 기준으로 외곽 프레임, 남색 트랙, 청록색 Fill을 각각 단색 크로마 배경의 독립 래스터 자산으로 생성했다. 기본 Python 크로마 도구를 실행할 런타임이 없어 Windows 이미지 처리로 알파 제거·despill·여백 크롭만 수행했으며 크로마 원본과 적용 스냅샷을 함께 보관했다.
- 수정한 내용: `HeaderCoin`, `HeaderCounterSlot`, `CreditsRewardButton` 생성·바인딩·스킨 연결을 제거했다. 새 `HudOreHealthFrame`, `HudOreHealthTrack`, `HudOreHealthFill`을 `MineUiSkin`의 Simple 경로로 적용하고 프레임 638×78, 트랙 604×50, Fill 572×36으로 구성했다. Fill은 기존 왼쪽 기준 폭 감소 방식을 유지하며 생성 이미지 자체의 광택을 사용하므로 별도 `ProgressShine`을 제거했다.
- 수정 후 상태: 상단에는 실제 크레딧·심도 표시, 설정과 우측 보상형 광고만 남는다. 중앙 체력바는 은색·보라 외곽, 남색 빈 영역, 청록색 광택 Fill의 세 레이어로 표시되며 챕터 상태 문구는 체력바 위에 유지된다.
- 테스트 결과: Unity 컴파일 오류 0건, 전체 EditMode 64/64 통과. Play Mode에서 제거 대상 세 오브젝트 부재, 새 텍스처 이름·Simple 타입·RectTransform 치수를 확인했다. 채굴 1회 후 Fill 폭이 `572 → 544.323`으로 줄고 왼쪽 시작점 x=16을 유지했다. Device Simulator 실제 합성 화면을 캡처해 좌상단 중복 UI가 사라지고 새 체력바가 동굴 배경·채굴 버튼과 겹치지 않는 것을 확인했다. 최종 Console 오류 0건이다.
- 검증하지 않은 항목: Android APK/AAB 빌드와 실기기 화면 검증은 수행하지 않았다.
- 다음 작업: 사용자가 새 체력바 시각 결과를 확인한 뒤 Task 13-1 챕터 완료 보상 패널 구현을 계속한다.

## 수정사항 42

- 기록 시각: 2026-07-30 19:34:44
- 작업 요청 요약: Task 13-1의 다음 순서로 챕터 최초 클리어 보상을 별도 모달에 표시하고, 확인 후 이미 진행된 다음 stage로 복귀하는 흐름을 구현한다.
- 수정 전 상태: 최초 챕터 보상은 게임 규칙과 저장 데이터에는 존재했지만 일반 채굴 보상 피드백으로만 표시됐다. 완료 챕터 번호를 Presenter가 알 수 없었고, 챕터 클리어를 강조하거나 사용자의 확인을 기다리는 전용 UI가 없었다.
- 수정한 내용: `MiningGameResult`에 `CompletedChapterNumber`를 추가해 최초 클리어 결과가 완료된 챕터를 명시하도록 했다. Presenter는 최초 클리어일 때 일반 보상 피드백 대신 `MineHudView.ShowChapterComplete`를 호출한다. 기존 설정창·버튼·재화 아이콘 자산을 재사용해 전체 화면 입력 차단 배경, 챕터 제목, 최초 클리어 보상 문구, Credits·Gem 행, `계속` 버튼을 가진 전용 모달을 구성했다. `계속`은 보상이나 진행 상태를 다시 변경하지 않고 모달만 닫는다. 제목·보상 안내·버튼은 한국어·영어·일본어·중국어 현지화 키를 추가했다.
- 수정 후 상태: 챕터 보스를 최초로 클리어하면 보상 지급과 다음 stage 전환이 먼저 완료된 뒤 모달이 최상위 형제로 표시되어 하위 HUD 입력을 차단한다. 모달에는 완료 챕터 번호와 실제 지급된 Credits·Gem이 표시되고 `계속`을 누르면 다음 stage 상태를 유지한 채 닫힌다. 같은 챕터 재클리어에는 최초 클리어 모달이 나타나지 않는다.
- 크래시와 복구: 최초 전체 EditMode 실행 중 Unity Editor가 테스트 본문 진입 전에 종료됐다. `Editor.log`의 네이티브 스택은 Unity Test Framework가 부트스트랩 씬을 만드는 `EditorSceneManager.NewScene` 과정의 `PhysicsScene2D::DestroyWorld`에서 중단된 것으로 기록됐으며 관리 C# 예외나 새 테스트 실패는 없었다. Unity 6000.5.4f1을 같은 프로젝트로 다시 실행해 MCP를 재연결한 뒤 대상 테스트와 전체 테스트를 재실행했다.
- 테스트 결과: 재시작 후 챕터 결과·모달 입력 차단/계속·4개 언어 대상 테스트 3/3 통과, 전체 EditMode 66/66 통과, Unity Console 오류 0건. Play Mode에서 `챕터 1 클리어`, `+200`, `+5`, 1080×2316 입력 차단 배경, 780×720 카드, 최상위 형제 배치를 확인했고 `계속` 호출 후 모달만 닫히는 것을 확인했다. Device Simulator 실제 합성 화면도 캡처했다.
- 검증하지 않은 항목: 새 Android APK/AAB 빌드와 실기기 검증은 수행하지 않았다.
- 다음 작업: Task 13-1의 챕터 선택·재도전 진입점을 설계하고 구현한다.

## 수정사항 43

- 기록 시각: 2026-07-30 20:06:26
- 작업 요청 요약: Task 13-1의 마지막 핵심 흐름으로 현재 챕터 상태 표시를 눌러 챕터를 선택하고, 완료 챕터를 재도전한 뒤 기존 최장 진행으로 돌아올 수 있게 한다.
- 수정 전 상태: 챕터 진행·보스·최초 클리어 보상은 동작했지만 완료한 챕터로 돌아갈 진입점이 없었다. 저장 데이터에는 현재 stage만 있어 이전 챕터 재도전 시 원래 진행 위치를 복원할 근거도 없었다.
- 수정한 내용: 저장 형식을 버전 6으로 올리고 `furthestStage`를 추가해 현재 stage와 독립적으로 최장 진행을 보존했다. `MiningContentCatalog`의 챕터 목록과 범위를 기준으로 완료·현재·잠김 상태 및 이동 목표를 계산하는 `ChapterSelectionOption`과 `SelectChapter` 규칙을 `MiningGameService`에 추가했다. 기존 `ChapterStatus`에 버튼 역할을 연결하고, 현재 UI 스킨의 Simple 이미지만 재사용하는 입력 차단형 챕터 선택 모달을 구현했다. 목록은 챕터 수에 고정되지 않으며 완료 챕터는 `재도전`, 진행 중 챕터는 `현재` 또는 `이어하기`, 미도달 챕터는 `잠김`으로 표시한다. 제목·상태·스테이지 범위·행동 문구는 한국어·영어·일본어·중국어를 지원한다.
- 수정 후 상태: 실제 저장 진행이 stage 19일 때 챕터 1은 완료·재도전, 챕터 2는 현재, 챕터 3은 잠김으로 표시된다. 챕터 1 재도전으로 stage 1에 이동해도 `furthestStage` 19가 유지되고 챕터 2의 `이어하기`로 stage 19에 복귀한다. 현재 챕터와 잠긴 챕터 선택은 상태를 변경하지 않는다.
- 초기화 방어: 첫 Play 전환에서 Presenter 초기화 전 `MineGameController.Update`가 호출되어 null 예외가 반복되는 사례가 있어 Update에 의존 객체 준비 여부 가드를 추가했다. 재컴파일 후 깨끗한 Play Mode를 반복해 같은 예외가 재현되지 않고 Console 오류 0건임을 확인했다.
- 테스트 결과: 챕터 상태·재도전·잠금 규칙과 네 언어 모달 대상 EditMode 6/6, 전체 EditMode 71/71을 통과했다. Play Mode에서 `stage 19 → 1 → 19`, `furthestStage 19` 보존, `이어하기` 표시, 모달 최상위 입력 차단과 닫기를 실제 버튼 경로로 확인했다. 최종 합성 화면은 `Assets/PocketForge/Art/Generated/UI/References/PocketForge_ChapterSelection_RuntimeUI.png`에 기록했으며 Unity Console 오류는 0건이다.
- 검증하지 않은 항목: 새 Android APK/AAB 빌드와 실기기 검증은 승인 범위에 포함하지 않아 수행하지 않았다. 챕터 전용 배경·보스 모델·연출과 Gem 상시 HUD도 이번 기능 범위에서 제외했다.
- 다음 작업: Task 13-2로 이동해 광부/계정 레벨, 경험치 획득원, 레벨 기반 기능 해금 구조를 먼저 설계한다.

## 수정사항 44

- 기록 시각: 2026-07-30 21:00:20
- 작업 요청 요약: 기존 클릭커 중심 기획을 자동 채굴과 오프라인 생산이 중심이 되는 하이브리드 방치형으로 변경하고, 후반 챕터는 클릭만이 아니라 여러 콘텐츠에서 합산된 전체 스펙이 충분해야 보스를 처치할 수 있게 다시 기획한다.
- 수정 전 상태: 탭 피해는 곡괭이 레벨, 자동 피해는 드릴 레벨, 보상 배율은 로봇 레벨에서 독립적으로 계산됐다. 오프라인 보상은 드릴 레벨만 사용했고, 보스 실패 시 같은 보스가 즉시 초기화돼 성장 없이 반복 도전하는 흐름이었다. `PROJECT_PLAN.md`의 다음 메타 단계는 광부 레벨부터 바로 추가하도록 정리돼 있었다.
- 수정한 내용: 제품 장르를 하이브리드 방치형 채굴 성장 게임으로 명시하고 자동 생산→투자→보스 관문→기능 해금의 핵심 루프를 정의했다. 시설·광부 등급·장비·연구·광물 도감·일시 버프를 합산하는 파생 능력치 `총 채굴력`, 보스 내구도와 제한 시간에서 계산하는 `권장 채굴력`, 전체 성장 배율을 공유하는 탭 보조 원칙을 기획했다. 보스 실패 후 직전 일반 스테이지 자동 파밍, 준비 후 명시적 재도전, 오프라인 보스 자동 클리어 금지 정책을 추가했다. Task 13을 13-2A 방치형 핵심 공식, 13-2B 오프라인 진행, 13-3 광부 등급·연구, 13-4 장비, 13-5 수집·미션, 13-6 상점·수익화·이벤트 순서로 재편했다.
- 수정 후 상태: 다음 구현은 계정 레벨 UI부터 추가하는 것이 아니라 총 채굴력 계산 경계, 자동 채굴 중심 피해, 탭 보조, 보스 권장치와 실패 후 파밍을 먼저 완성하는 Task 13-2A다. 신규 성장 시스템은 같은 계산 경계에 기여 소스로 추가할 수 있으며 총 채굴력 자체는 저장하지 않는다.
- 참고 근거: Idle Miner Tycoon 공식 소개의 자동화·오프라인 생산·연구·수집 계층과 Tap Titans 2 공식 스토어 설명의 탭·영웅·장비·영구 성장 결합을 구조 참고로 사용했다. 구체 콘텐츠·수치·UI는 복제하지 않고 현재 Pocket Forge 규모에 맞게 축소했다.
- 테스트 결과: 기획 문서만 변경했으므로 Unity 컴파일·EditMode·Play Mode·Android 테스트는 실행하지 않았다. `PROJECT_PLAN.md`, `TASKS.md`, `WORKING_CONTEXT.md`, `AI_USAGE.md`의 용어·단계·미구현 표시를 상호 대조했다.
- 남은 작업: Task 13-2A 구현 전에 현재 채굴 공식과 보스 실패 경로의 구체 변경 파일, 초기 수치, 회귀 테스트 범위를 작업 전 확인 형식으로 승인받는다. 오프라인 계산 개편과 신규 성장 콘텐츠는 각 후속 단계에서 별도로 승인받는다.

## 수정사항 45

- 기록 시각: 2026-07-30 21:37:14
- 작업 요청 요약: 승인된 Task 13-2A 범위에 따라 자동 채굴을 기본 진행 동력으로 만들고, 탭 보조·총 채굴력·보스 권장치·실패 후 파밍과 명시적 재도전을 구현한다.
- 수정 전 상태: 자동 피해는 드릴 레벨이 1 이상일 때만 발생했고 탭 피해·자동 피해·로봇 보상 배율이 서로 다른 계산 경로에 있었다. 보스 시간 초과 시 같은 보스가 즉시 최대 체력으로 초기화돼 성장 판단 없이 무한 재도전됐으며 HUD에는 현재 전력과 권장치가 없었다.
- 수정한 내용: `MiningPowerService`와 `MiningPowerSnapshot`을 추가해 드릴 출력, 로봇 배율, 자동 채굴력, 탭 피해, 능동 채굴력과 보스 권장 채굴력을 한 순수 C# 경계에서 계산하도록 했다. 기본 자동 채굴력은 0.5/s이며 드릴은 레벨당 +0.5, 로봇은 레벨당 전체 출력 +10%, 탭은 `1 + √(곡괭이 레벨)` 기반 피해와 자동 채굴력 0.05~0.10초분 중 큰 값을 사용한다. 탭 입력은 0.2초 쿨다운으로 초당 최대 5회만 유효하다. 미래 광부 등급·장비·연구·도감·버프는 `MiningPowerModifiers` 배율로 같은 공식에 합성할 수 있지만 실제 메타 시스템과 저장 필드는 추가하지 않았다.
- 보스 관문: 보스 권장 채굴력은 `내구도 ÷ 제한 시간`으로 계산한다. 현재 챕터 권장치는 약 5.5/s, 14.58/s, 26/s이며 기본 능동 채굴력 5.5/s는 챕터 1만 간신히 도달한다. 제한 시간의 마지막 자동 피해로 체력이 0이 되면 클리어하고, 실패하면 최장 진행의 보스 도달 기록은 유지한 채 직전 일반 stage로 이동한다. 일반 광석을 반복 파괴해도 미클리어 보스로 자동 진입하지 않으며 챕터 상태 버튼의 `도전` 경로로만 재진입한다.
- UI·현지화: 기존 HUD 배치는 유지했다. 일반 stage의 `ChapterStatus`에는 자동 채굴력 `/s`, 보스에는 남은 시간과 현재/권장 채굴력, 실패 후 파밍 상태에는 네 언어 `보스 준비`와 현재/권장치를 표시한다. 시간 초과 피드백은 네 언어 모두 직전 stage 자동 파밍을 안내하도록 변경했다.
- 수정 후 상태: 탭을 하지 않아도 모든 일반 stage가 기본 0.5/s로 진행되며 드릴·로봇 투자에 따라 자동 진행 속도가 증가한다. 탭은 입력 상한이 있는 능동 가속 수단이고, 보스 실패는 일반 stage 자동 파밍과 전력 비교를 거친 뒤 사용자가 재도전하는 관문으로 동작한다.
- 테스트 결과: 변경 대상 테스트 17/17 통과. 첫 전체 EditMode 실행은 기존 자동 피해 없음 가정을 가진 테스트 1개가 실패해 새 기본 자동 채굴 규칙에 맞게 교정했으며, 최종 전체 EditMode 83/83 통과했다. Unity 컴파일 오류와 Console 오류는 0건이다. Editor Play Mode에서 실제 저장 상태 stage 25의 광석 체력이 자동으로 감소하고 HUD에 `5.6/s`가 표시되는 것을 확인했다. 저장 데이터를 변경하지 않는 런타임 시나리오에서 보스 실패, 직전 stage 파밍 상태, target stage 10의 명시적 도전, 보스 재진입과 기본 `auto=0.5`, `active=5.5`를 확인했다.
- 검증하지 않은 항목: 승인 범위에 따라 오프라인 보상 공식은 기존 드릴 전용 계산을 유지했고, Android APK/AAB 빌드와 실기기 검증은 수행하지 않았다. 광부 등급·장비·연구·도감·버프는 계산 확장 지점만 있으며 실제 콘텐츠는 아직 없다.
- 남은 작업: Task 13-2B로 이동해 오프라인 보상이 `MiningPowerService.AutoPowerPerSecond`를 사용하도록 설계하고, 보스 자동 클리어 금지·직전 일반 stage 파밍·중복 지급 방지 정책을 구현 전에 승인받는다.

## 수정사항 46

- 기록 시각: 2026-07-30 21:59:19
- 작업 요청 요약: Task 13-2B로 기존 드릴 전용 오프라인 보상을 전체 자동 채굴력 기준으로 개편하고, 일반 stage 파밍·보스 자동 클리어 금지·중복 수령 및 시간 조작 방어·상세 복귀 UI를 구현한다.
- 수정 전 상태: `ApplyOfflineReward`는 드릴 레벨 피해만 현재 stage의 Credits/내구도 비율로 환산했다. 처리 광석 수와 적용 시간이 없었고 보스 stage도 현재 보상 기준을 사용했다. 저장 시각이 없는 신규 유저는 드릴 0 조건으로 우연히 보상을 피했으므로 기본 자동 채굴 0.5/s 도입 후에는 최초 실행에 4시간치 보상이 지급될 위험이 있었다. 복귀 UI는 Credits만 표시했으며 백그라운드에서 같은 프로세스로 돌아올 때는 보상을 계산하지 않았다.
- 수정한 내용: `OfflineProgressResult`와 `MiningGameService.ClaimOfflineProgress`를 추가해 실제 경과 시간, 최대 4시간의 적용 시간, 파밍 stage, 정수 처리 광석 수와 Credits를 반환하도록 했다. 피해 기준은 `MiningPowerService.AutoPowerPerSecond`이며 보상은 일반 광석의 내구도와 일반 보상만 사용한다. 파밍 stage는 `furthestStage - 1`부터 보스 stage를 건너뛴 가장 가까운 일반 stage이고, 신규 유저만 stage 1을 사용한다. 이전 챕터 재도전 중에도 최장 진행 기준으로 계산하며 stage·furthestStage·챕터 완료·현재 OreState는 변경하지 않는다.
- 저장·생명주기: 저장 시각이 없으면 보상 없이 현재 UTC 체크포인트만 초기화한다. 현재 시각이 기존 체크포인트와 같거나 과거면 청구하지 않고, 정상 청구 후에는 체크포인트를 즉시 전진시켜 저장한다. `SaveService.Save`는 저장 시각을 단조 증가시켜 기기 시간 역행으로 체크포인트가 낮아지지 않게 했다. `MineGameController`는 시작과 실제 `OnApplicationPause(false)` 복귀에서 같은 경로를 사용하며 복귀 보상 적용 직후 HUD와 저장을 갱신한다.
- UI·현지화: 기존 `OfflineRewardSurface` 410×82와 텍스트 380×58의 위치·크기를 유지했다. 적용 시간, 처리 광석 수, Credits를 두 줄로 표시하고 시간·분·초와 요약 문구를 한국어·영어·일본어·중국어로 추가했다. 새 이미지와 레이아웃 변경은 없다.
- 수정 후 상태: 온라인과 오프라인 생산은 같은 자동 채굴력에서 파생되고 오프라인에서는 정수 개수의 마지막 클리어 일반 광석만 처리한다. 보스·챕터·진행 단계는 건너뛰지 않으며 신규 저장·중복 청구·시간 역행 경계는 보상을 만들지 않는다. 4시간을 넘은 경과 시간은 4시간까지만 보상하고 초과분은 이월하지 않는다.
- 테스트 결과: 최초 대상 테스트는 stage 1 기본 광석 보상을 1 Credits로 잘못 가정한 기대값 2건만 실패했다. 실제 카탈로그의 2 Credits에 맞춰 기대값을 교정한 뒤 오프라인 계산·보스 경계·신규 저장·중복/시간 역행·네 언어·HUD 대상 EditMode 11/11, 전체 EditMode 93/93을 통과했다. Unity 컴파일과 최종 Console 오류는 0건이다. Play Mode 임시 저장에서 보스 stage 10과 furthestStage 10을 유지한 채 stage 9 기준 1시간 결과 `광석 31 / 837 C`를 확인했다. 같은 세션에서 2분 백그라운드 복귀를 모사해 `광석 1 / 27 C` 추가와 HUD 갱신을 확인했다. 검증 전 기존 PlayerPrefs 원문을 세션 백업하고 종료 후 바이트 문자열 비교가 일치하도록 복원했다.
- 검증하지 않은 항목: Android APK/AAB 빌드와 실기기 백그라운드 복귀는 승인 범위에서 제외했다. 경험치·신규 재료·광고 배수·오프라인 상한 확장 상품은 아직 구현하지 않았다.
- 남은 작업: Task 13-3으로 이동해 경험치·계정 레벨·기능 해금·소규모 연구를 한 번에 과도하게 확장하지 않도록 구현 단위를 나누고, 저장 스키마와 첫 경험치/해금 수치를 작업 전 승인받는다.

## 수정사항 47

- 기록 시각: 2026-07-30 22:25:29
- 작업 요청 요약: Task 13-3을 과도하게 한 번에 묶지 않고 13-3A로 분리해 접속 중·오프라인 경험치, 광부 등급, 기능 해금, 레벨업 보상과 총 채굴력 반영 기반을 구현한다. 연구는 다음 13-3B로 남긴다.
- 수정 전 상태: 저장 버전 6에는 경험치와 광부 레벨이 없었고 `MiningPowerModifiers.MinerRankMultiplier`는 항상 1이었다. 오프라인 진행은 Credits만 지급했으며 레벨·기능 해금·다음 해금 조회가 없었다. HUD의 기존 자원 카운터 왼쪽은 비어 있었다.
- 수정한 내용: 저장을 버전 7로 올리고 `minerLevel`, `minerExperience`, `highestRewardedMinerLevel`을 추가했다. 독립 `MinerProgressionService`가 XP 곡선, 일반·보스 XP, 다중 레벨업, 자동 Credits·Gem 보상, 마지막 보상 레벨 중복 방지, 기능 해금·다음 해금, 레벨당 +2% 등급 배율을 계산한다. `MiningGameService`는 접속 중 광석 파괴와 오프라인 정수 광석 처리에서 같은 경험치 경계를 호출하고 등급 배율을 `MiningPowerModifiers`로 전달한다. `MiningContentCatalog`에는 조절 가능한 경험치·보상·해금 데이터를 추가했다.
- UI·다국어: 사용자가 확정한 기존 Credits·Depth·Settings 좌표를 이동하지 않았다. 헤더의 비어 있던 왼쪽 영역에 260×94 광부 레벨·XP 표시를 추가하고, 누르면 총 채굴력 보너스와 다음 해금을 안내한다. 오프라인 요약은 기존 410×82 표면과 380×58 텍스트 치수에서 Credits와 XP를 함께 표시한다. 레벨업·해금·장비·박물관·연구·미션·상점·이벤트 문구를 한국어·영어·일본어·중국어로 추가했다.
- 초기 데이터: 일반 광석은 챕터 번호만큼 XP, 보스는 챕터 번호×10 XP를 준다. 다음 레벨 요구량은 `20 + 15 × (현재 레벨 - 1)`, 레벨업 Credits는 `새 레벨 × 25`, 5레벨마다 Gem 1개다. Lv.2 장비, Lv.3 박물관, Lv.4 연구, Lv.5 미션, Lv.6 상점, Lv.7 이벤트가 해금된다. 실제 하위 콘텐츠 화면은 아직 Placeholder이며 수치는 Task 14 밸런싱 전 초기값이다.
- 수정 후 상태: 광석 파괴와 오프라인 생산이 계정 경험치로 이어지고 광부 레벨 상승이 자동·탭·오프라인 채굴력과 보스 비교에 동일하게 반영된다. 기능 해금은 별도 불리언을 저장하지 않고 레벨과 데이터 정의에서 파생되며 레벨 보상은 마지막 지급 레벨을 저장해 중복 지급을 방지한다.
- 테스트 결과: 최초 영향 범위 EditMode 89개 중 오프라인 비교 시간이 짧아 정수 내림 결과가 같은 테스트 1개만 실패했고 비교 구간을 늘려 교정했다. 해당 테스트 1/1 재통과 후 전체 EditMode 105/105 통과, Unity 컴파일 오류와 최종 Console 오류 0건을 확인했다. 스크립트 재컴파일 중 Google Android Resolver가 `GvhProjectSettings.xml`에 동시에 접근해 공유 위반을 한 번 기록했지만 3초 뒤 파일 잠금 해제와 재실행 전체 테스트 통과를 확인했고 같은 오류는 남지 않았다. Play Mode 진입·종료 중 런타임 오류는 발생하지 않았다.
- 검증하지 않은 항목: Android APK/AAB 빌드와 실기기 화면·백그라운드 복귀는 승인 범위에서 제외했다. Game View 카메라 캡처는 Screen Space Overlay HUD를 포함하지 않아 UI 시각 검증 근거로 사용하지 않았고, HUD는 좌표·타입·문자열 EditMode 회귀 테스트로 검증했다.
- 다음 작업: Task 13-3B로 설계도 코어 지급 정책, 3~5개 연구 노드, 비용·최대 단계·선행 조건, Lv.4 연구 해금과 `ResearchMultiplier` 연결 범위를 작업 전 승인받는다.

## 수정사항 48

- 기록 시각: 2026-07-30 23:26:39
- 작업 요청 요약: Task 13-3B의 설계도 코어·소규모 영구 연구를 구현하면서, 증가하는 크레딧과 모든 플레이어 표시용 성장 수치에 공통 K/M/B/T 단위 축약 정책을 적용한다.
- 수정 전 상태: 저장 버전 7의 `credits`와 `gems`는 32비트 정수라 약 21억에서 포화됐고 HUD·보상·비용·XP·채굴력은 각각 `N0`, `N1` 형식을 직접 사용했다. 연구는 Lv.4 해금 정의와 `MiningPowerModifiers.ResearchMultiplier` 자리만 있었으며 보스 처치 재화, 연구 저장·구매 규칙·화면은 없었다.
- 64비트·표시 정책: 저장을 버전 8로 올리고 `credits`, `gems`, `blueprintCores`를 64비트 정수로 전환했다. 보상 결과·강화 비용·광석 보상·광고·오프라인 계산도 같은 타입과 포화 연산을 사용한다. `CompactNumberFormatter`는 증가형 수치를 K/M/B/T/Qa/Qi로 표시하고 3자리 수준의 유효 정밀도를 유지한다. Credits·Gem·코어·보상·비용·XP·채굴력은 이 경계를 사용하며 레벨·챕터·stage·시간·진행 개수는 정확한 값을 유지한다.
- 연구 규칙: `ResearchNodeDefinition`과 `ResearchService`를 추가했다. 1~3챕터 보스 최초 처치는 3/6/9, 반복 처치는 1/2/3 설계도 코어를 지급한다. `core_output`은 최대 5레벨·레벨당 +5%, `precision_tools`는 선행 Lv.2·최대 4레벨·레벨당 +7%, `deep_automation`은 선행 Lv.2·최대 3레벨·레벨당 +10%다. 비용과 선행 조건은 `MiningContentCatalog.asset`에 직렬화했고 구매 시 해금·선행·최대·잔액을 방어한다.
- 저장·전력: 연구 진행은 `researchProgress[]`의 노드 ID와 레벨만 저장해 노드 추가에 저장 스키마 변경이 필요 없게 했다. 마이그레이션은 음수 코어와 비정상 항목을 제거하고 중복 노드 ID는 가장 높은 레벨 하나로 정규화한다. 연구 보너스 합계는 `ResearchMultiplier`로 들어가 자동·탭·오프라인·보스 권장치 비교가 같은 파생 능력치 경계를 공유한다.
- UI·다국어: 사용자가 직접 맞춘 기존 HUD 위치·크기는 변경하지 않았다. 광부 Lv.4부터 기존 광부 등급 버튼이 3개 연구 노드, 코어 잔액, 현재 연구 배율, 비용·잠금·최대 상태를 표시하는 입력 차단 모달을 연다. 최초 챕터 완료와 반복 보상 피드백에 코어를 표시하고 한국어·영어·일본어·중국어 문구를 추가했다.
- 테스트 결과: 숫자 축약 경계, 21억 초과 Credits, 저장 정규화, 연구 잠금·비용·선행·최대·중복 방어, 보스 최초·반복 코어, 연구의 자동·탭·오프라인 공통 배율, 4개 언어, 실제 연구 모달 열기와 첫 노드 구매를 포함해 전체 EditMode 121/121을 통과했다. Unity 강제 전체 Refresh 뒤 컴파일 오류와 최종 Console 오류는 0건이다.
- 검증하지 않은 항목: 승인 범위에 따라 Android APK/AAB 빌드와 실기기 화면·저장 마이그레이션은 수행하지 않았다. Task 14 전 초기 수치이므로 연구 비용·코어 보상·배율은 밸런싱 대상이다.
- 다음 작업: Task 13-4로 이동해 곡괭이·드릴·로봇·부적 4슬롯 장비의 등급, 획득, 비교, 장착·합성, 저장과 `EquipmentMultiplier` 연결 범위를 작업 전 승인받는다.

## 수정사항 49

- 기록 시각: 2026-07-30 23:33:27
- 작업 요청 요약: 지금까지 확정된 Pocket Forge 기획, 구현 완료 범위, 앞으로 해야 할 콘텐츠·밸런싱·폴리싱·출시 작업을 하나의 TXT 문서로 통합한다.
- 수정 전 상태: 현재 작업은 `TASKS.md`, 장기 계획은 `PROJECT_PLAN.md`, 구현 구조는 `ARCHITECTURE.md`, 상세 이력은 `WORKING_CONTEXT.md`에 나뉘어 있어 전체 현황과 남은 순서를 한 번에 읽는 문서가 없었다.
- 수정한 내용: 루트에 `POCKET_FORGE_MASTER_PLAN.txt`를 생성했다. 제품 방향, 핵심 루프, 채굴·챕터·재화·광부 등급·연구·오프라인·광고·IAP·UI·저장·최적화의 현재 상태를 완료·부분 완료·예정·Placeholder로 구분했다. 다음 Task 13-4 장비, 13-5 수집·미션, 13-6 상점·이벤트, Task 14 밸런싱, 최종 폴리싱·QA·Google Play 출시 순서와 미결정 사항·완료 기준을 함께 정리했다.
- 수정 후 상태: 프로젝트 전체 방향과 다음 작업 순서를 비개발자도 TXT 하나에서 확인할 수 있다. 기준 Markdown은 변경하지 않았으며 통합 TXT가 해당 문서들을 대체하지 않는다.
- 테스트 결과: 문서 작업이므로 Unity 컴파일·EditMode·Play Mode·Android 테스트는 실행하지 않았다. `TASKS.md`, `PROJECT_PLAN.md`, `ARCHITECTURE.md`, `WORKING_CONTEXT.md`, `AI_USAGE.md`의 2026-07-30 기준 사실과 TXT의 상태 표기를 대조했다.
- 남은 작업: 통합 TXT는 각 Task 완료 시 최신 상태를 반영해야 한다. 현재 다음 구현은 Task 13-4 장비 시스템 Lite이며 구체 등급·드롭·합성 규칙은 작업 전 사용자와 조율한다.

## 수정사항 50

- 기록 시각: 2026-07-31 00:04:00
- 작업 요청 요약: 승인된 보랏빛 캐주얼 메인 화면 가스샷을 구성 요소별 이미지로 제작할 수 있도록, 텍스트 없는 UI 자산 생성 원칙과 모든 요소별 프롬프트를 하나의 TXT 파일로 통합한다.
- 수정 전 상태: 가스샷과 요소별 생성 프롬프트는 대화에만 존재했고 프로젝트에서 한 번에 복사하거나 반복 참조할 수 있는 문서가 없었다.
- 수정한 내용: 프로젝트 루트에 `POCKET_FORGE_UI_ASSET_PROMPTS.txt`를 생성했다. 공통 스타일·크로마키·무문자 규칙, 38개 요소별 `Primary request`, 이미지에 포함하지 않을 런타임 정보, 권장 생성 순서, 검수 기준과 Unity 적용 메모를 수록했다.
- 수정 후 상태: UI 요소를 한 개씩 생성할 때 공통 프롬프트와 개별 요청을 한 파일에서 복사할 수 있다. 레벨·가격·재화량·진행도·다국어 문구는 생성 이미지에 굽지 않고 Unity 런타임 UI로 표시한다는 기준이 명시됐다.
- 테스트 결과: UTF-8 TXT 파일 생성과 필수 섹션·금지 텍스트 원칙의 포함 여부만 확인했다. 이미지 생성, 크로마키 제거, Unity 임포트, 컴파일, EditMode, Play Mode와 Android 테스트는 수행하지 않았다.
- 남은 작업: 실제 자산 생성 시 한 요청당 한 요소 원칙을 지키고, 각 결과의 글자·가짜 문자·크로마 잔상·9-slice 적합성을 개별 검수해야 한다. Task 13-4 상태와 게임 구현은 변경하지 않았다.

## 수정사항 51

- 기록 시각: 2026-07-31 01:47:38
- 작업 요청 요약: `POCKET_FORGE_UI_ASSET_PROMPTS.txt`를 그대로 따라 승인된 메인 화면 가이드의 배경을 제외한 UI 요소 38개를 각각 제작하고 실제 HUD에 적용한다.
- 수정 전 상태: V4 HUD는 기능은 연결돼 있었지만 새 852×1846 가이드샷의 상단 통합 자원 HUD, 챕터·전투력·보스 정보 위계, 방치·연구 바로가기, 보스 도전, 하단 내비게이션을 재현하지 않았다. 프롬프트 문서는 있었지만 실제 생성·투명화·런타임 연결은 수행되지 않았다.
- 생성 자산: 공통 스타일 프롬프트와 요소별 요청을 사용해 한 요청당 한 요소씩 38개를 생성했다. `V5Guide/SourceChroma`에 크로마 원본, `V5Guide/Sprites`에 투명본, `Resources/PocketForge/UI/V5`에 런타임 사본을 보관하고 원본 가이드·접촉 시트·자산 명세를 함께 남겼다. Unity 픽셀 후처리로 크로마 제거·despill·알파 크롭을 수행했으며 38개 모두 네 모서리 알파 0과 잔류 녹색 픽셀 0을 확인했다.
- 런타임 적용: `MineUiSkin`에 V5 전용 캐시를 추가하고 `MineHudViewV5` 부분 클래스로 새 HUD 조립을 분리했다. Credits·Gem·설계도 코어·광부 레벨/XP, 챕터·스테이지·광산명, 현재/권장 채굴력, 보스 거리/타이머, 광석 체력, 방치 보상, 연구, 채굴, 보스 도전, 곡괭이·드릴·로봇 강화와 하단 메뉴를 기존 상태·서비스에 연결했다. 광고 보상은 Credits의 추가 버튼, 연구는 기존 연구 모달, 보스는 기존 챕터 재도전 경로를 사용한다. 미구현 장비·박물관·미션·상점은 해금 상태와 준비 중 안내만 표시한다.
- 그래픽·레이아웃: 배경과 기존 설정 모달은 변경하지 않았다. 일반 UI는 사용자가 승인한 `Image.Type.Simple`을 유지하고 실시간 광석 체력만 `Filled`로 사용한다. 1080 폭·Safe Area 기준에서 16:9, 19.5:9, 20:9의 하단 내비게이션→강화 카드→채굴 버튼→체력바 순서가 겹치지 않도록 좌표를 보정했으며, 긴 화면에서 늘어난 공간은 중앙 광석 연출에 남겼다.
- 현지화·테스트 코드: 스테이지, 전투력, 권장치, 체력, 보스 거리, 방치 보상, Home, 준비 중과 3개 광산명을 4개 언어에 추가했다. HUD 좌표·자산 38개·Simple/해당 Filled 타입·새 기능 진입점을 검사하도록 EditMode 회귀 테스트를 갱신했다.
- 검증 결과: 38개 투명 자산의 시각 접촉 시트와 픽셀 알파·녹색 잔류 검사는 완료했다. 첫 런타임 통합 시점의 Unity 컴파일 오류는 0건이었다. 이후 16:9 간격과 테스트 기대값을 최종 보정했지만 Unity MCP 호출 한도가 소진돼 최종 재컴파일, 전체 EditMode, Play Mode Console, 19.5:9 Simulator 캡처는 실행하지 못했다. Android 빌드·실기기는 처음부터 이번 범위에서 제외했다.
- 남은 작업: MCP 호출이 다시 허용되면 최종 컴파일→전체 EditMode→19.5:9 Simulator 캡처 순서로 검증하고, 실패나 시각 오차가 있으면 Task 12-B 안에서 교정한다. 이 검증 전에는 Task 13-4로 상태를 넘기지 않는다.

## 수정사항 52

- 기록 시각: 2026-07-31 03:18:39
- 작업 요청 요약: V5 HUD의 38개 UI가 실제로 모두 생성·적용됐는지 다시 감사하고, 보류됐던 Unity 최종 검증을 완료한다.
- 자산 감사: 프롬프트의 38개 ID와 `SourceChroma`, `Sprites`, `Resources/PocketForge/UI/V5`를 이름 기준으로 대조했다. 세 폴더 모두 예상 38개·실제 38개·중복 0개·누락 0개·초과 0개였고 투명본과 런타임 사본의 SHA-256 불일치는 0개였다. `MineHudViewV5`에서 참조되지 않은 자산도 0개였다.
- 이미지 교정: 초기 AI 크로마 소스의 녹색이 정확한 `#00FF00`이 아니어서, 검수 완료된 투명본을 원래 캔버스 크기에 유지한 채 순수 `#00FF00` 배경으로 다시 합성했다. Unity 픽셀 감사 결과 투명 모서리 실패 0개, 잔류 녹색 파일·픽셀 0개, 크로마 모서리 실패 0개다.
- 런타임 결함 수정: V5 HUD가 보상형 광고 라벨을 숨긴 뒤 `GetComponentInChildren<Text>()`가 비활성 자식을 찾지 못해 발생한 `NullReferenceException`을 비활성 자식 포함 조회와 null 방어로 수정했다. 또한 Device Simulator/Game View 해상도 변경 직후 이전 프레임의 `Screen.safeArea`가 보고되면 Safe Area 앵커가 1을 넘어 HUD가 한쪽에 압축되는 문제를 발견해, 현재 화면 경계로 값을 제한하고 비정상 면적은 전체 화면으로 복구하도록 했다.
- 시각 검증: Android Game View를 `1080×2340` 고정 해상도(19.5:9)로 설정하고 Overlay Canvas를 포함한 실제 런타임 화면을 `Assets/PocketForge/Art/Generated/UI/V5Guide/Review/V5Hud_1080x2340_Runtime.png`에 캡처했다. 상단 자원·초상·설정, 챕터·전투력·보스, 광석 체력, 방치·연구, 채굴·보스 도전, 강화 카드와 하단 메뉴가 전체 폭에 정상 표시됨을 확인했다.
- 테스트 결과: 최종 강제 스크립트 컴파일 오류 0건, Play Mode Console 오류 0건, 전체 EditMode 123/123 통과다. Android APK/AAB와 실기기 검증은 승인된 Task 12-B 범위에서 제외했다.
- 다음 작업: Task 12-B는 완료다. 다음은 사용자와 등급·드롭·합성 범위를 조율한 뒤 Task 13-4 4슬롯 장비 시스템 Lite 설계를 승인받는다.

## 수정사항 53

- 기록 시각: 2026-08-01
- 작업 요청 요약: 승인된 Task 13-4 범위로 곡괭이·드릴·로봇·부적 4슬롯 장비의 보스 획득, 비교, 장착·해제, 동일 장비 3개 합성, 자동 장착, 저장과 공통 채굴력 반영, 최소 V5 UI를 구현한다.
- 수정 전 상태: 광부 Lv.2 장비 해금과 `MiningPowerModifiers.EquipmentMultiplier` 확장 지점은 있었지만 실제 장비 정의·인벤토리·저장·획득·장착·합성·화면은 없었고 V5 하단 장비 버튼은 준비 중 안내만 표시했다. 저장 버전은 8이었다.
- 데이터·저장: `EquipmentDefinition`과 Pickaxe/Drill/Robot/Charm 슬롯, Common/Rare/Epic/Legendary 등급을 추가했다. `GameSaveData` 버전을 9로 올리고 고유 ID 인벤토리, 슬롯별 장착 참조, 보스 보상 순서를 저장한다. 마이그레이션은 빈 ID, 중복 ID, 등급 범위, 존재하지 않는 인벤토리 참조를 정규화하고 `EquipmentService`가 카탈로그 정의·슬롯 일치를 추가 검증한다.
- 규칙: 보스 처치마다 카탈로그 순서로 다음 슬롯 장비를 1개 지급하며 1~2챕터는 Common, 3챕터 이후는 Rare다. Lv.2부터 장착·해제할 수 있고 자동 장착은 슬롯별 최고 보너스를 선택한다. 장착하지 않은 동일 정의·등급 3개를 다음 등급 1개로 합성하며 전설 등급, 재료 부족, 잠금, 잘못된 장비를 차단한다. 장착 보너스 합계를 `EquipmentMultiplier`에 전달해 자동·탭·오프라인·보스 비교가 동일 계산을 사용한다.
- UI·현지화: `MineHudViewEquipment` 부분 클래스와 V5 장비 탭 연결을 추가했다. 4개 슬롯, 페이지당 6개 장비, 현재 장비 대비 차이, 장착·해제·3개 합성·자동 장착을 제공한다. 새 그래픽과 외부 패키지는 추가하지 않고 기존 V5 장비 아이콘과 설정 모달의 Simple 스킨을 재사용했다. 모든 장비명·등급·슬롯·상태·오류 문구를 한국어·영어·일본어·중국어로 추가했다.
- 시각 교정: 첫 Simulator 검수에서 잘못 지정한 V5 스킨 키가 흰색 폴백으로 보이는 문제를 발견해 실제 `SettingsModal`, `SettingsTitlePlaque`, `SettingsRow`, `SettingsActionButton`, `SettingsCloseButton`으로 교체했다. 두 페이지 이동 버튼의 하단 기준 앵커가 카드 밖으로 나간 것을 중앙 기준으로 교정했다. 기존 HUD와 설정창의 좌표·크기는 변경하지 않았다.
- 테스트 결과: 정적 스크립트 진단 오류 0건, Unity 실제 컴파일 오류 0건이다. 첫 Play Mode 컴파일에서 새 partial 파일의 `PocketForge.Presentation` using 누락을 발견해 교정했다. 최종 전체 EditMode 134/134 통과했다. 1440×3088 Punch Hole Center Device Simulator에 6개 샘플 장비를 주입해 모달 카드·4슬롯·목록·비교·액션 버튼·페이지 이동이 Safe Area 안에 렌더링되는 것을 확인했다. 최종 Console에는 작업과 무관한 기존 Android 빌드 API obsolete 경고 1건만 있었고 런타임 오류는 없었다.
- 검증하지 않은 항목: 승인 범위에 따라 Android APK/AAB 빌드와 실기기 검증은 수행하지 않았다. 장비 전용 신규 그래픽·사운드, 확률 드롭, 미션·상점 획득처, 프리셋·랜덤 옵션·세트 효과·유료 장비는 구현하지 않았다.
- 다음 작업: Task 13-5 광물 박물관·업적·일일/주간 미션의 저장·갱신 시각·보상 범위를 사용자와 조율하고 승인받는다.

## 수정사항 54

- 기록 시각: 2026-08-01 16:04:18
- 작업 요청 요약: 승인된 Task 13-5A 범위로 광물 박물관의 발견·누적 채굴 보너스와 기존 진행 상태를 재사용하는 단계형 업적을 구현하고 저장·공통 채굴력·V5 UI·4개 언어에 연결한다.
- 수정 전 상태: 광부 Lv.3 박물관 해금과 `MiningPowerModifiers.CollectionMultiplier` 확장 지점은 있었지만 V5 박물관 버튼은 준비 중 안내만 표시했다. 광석별 채굴 수, 도감 배율, 업적 정의·수령 상태·보상·화면은 없었고 저장 버전은 9였다.
- 데이터·저장: `OreCollectionData`, `AchievementClaimData`, `AchievementDefinition`을 추가하고 저장 버전을 10으로 올렸다. 광석 ID별 누적 채굴 수와 업적 ID별 수령 단계를 저장하며 마이그레이션은 null·빈 ID·음수·중복 항목을 정규화한다. 서비스 계층은 카탈로그에 없는 ID와 업적 최대 단계 초과를 추가로 제거한다.
- 도감 규칙: Copper, Iron, Gold, Crystal은 최초 발견 시 각각 총 채굴력 +1%, 누적 25·100·500개마다 +1%를 제공해 종류당 최대 +4%가 된다. 온라인 일반·보스 광석 파괴와 오프라인 처리 광석을 같은 집계에 기록한다. 오프라인 생산량은 청구 시작 시점 도감 배율로 먼저 계산한 뒤 채굴 수를 기록해 한 번의 청구 안에서 보너스가 자기 증폭하지 않는다. 도감 배율은 `CollectionMultiplier`로 전달돼 자동·탭·오프라인·보스 비교가 같은 계산 경계를 사용한다.
- 업적 규칙: 누적 채굴, 최고 완료 챕터, 시설 레벨 합, 광부 레벨, 연구 레벨 합, 장비 획득 수의 6개 업적을 각 3단계로 정의했다. 진행도는 기존 저장 상태에서 파생하고 수령한 단계만 저장한다. 다음 단계를 충족한 경우에만 Credits·Gem·설계도 코어를 포화 연산으로 지급하며 잠금·알 수 없는 ID·미달·완료·중복 수령을 차단한다.
- UI·현지화: `MineHudViewCollection` 부분 클래스와 V5 박물관 내비게이션을 연결했다. 박물관 탭은 광석 4종의 발견·누적 수·개별/전체 보너스를, 업적 탭은 6개 목표의 진행·다음 보상·수령 상태를 표시한다. 새 그래픽과 외부 패키지는 추가하지 않고 기존 V5 설정 모달·버튼·광석 아이콘의 Simple 스킨을 재사용했다. 한국어·영어·일본어·중국어 문구와 보상 피드백을 추가했다.
- 최적화·안정성: `CollectionService`와 `AchievementService`가 카탈로그 정의를 생성 시 캐시하도록 해 프레임마다 호출되는 채굴력 계산에서 LINQ·배열 할당을 제거했다. Unity 오브젝트 파괴 뒤 정적 UI 스킨 캐시에 남은 가짜 null 참조를 제거하고 다시 로드하도록 보강했다.
- 테스트 결과: 구현 중 첫 전체 EditMode에서 기존 스킨 캐시 테스트 순서 문제 2건과 도감 발견 배율로 달라진 기존 오프라인 기대값 1건을 발견해 원인에 맞게 수정했다. 최종 강제 컴파일·Console 오류 0건, 전체 EditMode 151/151 통과다. 저장 마이그레이션, 도감 단계·공통 배율·온라인/오프라인 집계·자기 증폭 방지, 업적 진행·3단계 보상·중복 수령 방어, 4개 언어와 V5 실제 박물관 버튼 경로를 포함한다.
- 시각 검증: Device Simulator의 현재 560×1200 미리보기는 목표 1440×3088과 같은 세로 비율이다. 실제 카탈로그 샘플 상태를 비영구 임시 HUD에 주입해 박물관 광석 4행과 업적 6행·수령 버튼이 모달 경계 안에 표시됨을 확인했다. 임시 오브젝트와 EventSystem은 정리됐고 Mine 씬은 검수 전후 모두 미수정 상태였다. 기존 PlayerPrefs 저장값은 변경하지 않았다.
- 검증하지 않은 항목: 승인 범위에 따라 Android APK/AAB 빌드와 실기기 검증은 수행하지 않았다. 박물관 전용 신규 그래픽·대규모 도감, 업적 애니메이션·서버 동기화, 일일·주간 미션은 구현하지 않았다.
- 다음 작업: Task 13-5B 일일·주간 미션의 개수, 일일·주간 갱신 시각, 서버 없는 기기 시간 역행 정책, 보상 구성을 사용자와 조율하고 승인받는다.

## 수정사항 55

- 기록 시각: 2026-08-01 17:07:43
- 작업 요청 요약: Task 13-4 장비와 Task 13-5A 박물관·업적 작업에 사용된 실제 UI 화면을 캡처하고, 세 화면의 기능·정보 위계·시각 개선점과 가스샷 생성 요청문을 하나의 TXT로 정리한다.
- 생성 결과: `Assets/PocketForge/Art/Generated/UI/Task13GasShot/`에 장비, 박물관, 업적 화면을 각각 1203×2178 PNG로 캡처했다. 프로젝트 루트의 `POCKET_FORGE_TASK13_4_TO_13_5_GASSHOT_BRIEF.txt`에는 세 파일의 절대 경로, 1440×3088 최종 목표, 공통 스타일, 화면별 유지 기능, 현재 겹침·간격 문제, 동적 텍스트 제외 규칙, 통합 가스샷 요청문과 승인 후 자산 분리 순서를 기록했다.
- 캡처 방식과 안전성: Mine 씬의 실제 카탈로그와 V5 UI를 사용하되 저장 데이터는 메모리 안의 샘플만 주입했다. 기존 PlayerPrefs와 씬 파일은 변경하지 않았다. 캡처용 HUD와 EventSystem을 제거한 뒤 Mine 씬이 검수 전후 모두 미수정 상태임을 확인했다.
- 시각 검수: 장비 화면은 4슬롯·6개 목록·비교·행동 버튼을, 박물관은 광물 4행과 발견 상태를, 업적은 6행과 수령·진행 상태를 모두 표시한다. 장비 하단 정보와 버튼 겹침, 박물관 아이콘과 텍스트 충돌, 반복 행의 위계 부족은 현재 완성 상태가 아니라 가스샷에서 고칠 문제로 TXT에 명시했다.
- 테스트 결과: 세 PNG의 존재, 1203×2178 해상도와 육안 구성을 확인했고 Unity Console 오류는 0건이다. 코드와 프로젝트 설정은 변경하지 않아 컴파일·EditMode·Play Mode·APK/AAB·실기기 테스트는 수행하지 않았다.
- 다음 작업: 세 캡처와 TXT를 입력으로 장비·박물관·업적의 1440×3088 완성 가스샷을 먼저 생성하고 사용자 승인을 받는다. 승인 전에는 실제 UI 자산 분해나 런타임 적용을 시작하지 않으며 Task 13-5B 상태도 변경하지 않는다.

## 수정사항 56

- 기록 시각: 2026-08-01 17:38:22
- 작업 요청 요약: 현재 장비·박물관·업적 구현 화면과 캐주얼 메인 UI를 바탕으로 상용 방치형 게임의 정보 계층을 참고한 완성 가스샷 3장을 만들고, 메인 화면 배치 왜곡을 교정하는 상세 프롬프트 TXT와 재발 방지용 개인 Codex 스킬을 작성한다.
- 수정 전 상태: 장비 화면은 하단 비교·행동 버튼이 겹쳤고, 박물관은 같은 광석 아이콘이 이름·누적 수를 침범했으며, 업적은 동일 행 반복과 보상·상태 위계가 약했다. 메인 HUD는 프로필·자원 바, 보스 버튼·연구/로봇 카드, 강화 카드·하단 내비게이션 사이의 겹침과 가장자리 잘림이 있었다. 이를 좌표·Safe Area·금지 교차 규칙으로 통제하는 문서와 스킬은 없었다.
- 역기획·디자인 방향: Idle Miner Tycoon 공식 도움말의 장비 카드 활성화·중복 성장·영구 Artifact 보너스 구조와 현재 Pocket Forge 기능을 대조했다. 다른 게임의 고유 화면은 복제하지 않고 장비의 즉시 비교·장착 상태, 박물관의 전체 영구 보너스·다음 마일스톤, 업적의 고정 진행/보상/상태 열만 Pocket Forge 규모에 맞게 재구성했다.
- 생성 결과: `Assets/PocketForge/Art/Generated/UI/Task13GasShot/`에 `Task13_4_Equipment_GasShot_v2.png`, `Task13_5_Museum_GasShot_v2.png`, `Task13_5_Achievements_GasShot_v2.png`를 추가했다. 장비는 4슬롯과 2×3 인벤토리·분리 비교 트레이, 박물관은 2×2 전시 카드·전체/개별 마일스톤, 업적은 6개 고정 열 행과 수령/완료/진행 상태를 사용한다. 박물관 첫 결과의 상·하단 발견 수치 불일치는 한 영역 편집으로 `3/4`에 맞췄다.
- 배치 방지 산출물: 루트에 `POCKET_FORGE_MAIN_UI_LAYOUT_CORRECTION_PROMPT.txt`를 생성해 1440×3088 기준 안전선, 수직 밴드, 카드 좌표, 최소 간격, 절대 금지 교차, 이미지 편집 요청문, 생성 후 오버레이 검수와 Unity 적용 원칙을 기록했다. 개인 경로 `C:\Users\jacob\.codex\skills\pocket-forge-ui-layout-guard`에는 `SKILL.md`와 `agents/openai.yaml`을 생성해 기능 인벤토리→레이아웃 계약→프롬프트→가스샷 검수→UGUI 적용→다국어·해상도 회귀 순서를 고정했다.
- 수정 후 상태: 세 가스샷은 기존 캡처를 덮어쓰지 않고 1440×3088 PNG로 보관되며, 메인 화면 교정과 향후 신규 화면 작업에서 동일한 비겹침 기준을 재사용할 수 있다. Unity 씬·코드·RectTransform·현재 런타임 자산과 Task 13-5B 상태는 변경하지 않았다.
- 검증 결과: 세 PNG를 원본 비율 중앙 크롭 후 1440×3088로 고품질 리샘플링하고 파일 존재·해상도·육안 구성을 확인했다. 박물관·업적의 모달 규격과 장비 반복 카드 정렬을 검수했다. 스킬 공식 `quick_validate.py`는 실행 환경에 `PyYAML`이 없어 import 단계에서 중단됐으며 외부 패키지는 설치하지 않았다. 대신 SKILL.md 존재, YAML 구분자, name/description, 허용 키, 64자 이하 hyphen-case 이름, 설명 길이·금지 문자, 176줄 길이와 `agents/openai.yaml`의 스킬 호출 문구를 수동 확인했다. 이미지와 문서 작업이므로 Unity 컴파일·EditMode·Play Mode·APK/AAB·실기기는 실행하지 않았다.
- 남은 작업: 사용자 가스샷 승인 후에만 텍스트 없는 패널·버튼·아이콘 분리와 실제 UGUI 재배치를 별도 승인받아 진행한다. 공식 스킬 검증기를 다시 실행하려면 PyYAML이 있는 환경을 사용하거나 패키지 설치 승인을 먼저 받아야 한다.

## 수정사항 57

- 기록 시각: 2026-08-01 17:53:27
- 작업 요청 요약: 승인된 장비·박물관·업적 가스샷을 메인 화면 자산 제작 때처럼 요소 하나씩 분리하되, 가스샷과 100%에 가까운 형태·색·재질을 유지하도록 상세 생성 프롬프트를 하나의 TXT로 작성한다.
- 수정 전 상태: 1440×3088 완성 가스샷 3종과 메인 화면 요소별 프롬프트는 있었지만, 장비·박물관·업적의 패널·카드·버튼·상태 오버레이·개별 아이콘을 실제 제작 단위로 나눈 전용 문서는 없었다. 일반적인 스타일 참고 생성만 사용하면 반복 카드의 곡률·테두리·광택과 화면별 색이 달라질 위험이 있었다.
- 수정 내용: 루트에 `POCKET_FORGE_EQUIPMENT_MUSEUM_ACHIEVEMENT_ASSET_PROMPTS.txt`를 생성했다. 승인 PNG를 절대 기준으로 쓰는 원본 크롭 제한 편집 절차, 분리 불가능할 때만 쓰는 참조 기반 재생성 절차, 무문자·동적 데이터 분리·크로마키 규칙과 함께 장비 30종, 박물관 22종, 업적 15종의 개별 `Primary request`와 권장 파일명을 기록했다.
- 자산 구조 원칙: 기본 카드와 희귀도·선택·장착·합성·완료·잠금 상태를 분리하고, 장비 아이콘·광물 전시물·업적 카테고리 아이콘을 독립 자산으로 만들도록 했다. Credits·Gem·설계도 코어·닫기·알림·잠금처럼 기존 자산과 같은 요소는 재생성하지 않고 재사용하도록 명시했다. 모든 제목·설명·가격·수량·레벨·퍼센트·진행도·보상 값은 Unity TextMeshPro와 런타임 데이터에 남긴다.
- 근접도 검수: 승인 가스샷과 같은 1440×3088 캔버스에 결과 자산을 원래 위치·크기로 다시 놓고 50% 오버레이 또는 Difference 비교를 수행하도록 했다. 큰 모달 4px, 카드·버튼 3px, 작은 아이콘 표시 크기 3%의 외곽 오차 기준과 반복 카드 크기·곡률 일치 기준을 문서에 포함했다.
- 수정 후 상태: 요소별 생성 요청문을 한 파일에서 복사해 사용할 수 있으며, 단순 유사 스타일 재생성보다 원본 크롭의 제한 편집을 우선해 가스샷과의 형태 차이를 줄인다. 현재 Unity 씬·코드·RectTransform·런타임 자산과 Task 13-5B 상태는 변경하지 않았다.
- 검증 결과: 새 TXT가 UTF-8로 정상 표시되고 파일 크기 42,003바이트이며, 시작·종료 구간과 장비·박물관·업적 섹션, 공통 편집/재생성 블록, 텍스트 금지, 조립·검수·Unity 임포트 메모가 포함된 것을 확인했다. 문서 작성 작업이므로 이미지 생성·투명화·Unity 컴파일·EditMode·Play Mode·Android 검증은 수행하지 않았다.
- 남은 작업: 실제 자산 생성은 각 가스샷에서 대상 요소를 원본 해상도로 크롭한 뒤 이 문서의 공통 프롬프트 A와 해당 `Primary request`를 결합해 한 요소씩 진행한다. 생성 결과 승인 후에만 투명화·Sprite 임포트·UGUI 조립을 별도 범위로 수행한다.

## 수정사항 58

- 기록 시각: 2026-08-01 19:20
- 작업 요청 요약: 승인된 장비·박물관·업적 가스샷과 두 전용 TXT를 그대로 사용해 요소별 UI 자산을 생성·투명화·최적화하고 실제 UGUI에 적용한다. 동시에 메인 HUD를 1440×3088 좌표·Safe Area·수직 밴드 기준으로 교정하되 게임 규칙과 저장 데이터는 변경하지 않는다.
- 자산 생성·가공: `Assets/PocketForge/Art/Generated/UI/Task13GasShotV2/SourceChroma`에 요소별 생성 원본 56개를 보관하고, 공용 V5 재사용·희귀도 파생 자산을 포함한 67개 런타임 PNG를 `Resources/PocketForge/UI/Task13`에 구성했다. Unity 안에서 크로마 제거, despill, 알파 트림, 최대 256·512·1024px 다운스케일을 수행했다. 생성기가 녹색 대신 흰색·마젠타 배경을 만든 `ButtonAchievementClaim`과 `UiCollectionModalBody`는 연결 영역 제거와 색 프린지 보정으로 교정했다.
- 런타임 구조: `MineUiSkin`에 Task13 전용 캐시·Simple Sprite 로더를 추가했다. 장비 모달은 4슬롯, 2×3 인벤토리, 희귀도·선택·장착·합성·수량 오버레이, 비교 트레이와 장착·해제·합성·자동 장착 버튼을 실제 장비 상태로 조립한다. 박물관은 2×2 전시 카드, 광물별 아이콘·잠금·마일스톤 Filled 진행도·다음 도감 보상을 표시한다. 업적은 6개 고정 5열 행에 카테고리 아이콘, 목표, Filled 진행도, 실제 보상 아이콘·수량, 수령·진행 중·완료 상태를 조립한다.
- 메인 HUD 교정: `POCKET_FORGE_MAIN_UI_LAYOUT_CORRECTION_PROMPT.txt`의 1440×3088 좌표를 1080 기준으로 한 번만 0.75배 변환했다. 상단 HUD, 챕터·전투력, 보스 경고, 체력, 액션, 보스 CTA, 강화 카드, 하단 내비게이션을 독립 밴드로 배치했다. 체력 바와 하단 영역은 바닥 앵커를 사용해 1920·2340·2400 가상 높이에서도 순서가 바뀌지 않게 했고, 하단 6개 메뉴는 동일 간격으로 정렬했다.
- 이미지 타입·다국어: 사용자가 지정한 대로 표면·카드·버튼·아이콘은 전부 `Image.Type.Simple`이며 박물관·업적 진행 채움만 `Image.Type.Filled`다. 제목·설명·가격·수량·레벨·퍼센트·진행도·버튼 문구는 기존 `LanguageService`와 런타임 데이터로 남겨 한국어·영어·일본어·중국어 구조를 유지했다.
- 시각 검수: `Task13AssetContactSheet.png`, 메인 HUD·장비·박물관·업적 1440×3088 런타임 캡처, 메인 가스샷 50% 오버레이를 `Task13GasShotV2/Review`에 저장했다. 장비 하단 겹침, 박물관 마젠타 배경·빈 하단, 업적 반복 행 위계를 실제 캡처에서 확인하고 교정했다. 자산 구성·재사용·검수 파일은 `ASSET_MANIFEST.md`에 기록했다.
- 검증 결과: Unity 강제 컴파일과 최종 Console 오류는 0건이다. 전체 EditMode 152/152, UI·다중 화면비 대상 28/28이 통과했다. 1440×3088 실제 렌더에서 모든 HUD와 세 모달이 Safe Area 안에 표시되고 장비 선택·장착, 박물관 탭, 업적 수령 상태가 기존 서비스 데이터와 연결됨을 확인했다.
- 검증하지 않은 항목: Android APK/AAB 빌드와 실기기 테스트는 사전 승인 범위에서 제외했다. 외부 패키지·SDK·Unity 프로젝트 설정·게임 밸런스·저장 스키마는 변경하지 않았다.
- 다음 작업: `TASKS.md`의 다음 작업인 Task 13-5B 일일·주간 미션의 목표 풀, 갱신 경계, 보상 규모를 사용자와 조율한 뒤 별도 승인으로 구현한다.

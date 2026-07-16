# TASKS.md

## 현재 작업

현재 작업: 없음 — 다음 구현 Task 승인 대기

### Task 8 — 모바일 HUD UI 폴리싱

상태: 진행 중

- 상단 정보·광석·주 행동·강화 영역을 카드형 시각 계층으로 재구성
- 진행 바와 오프라인 보상 안내를 명확한 강조 패널로 개선
- 기존 입력·다국어·채굴 로직을 변경하지 않고 세로 화면 가독성 검증
- 설정창·언어 선택 UI는 후속 Task로 제외

## 완료 작업

### Task 8 — 모바일 HUD UI 폴리싱

상태: 완료

- 정보·주 행동·강화 영역을 카드형 패널과 그림자로 구분
- 오프라인 보상 안내를 강조 패널로 개선
- EditMode 13/13 통과, Android APK 빌드 및 SM-S938N 화면 확인

### Task 7 — 자동 채굴 오프라인 보상

상태: 완료

- 저장 데이터 버전을 3으로 올리고 마지막 저장 Unix 시각을 기록
- 드릴 자동 채굴력과 현재 광석 보상 효율로 오프라인 Credits 계산
- 최대 4시간으로 보상 시간을 제한하고 재접속 시 UGUI 보상 문구 표시
- EditMode 13/13 통과, Android 개발 APK 빌드·SM-S938N 설치·실행 및 Unity/AndroidRuntime 오류 logcat 0건
- 전체 UI 리디자인과 설정창은 보류

### Task 6 — 콘텐츠 카탈로그 기반 채굴 루프 확장

상태: 완료

- `OreDefinition`, `UpgradeDefinition`, `MiningContentCatalog` ScriptableObject 구조 추가
- 구리(1단계~)·크리스털(10단계~) 광석과 곡괭이·드릴·로봇 강화 자산을 카탈로그에 등록
- `MiningGameService`가 카탈로그에서 내구도·보상·강화 비용·효과를 조회하도록 전환
- `Mine` 씬의 `MineGameController.contentCatalog`에 카탈로그 자산 연결
- EditMode 12/12 통과, Editor Play Mode Console 오류 0건, Android 개발 APK 빌드·SM-S938N 설치·실행 및 화면 표시 확인
- 설정창과 언어 선택 UI는 보류 상태로 유지

### Task 5 — 다국어 기반과 설정 언어 선택

상태: 완료

- Unity Localization 패키지 `1.5.12` 추가
- 한국어·영어·일본어·중국어 간체 코드 문자열 테이블과 기기 언어 기본값 추가 (String Table 에셋 분리는 설정창 작업에서 진행)
- `LanguageService.SetLanguage`으로 선택 저장·즉시 UI 갱신 경로 제공
- 채굴 화면의 제목·광석·채굴·강화 문구를 언어 키 기반으로 전환
- EditMode 테스트 11/11 통과

### Task 4 — 생성 리소스 기반 비주얼 개선

상태: 완료

- 생성 광석 아트와 강화 아이콘 시트를 투명 PNG 에셋으로 변환해 프로젝트에 추가
- URP Unlit 머티리얼과 광석 빌보드로 3D 채굴 오브젝트에 광석 아트 적용
- Pickaxe·Drill·Robot 강화 버튼에 아이콘 시트의 각 영역을 UGUI `RawImage`로 연결
- Editor Play Mode 콘솔 오류 0건, EditMode 테스트 6/6 통과, Android 개발 빌드 성공(오류 0건·경고 1건)
- 최신 APK를 SM-S938N에 설치·실행하고 생성 광석 아트·강화 아이콘 표시를 확인

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

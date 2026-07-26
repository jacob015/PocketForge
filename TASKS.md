# TASKS.md

## 현재 작업 (2026-07-26 갱신)

현재 작업: Task 12 스냅샷 UI 분해·실기기 적용 완료 — 사용자 최종 화면 승인 대기

- 완료: HUD·설정 레퍼런스에서 글자를 제외한 패널·버튼·아이콘을 47개 투명 PNG로 분해하고 Android ASTC 6x6 임포트 설정 적용
- 완료: `MineUiSkin` 리소스 경계와 캐시를 추가해 런타임 레이아웃 코드와 생성 UI 자산 로딩·9-slice 책임 분리
- 완료: 상단 자원·광고·광석 상태·채굴·강화 카드와 설정 모달·오디오·접근성·언어·결제 행을 레퍼런스 비율로 교체
- 검증: Unity 컴파일 오류 0건, EditMode 44/44 통과, 1440×3088 SM-S938N에서 HUD·설정 모달·Safe Area와 채굴 입력 `10/10 → 09/10` 확인
- 검증: 별도 `.dev` 비개발 APK 51,918,068바이트(49.51MiB) 빌드·설치·기동 성공; 출시 패키지명·Custom Keystore·AAB 설정은 원래 값으로 복구
- 다음: 사용자 화면 승인 시 Task 12를 닫고 승인된 순서에 따라 게임 내 콘텐츠 확장으로 이동
- 완료: AI 레퍼런스의 시각 밀도에 맞춰 상단 자원 바·설정·광고 보상 버튼·광석 상태·주황 채굴 버튼·강화 카드 3개의 크기와 간격을 다시 설계
- 완료: 설정·추가·광고·크리스털 아이콘을 AI 생성 전용 HUD 아이콘 시트로 교체하고 Android ASTC 6x6 임포트 설정 적용
- 완료: `CanvasScaler`를 세로 화면 기준 너비 매칭으로 고정하고 1920·2340·2400 가상 높이 및 1080 기준 폭의 비겹침 회귀 테스트 5개 추가
- 완료: 1440×3088 Device Simulator 최종 스냅샷과 SM-S938N 실기기에서 전체 HUD·설정 모달·채굴 입력을 확인
- 검증: Unity 컴파일/Play Mode 오류 0건, EditMode 44/44 통과, Android 개발 APK 63,042,742바이트 빌드·별도 `.dev` 패키지 설치 성공, 채굴 입력으로 `광석 10/10 → 09/10` 확인
- 참고: ADB 사이드로드 `.dev` 패키지는 Play 설치본이 아니므로 IAP 상품 조회 실패가 예상되며, 선택적 Play Asset Pack 클래스 탐색 로그는 앱 실행·렌더링·입력에 영향을 주지 않음
- 완료: Play Console 내부 테스트 트랙과 라이선스 테스터를 구성하고 비소모성 `remove_ads` 상품을 KRW 1,100으로 활성화
- 완료: Play 설치본에서 무청구 라이선스 테스트 구매, 명시적 구매 복원, 앱 삭제·Play 재설치 후 구매 권한 자동 복구를 SM-S938N에서 확인
- 완료: 구매 후에도 공식 보상형 테스트 광고가 표시되고 보상이 지급되며, 180초 경과·광석 5개 파괴 뒤 강제 전면 광고는 표시되지 않음을 확인
- 다음: 사용자 최종 화면 승인 시 Task 12를 닫고 승인된 순서에 따라 게임 내 콘텐츠 확장으로 이동

- 완료: 실제 Device Simulator 화면과 기존 배경을 기준으로 HUD·설정창 완성 콘셉트를 생성하고 레이아웃에 반영
- 완료: Safe Area와 `CanvasScaler`를 기준으로 16:9(750×1334), 19.5:9(1080×2340), 20:9급(1440×3088) Simulator에서 핵심 HUD·설정 카드 경계 검증
- 완료: 모든 런타임 버튼에 눌림·복귀 애니메이션, 강화 성공 카드 펄스와 풀링된 8개 스파크 이펙트 적용; 모션 감소 설정 시 비활성화
- 완료: 업그레이드 성공 텍스트를 제거하고 AI 생성 전용 9-slice 피드백 패널을 보상·오류 메시지 배경으로 적용
- 완료: 네 언어 CJK 대응 동적 시스템 폰트, 텍스트 색·크기·위계, 상단 자원 바와 하단 카드 비율 정리
- 완료: 설정창에 BGM·효과음 볼륨/음소거, 진동, 모션 감소, 언어, 광고 제거·복원 기능을 한 화면으로 구성하고 설정 저장 추가
- 제한: 오디오 생성 제공자가 구성되지 않아 오디오 라우팅과 설정은 구현했지만 BGM·효과음 클립은 아직 비어 있음
- 검증: Unity 컴파일 오류 0건, EditMode 39/39 통과, PlayMode 등록 테스트 0건, 세 화면비 런타임 경계와 20:9 Play Mode 오류 0건 확인
- 다음: 사용자 화면 검수 결과에 따라 Task 12 비주얼을 추가 보정하거나, 승인 시 Play Console 인증 완료 후 IAP 실제 구매·복원 검증 재개
- 대기: Play Console 개발자 계정 인증 완료 후 `remove_ads` 실제 구매·복원 검증 재개

- 완료: AI 생성 UI 패널·버튼·카드를 하나의 1024px ASTC 6x6 아틀라스로 적용
- 완료: Copper, Iron, Gold, Crystal Meshy 원본을 모바일 네이티브 메시·512px ASTC 텍스처로 최적화
- 완료: 씬과 OreDefinition에서 고용량 원본 GLB 런타임 참조 제거
- 완료: 최신 릴리스 APK 37,178,105바이트(35.46MiB), 목표 50MB 이하 유지
- 완료: EditMode 18/18 및 SM-S938N 설치·실행·채굴 입력 검증
- 완료: AI HUD 레퍼런스의 상단 자원 바·중앙 대형 광석·진행 바·주황 채굴 버튼·세로형 강화 카드 3개 구성을 실제 HUD에 재현
- 완료: 배경을 3D 광석 뒤의 카메라 쿼드로 이동하고 화면비 변경 시 자동 리사이즈하도록 개선; 중복 Ground 제거
- 완료: Device Simulator를 중앙 펀치홀 Android 세로 기기(1440×3088)로 전환해 검수하고 SM-S938N 실제 화면과 설정 모달을 확인
- 완료: 글자 없는 AI 생성 업그레이드 버튼을 별도 512px ASTC 6x6 에셋으로 적용하고 Safe Area·상단 패널·광석 내구도 바·채굴 버튼·하단 카드 비율 정리
- 완료: Android 패키지명을 `com.jacob015.pocketforge`, 버전명을 `0.1.0`, 버전 코드를 `1`로 확정
- 완료: Git 외부의 바탕화면에 RSA 4096 출시 키스토어와 복구 정보를 생성하고 환경변수 기반 서명 AAB 빌드 경로 추가
- 완료: 서명 AAB 37,091,264바이트(35.37MiB) 생성, Bundletool 구조 검증·매니페스트·서명 인증서 일치 확인
- 완료: 클릭 애니메이션, 업그레이드 문구 대체 이펙트, CJK 대응 런타임 폰트를 최종 폴리싱 1차 범위에 적용
- 완료: Google Mobile Ads Unity Plugin 11.3.0과 공식 Android 테스트 앱·광고 단위 ID 적용
- 완료: 일반 광석 보상의 5배를 지급하는 보상형 광고와 실패 시 수동 재시도 UI 구현
- 완료: 광석 5개 파괴 및 최소 180초 간격의 전면 광고 정책 구현; 광고 실패는 게임 진행을 차단하지 않음
- 완료: 광고 상태 문구를 한국어·영어·일본어·중국어 간체에 추가
- 완료: EditMode 21/21, 서명 AAB·Manifest·인증서 검증 및 SM-S938N 설치·기동·광고 실패 복구 UI 확인
- 완료: SM-S938N 사설 DNS를 자동 모드로 전환한 뒤 공식 보상형 테스트 광고가 실제 표시되고, 완료 후 크레딧이 `20 → 80 C`로 증가하며 광고가 다시 로드되는 것을 확인
- 완료: 앱 활성 180초 이상과 광석 5개 파괴 조건에서 공식 전면 테스트 광고가 표시되고, 닫은 뒤 심도 `11`의 게임 진행으로 정상 복귀하는 것을 확인
- 완료: 광고 실기기 검증 후 MCP EditMode 29/29 통과 및 AndroidRuntime 크래시·`JavascriptEngine` 오류 없음 확인
- 완료: Unity IAP 5.4.1과 비소모성 `remove_ads` 상품을 코드 기반 카탈로그로 연결
- 완료: `IIapService`·`UnityIapService`·`MineIapCoordinator`로 스토어 SDK, 저장 권한, UI 조정을 분리
- 완료: 광고 제거 권한을 저장 데이터 버전 4에 영속화하고 저장 성공 후에만 구매를 승인; 강제 전면 광고만 제거하고 보상형 광고는 유지
- 완료: 설정창에 현지화 가격·구매·복원·완료·실패·취소·보류 상태를 네 언어로 추가하고 SM-S938N 화면 겹침 제거
- 완료: Kotlin 1.6.21/1.8.22 충돌을 Base Gradle 의존성 정렬로 해결
- 완료: EditMode 29/29, Release AAB 48,608,517바이트(46.36MiB), Bundletool·서명 검증, SM-S938N 설치·기동·상품 `$0.01` 조회·설정 UI 확인
- 완료: Unity IAP 5.4.1의 `RestoreTransactions` 결과 전달과 중복되던 추가 `FetchPurchases` 호출 제거
- 완료: 복원 권한 저장·중복 구매 차단·이벤트 해제 회귀 테스트 추가 및 EditMode 32/32 통과
- 완료: 업로드 대기용 Release AAB `PocketForge-0.1.0-iap-release4.aab` 48,610,452바이트(46.36MiB) 생성, 패키지·버전·구조·서명 인증서 일치 확인
- 완료: Play Console 내부 테스트 트랙, 라이선스 테스터, KRW 1,100 비소모성 `remove_ads` 상품을 구성
- 완료: 무청구 테스트 구매·명시적 복원·삭제 후 Play 재설치 자동 권한 복구·보상형 광고 유지·강제 전면 광고 제거를 SM-S938N에서 실제 검증

### 승인된 남은 작업 순서

1. 광고 실기기 검증 완료
2. 인앱 결제 실제 구매·복원 검증 완료
3. UI·그래픽 최종 폴리싱
4. 게임 내 콘텐츠 확장
5. 성장·보상·광고 밸런싱
6. 최종 최적화와 QA
7. Jenkins Android AAB 빌드 자동화
8. Google Play 비공개 테스트 배포
9. 출시 준비와 포트폴리오 정리

- Play Console 개발자 인증 대기 동안 사용자 승인에 따라 UI 폴리싱을 선행하고, 인증 완료 후 IAP 실제 검증을 재개한다.
- 콘텐츠 추가는 UI 폴리싱 완료 후, 수치 밸런싱은 콘텐츠 구성이 확정된 후 진행한다.

## 현재 작업

현재 작업: 없음 — Task 11 콘텐츠 확장 진행 대기

### Task 10 — 채굴·강화 피드백
상태: 완료

- 광석 파괴 보상과 강화 성공/실패를 HUD 피드백으로 표시
- 규칙 계층은 결과 데이터만 반환하고 View가 토스트 표시를 담당
- EditMode 17/17 통과, Android APK 빌드 및 SM-S938N 설치·실행 확인

### Task 9 — 설정 언어 선택 UI
상태: 완료

- 상단 설정 버튼과 런타임 UGUI 설정 모달을 추가
- `한국어`, `English`, `日本語`, `简体中文` 선택을 `LanguageService.SetLanguage`에 연결
- 선택 즉시 HUD와 설정 UI를 재렌더링하고 PlayerPrefs에 선택 언어 저장
- EditMode 17/17 통과, Android APK 빌드 및 SM-S938N에서 영어 전환 확인

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
<!-- 2026-07-16: Task 11 content expansion verified. Task 12 visual redesign is now active. -->
<!-- 2026-07-17: Task 12 checkpoint: a Meshy image-to-3D turquoise crystal ore GLB is imported through glTFast, assigned in Mine.unity, and verified in an Android development APK on SM-S938N. UI polish remains active. -->
<!-- 2026-07-17: Task 12 checkpoint: Copper, Iron, Gold, and Crystal now resolve their own Meshy-generated GLB through OreDefinition; the HUD was reorganized into compact header, visible mine stage, bottom mine action, and horizontal upgrades. EditMode 18/18 and an SM-S938N Android development APK launch passed; release-size optimization remains pending. -->

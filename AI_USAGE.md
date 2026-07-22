# AI 활용 및 검증 기록

| 날짜 | 도구 | 사용 목적 | AI 산출물 | 개발자가 검증·수정한 내용 | 결과 |
|---|---|---|---|---|---|
| 2026-07-16 | ChatGPT | 초기 기획 및 Codex 협업 구조 설계 | 프로젝트 구조·2주 계획 | 실제 Unity 프로젝트 생성 전 문서 구조만 검토 | 진행 중 |
| 2026-07-16 | Codex | 루트 문서의 역할·참조 관계 정리 | 문서 색인, 작업 상태·계획 문구 정리 | 문서 간 링크와 현재 상태만 확인; Unity·코드·외부 설정은 미검증 | 문서 정리 완료 |
| 2026-07-16 | Codex + Unity MCP | 기존 Unity 프로젝트 기반 설정 정비 | 제품명·세로 화면 설정, Git ignore, 작업 상태 갱신 | Unity Editor에서 `PocketForge`·Portrait·시작 씬·Console 오류를 확인; Android SDK/JDK와 실제 빌드는 미검증 | Task 0 완료 |
| 2026-07-16 | Codex + Unity MCP | 기존 구현 현황 감사 | 현재 아키텍처 문서, 계획·작업 상태 정합화 | `Mine` 씬·스크립트·Unity 씬 검증을 확인하고 EditMode 테스트 3개 통과 | Task 1 완료 |
| 2026-07-16 | Codex + GitHub CLI | 공개 원격 저장소 연결 | GitHub CLI 설치, 저장소 생성, 초기 커밋·푸시 | GitHub 인증 및 `origin/main` 추적 상태 확인 | 완료 |
| 2026-07-16 | Codex + Unity MCP | 채굴 수직 슬라이스 Play Mode 기동 확인 | 런타임 화면 캡처, Android 환경 사전 점검 | `Mine` 씬 기동 및 Console 오류 0건 확인; Android Playback Engine 미설치 확인 | 진행 중 |
| 2026-07-16 | Codex + Unity Hub | Android 개발 종속성 설치 확인 | Android Build Support, SDK·NDK Tools, OpenJDK 설치 | Unity `6000.5.4f1`의 AndroidPlayer 아래 SDK·NDK·OpenJDK 폴더 존재 확인; MCP 브리지 재연결은 미완료 | 진행 중 |
| 2026-07-16 | Codex + Unity MCP | Android 개발용 APK 빌드 검증 | `PocketForge-dev.apk` 생성 및 빌드 상태 기록 | Android 대상·빌드 씬·Console을 확인하고 APK 빌드 성공, 파일 존재 확인; 연결 기기가 없어 실기기 검증은 미수행 | 진행 중 |
| 2026-07-16 | Codex + Unity MCP | Editor 수동 채굴·강화·저장 흐름 검증 | Play Mode 기동, UI 노출·검증 기록 | 사용자가 `MINE`·`PICKAXE` 입력 및 재기동 후 저장 상태 유지를 직접 확인; 실기기 검증은 미수행 | 진행 중 |
| 2026-07-16 | Codex + ADB | Android 실기기 APK 설치·기동 검증 | APK 설치, Activity·프로세스·화면·logcat 확인 | SM-S938N에서 앱 기동과 UI 표시 확인; `SphereCollider` 누락 런타임 오류를 발견해 코드 수정 전 보류 | 결함 발견 |
| 2026-07-16 | Codex + Unity MCP + ADB | Android 광석 생성·표시 오류 수정 및 재검증 | primitive 의존 타입 참조, URP Lit 광석 Material, APK 재빌드·실기기 재검증 | Unity 공식 `CreatePrimitive` 문서의 스트리핑 요구 사항을 적용; Editor·SM-S938N 화면과 새 앱 프로세스 logcat으로 오류 부재·주황색 광석을 확인; EditMode 3개 통과 | 완료 |

## 기록 원칙

- AI가 제안한 내용과 실제 적용한 내용을 구분한다.
- 공식 문서, 컴파일, Unity Console, 기기 테스트 중 무엇으로 검증했는지 남긴다.
- 검증하지 못한 제안은 적용 완료로 기록하지 않는다.
-
| 2026-07-16 | Codex + Unity MCP + ADB | 확장 가능한 채굴 구조와 UGUI 전환 | 순수 상태·서비스, ScriptableObject 설정, 저장 정규화, UGUI 뷰·프레젠터 | Unity 컴파일 오류 0건, EditMode 6/6 통과, Editor Play Mode 콘솔 오류 0건, Android APK 빌드 성공 및 SM-S938N 설치·전면 실행 확인 | 완료 |
| 2026-07-16 | Codex + imagegen + Unity MCP + ADB | 생성 리소스 기반 비주얼 개선 | 광석 아트, 강화 아이콘 시트, 투명 PNG, URP Unlit 머티리얼, UGUI 아이콘 | 크로마키 제거 결과를 Unity 에셋으로 적용; Editor Play Mode 오류 0건, EditMode 6/6 통과, Android 빌드·SM-S938N 설치·실행 성공 및 화면 표시 확인. Unity 서비스 연결과 선택적 Play Asset Pack 관련 로그는 남음 | 완료 |
| 2026-07-16 | Codex + Unity MCP | 다국어 기반 | Unity Localization 패키지, 네 언어 코드 문자열 테이블, 기기 언어 기본값, 저장·즉시 갱신 서비스 | Unity 컴파일 오류 0건, EditMode 11/11 통과. String Table 에셋과 설정창은 후속 작업에서 서비스 API에 연결 | 완료 |
| 2026-07-16 | Codex + Unity MCP + ADB | 콘텐츠 카탈로그 기반 채굴 루프 확장 | 광석·강화 ScriptableObject 정의, 카탈로그 자산, 서비스 조회 로직, 10단계 크리스털 콘텐츠 | Unity 컴파일 오류 0건, EditMode 12/12 통과, Editor Play Mode Console 오류 0건, Android APK 빌드·SM-S938N 설치/실행/화면 확인 및 Unity·AndroidRuntime 오류 logcat 0건 | 완료 |
| 2026-07-16 | Codex + Unity MCP + ADB | 자동 채굴 오프라인 보상 | 저장 시각 마이그레이션, 4시간 상한 보상 계산, UGUI 재접속 보상 안내 | Unity 컴파일 오류 0건, EditMode 13/13 통과, Android APK 빌드·SM-S938N 설치/실행 및 Unity·AndroidRuntime 오류 logcat 0건 | 완료 |
| 2026-07-16 | Codex + Unity MCP + ADB | 모바일 HUD UI 폴리싱 | 카드형 HUD 표면, 버튼 외곽선·그림자, 오프라인 보상 강조 패널 | Unity 컴파일 오류 0건, EditMode 13/13 통과, Android APK 빌드·SM-S938N 화면 캡처 확인 | 완료 |
| 2026-07-17 | Codex + imagegen + Meshy + Unity MCP + ADB | Task 12 genuine AI 3D ore | Original concept image, Meshy image-to-3D textured GLB, glTFast import, scene assignment | Meshy-generated model was visually reviewed from four angles, then tested in Unity and on SM-S938N; rejected FBX remains unassigned because its texture did not import correctly | Integrated and verified |
| 2026-07-17 | Codex + imagegen + Meshy + Unity MCP + ADB | Task 12 ore variants and HUD layout | Three original ore concepts (copper, iron, gold), three textured Meshy GLBs, OreDefinition visual references, and a bottom-action HUD layout | Each asset reference was validated by Unity serialization; copper GLB was visually reviewed in Scene View; Unity console errors 0, EditMode 18/18 passed, and Android development APK installed/launched on SM-S938N with no captured Unity, AndroidRuntime, glTF, or exception logs. | Integrated and device launch verified |
| 2026-07-18 | Codex imagegen + Unity MCP + ADB | Task 12 HUD art and mobile build optimization | Generated a portrait HUD reference and text-free chroma UI kit; converted the kit to a transparent reusable atlas; simplified four Meshy ore assets into native mobile meshes and ASTC textures | Kept localized text in Unity rather than baking text into images; visually reviewed the atlas and device HUD; verified no GLB runtime dependency, Unity EditMode 18/18, APK v2 signature, 37,146,673-byte release APK, SM-S938N launch, and mining input 62→56 | Integrated and device verified |
| 2026-07-18 | Codex imagegen + Unity MCP + ADB | HUD 비율·안전 영역·업그레이드 버튼 정리 | 기존 UI 키트 스타일을 참조한 글자 없는 라임색 업그레이드 버튼 PNG | 크로마키를 투명 처리하고 여백을 제거한 뒤 512px ASTC 6x6로 제한했다. Device Simulator와 SM-S938N에서 Safe Area, 설정 버튼 분리, 광석 내구도 바, 대형 곡괭이 채굴 버튼, 강화 카드 내부 버튼을 검증하고 EditMode 18/18 및 37,178,105바이트 APK를 확인했다. | 적용·실기기 검증 완료 |
| 2026-07-22 | Codex + Unity batch build + Android signing tools | Google Play 배포 식별자·서명 기반 준비 | 환경변수 기반 Android Release AAB 빌드 자동화와 로컬 출시 키 설정 | 임시 Unity 프로젝트에서 AAB 빌드 성공, Bundletool 구조·패키지·버전 검증, jarsigner 서명 검증, AAB와 키스토어 SHA-256 인증서 지문 일치를 확인했다. 비밀정보와 키스토어는 Git 외부에 보관했다. | 적용·서명 빌드 검증 완료 |
| 2026-07-22 | Codex + Google Mobile Ads + Unity batch build + ADB | 테스트 광고 보상·실패 처리 | 광고 SDK 어댑터, 보상형 광고 5배 보상, 전면 광고 노출 정책, 네 언어 상태 UI | 공식 테스트 ID만 사용했다. Unity 컴파일 및 EditMode 21/21, 서명 AAB 구조·Manifest·인증서, SM-S938N 설치·기동·SDK 요청과 실패 복구 UI를 검증했다. 사설 DNS 환경의 JavascriptEngine 로드 실패로 실제 광고 완료 콜백은 네트워크 설정 변경 후 재검증 대상으로 남겼다. | 구현 완료·기기 네트워크 재검증 필요 |
| 2026-07-22 | Codex + Unity IAP + Unity batch build + Bundletool + ADB | 비소모성 광고 제거 결제 클라이언트 | IAP v5 어댑터, 저장 후 주문 승인, 복원·다국어 가격 UI, 전면 광고 권한 차단, Kotlin 의존성 정렬 | 공식 Unity IAP v5 흐름과 패키지 API를 기준으로 구현했다. 전체 EditMode 29/29, 48,608,517바이트 서명 AAB 구조·Manifest·서명, SM-S938N 설치·기동·상품 가격 조회와 설정 UI 비겹침을 검증했다. 실제 금전 구매·복원은 별도 승인 전 수행하지 않았다. | 클라이언트 구현·기기 조회 검증 완료 |
| 2026-07-22 | Codex + Unity MCP + ADB | 테스트 광고 실기기 완료 검증 | MCP 에디터 준비 상태·테스트 실행, 보상형 광고 완료·보상, 전면 광고 정책·복귀 검증 | SM-S938N의 사설 DNS 자동 모드에서 공식 테스트 광고만 실행했다. 보상형 완료 후 크레딧 `20 → 80 C`, 5개 광석·180초 전면 광고 표시, 게임 복귀, AndroidRuntime 오류 없음과 EditMode 29/29를 확인했다. | 광고 단계 실기기 검증 완료 |
| 2026-07-22 | Codex + Unity MCP + Unity IAP 공식 문서 | IAP 계정 인증 대기 중 복원 안정성·AAB 사전 준비 | Unity IAP 5.4.1 복원 중복 조회 제거, 복원·중복 구매·이벤트 해제 회귀 테스트, 새 서명 AAB | 패키지 변경 기록과 공식 복원 흐름을 대조했다. Unity 스크립트 진단 오류 0건, EditMode 32/32, 48,610,452바이트 AAB 구조·release manifest·출시 키 인증서 일치를 확인했다. 실제 Play 구매·복원은 계정 인증 후로 남겼다. | 로컬 IAP 사전 준비 완료 |

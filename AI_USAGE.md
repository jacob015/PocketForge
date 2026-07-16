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

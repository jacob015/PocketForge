# Pocket Forge 현재 아키텍처

마지막 구현 확인: 2026-07-30

이 문서는 현재 소스·Unity Editor·테스트에서 확인한 구현 사실만 기록한다. 예정 기능은 `PROJECT_PLAN.md`, 현재 범위는 `TASKS.md`를 따른다.

## 프로젝트 기준선

- Unity `6000.5.4f1`, URP `17.5.0`
- Android Portrait
- 시작 씬: `Assets/PocketForge/Scenes/Mine.unity`
- 패키지명: `com.jacob015.pocketforge`
- 앱 버전: `0.1.0 (1)`
- 저장 형식: `GameSaveMigrator.CurrentVersion = 8`
- 검증 기준: Unity 컴파일 오류 0건, EditMode 121/121 통과
- 이번 Task 13-3B 변경은 Android APK/AAB와 실기기에서 아직 검증하지 않았다.

## 계층과 책임

```text
MineGameController (Unity composition root)
  ├─ SaveService → GameSaveMigrator → GameSaveData
  ├─ MiningContentCatalog
  │   ├─ OreDefinition[]
  │   ├─ UpgradeDefinition[]
  │   ├─ ChapterDefinition[]
  │   ├─ FeatureUnlockDefinition[]
  │   └─ ResearchNodeDefinition[]
  ├─ MiningGameService
  │   ├─ MiningPowerService
  │   ├─ MinerProgressionService
  │   ├─ ResearchService
  │   └─ MiningGameState / OreState
  ├─ MineHudPresenter → MineHudView
  │                         └─ CompactNumberFormatter
  ├─ MineAdCoordinator → IAdsService → GoogleMobileAdsService
  ├─ MineIapCoordinator → IIapService → UnityIapService
  ├─ LanguageService
  └─ GameAudioController / GameSettingsService
```

| 구성 요소 | 책임 |
|---|---|
| `MineGameController` | Unity 생명주기, 의존성 조립, 광석 모델 표시, 자동 채굴 프레임 처리 |
| `MiningGameService` | 채굴·보상·강화·오프라인 보상·다음 광석·챕터·보스 관문의 순수 규칙 |
| `MiningPowerService` | 자동 채굴력·탭 피해·능동 채굴력·보스 권장 채굴력 계산과 미래 성장 배율 합성 |
| `MinerProgressionService` | 경험치 곡선·레벨업·1회 보상·기능 해금·광부 등급 배율 계산 |
| `ResearchService` | 설계도 코어 연구의 잠금·비용·선행 조건·최대 레벨·영구 배율 계산 |
| `MiningGameState` / `OreState` | 실행 중 플레이어·광석 상태 |
| `MiningContentCatalog` | 현재 단계의 광석·챕터·강화·연구 노드 정의 선택 |
| `MineHudPresenter` | 서비스 결과를 UI 갱신·피드백·저장 요청으로 연결 |
| `MineHudView` | UGUI 생성, Safe Area 레이아웃, 입력 이벤트, 설정·챕터·연구 모달 |
| `CompactNumberFormatter` | 증가형 수치의 K/M/B/T/Qa/Qi 표시 정책 |
| `SaveService` | `PocketForge.Save.v1` 키에 JSON을 PlayerPrefs로 저장·로드 |
| 광고·IAP Coordinator | 외부 SDK 이벤트를 게임 상태와 UI에 연결 |

게임 규칙은 Unity SDK·광고 SDK·결제 SDK 타입을 직접 참조하지 않는다. 외부 SDK는 어댑터와 Coordinator를 통해 연결해 EditMode에서 규칙을 독립 검증한다.

## 런타임 데이터 흐름

```text
앱 시작
  → SaveService.Load
  → GameSaveMigrator.Normalize(version 8)
  → MiningGameService.CreateInitialState
  → 단계에 맞는 OreDefinition + ChapterDefinition 조회
  → MineGameController가 3D 광석과 HUD 표시

탭 / 자동 채굴
  → MiningPowerService.Calculate
  → 자동 채굴력과 탭 피해 계산
  → MiningGameService.Tick / Mine
  → 체력 감소
  → 파괴 시 Credits와 챕터 기준 XP 지급
  → MinerProgressionService가 레벨업·1회 보상·기능 해금을 계산
  → 보스라면 보스 배율과 최초 클리어 보상 적용
  → 최초/반복 처치 정책에 따라 설계도 코어 지급
  → stage 증가, 다음 OreState 생성
  → Presenter가 HUD·피드백·저장을 갱신

보스 진행
  → MiningGameService.Tick이 자동 피해와 남은 시간을 함께 처리
  → 마지막 시간 경계의 자동 피해로 보스를 처치하면 클리어
  → 실패하면 furthestStage의 보스 도달 기록은 유지
  → current stage를 직전 일반 stage로 옮겨 자동 파밍
  → ChapterStatus 또는 챕터 선택 모달에서 명시적으로 보스 재도전
  → Presenter가 다국어 시간 초과·파밍 피드백 표시

오프라인 진행
  → SaveService.Load의 lastSavedUnixSeconds와 현재 UTC 비교
  → MiningGameService.ClaimOfflineProgress
  → MiningPowerService의 AutoPowerPerSecond 사용
  → furthestStage 직전에서 가장 가까운 일반 stage 선택
  → 정수 개수의 일반 광석만 처리해 Credits·XP 지급
  → stage·furthestStage·챕터 완료·OreState는 변경하지 않음
  → 체크포인트를 현재 UTC로 전진시키고 즉시 저장
  → Presenter가 적용 시간·광석 수·Credits·XP를 네 언어로 표시
```

탭 입력은 `OreState`의 0.2초 쿨다운으로 초당 최대 5회만 유효하다. 보스 실패 후 일반 광석을 반복 파괴해도 이미 도달한 미클리어 보스로 자동 진입하지 않으며, 사용자가 챕터 상태에서 `도전`을 선택해야 다시 진입한다.

## 콘텐츠 데이터

### 광석과 강화

- `OreDefinition`: 시작 단계, 내구도 성장, 희귀 확률, 보상 배율, 표시 색상·스케일·모델
- `UpgradeDefinition`: 강화 타입, 비용 성장, 레벨당 효과
- 등록 광석: Copper, Iron, Gold, Crystal
- 강화 타입: Pickaxe, Drill, Robot
- 현재 런타임 입력은 `MiningContentCatalog.asset`이다. `MiningGameConfig.asset`은 이전 밸런스 자산으로 남아 있다.

### 채굴력 계산

`MiningPowerService`가 진행 판정에 쓰는 파생 능력치를 한 경계에서 계산한다. 파생값은 저장하지 않고 `GameSaveData`와 카탈로그에서 매번 계산한다.

- 드릴 출력: `0.5 + 드릴 레벨 × 드릴 레벨당 효과`
- 로봇 배율: `1 + 로봇 레벨 × 로봇 레벨당 효과`
- 자동 채굴력: `드릴 출력 × 로봇 배율 × 영구 성장 배율 × 일시 버프 배율`
- 기본 탭 피해: `(1 + √(곡괭이 레벨 × 곡괭이 레벨당 효과)) × 성장 배율`
- 최종 탭 피해: 기본 탭 피해와 자동 채굴력의 0.05~0.10초분 중 큰 값
- 능동 채굴력: `자동 채굴력 + 탭 피해 × 기준 초당 5회`
- 보스 권장 채굴력: `보스 내구도 ÷ 제한 시간`

현재 실제 저장 데이터에서는 곡괭이·드릴·로봇 레벨, 광부 등급, 연구가 기여한다. 광부 등급은 Lv.1을 기준으로 레벨당 총 채굴력 `+2%`를 `MinerRankMultiplier`에 전달한다. 연구 노드의 누적 보너스는 `ResearchMultiplier`로 전달되므로 자동·탭·오프라인·보스 비교가 같은 계산 경계를 공유한다. 장비·도감·일시 버프는 아직 `1`이며 각 후속 메타 시스템이 연결될 확장 지점이다. 기본 레벨의 능동 채굴력은 5.5/s이고 현재 챕터 보스 권장치는 각각 약 5.5/s, 14.58/s, 26/s다.

### 광부 경험치와 기능 해금

`MinerProgressionService`는 Unity 표시 계층과 분리된 순수 규칙 서비스다. `MiningGameService`가 접속 중 광석 파괴와 오프라인 광석 처리 결과를 이 서비스에 전달한다.

- 다음 레벨 요구 XP: `20 + 15 × (현재 레벨 - 1)`
- 일반 광석 XP: `챕터 번호 × 1`
- 보스 XP: `챕터 번호 × 10`
- 광부 등급 배율: `1 + (광부 레벨 - 1) × 0.02`
- 자동 레벨업 보상: `새 레벨 × 25 Credits`, 5레벨마다 `1 Gem`
- 해금: Lv.2 장비, Lv.3 박물관, Lv.4 연구, Lv.5 미션, Lv.6 상점, Lv.7 이벤트

레벨업 보상은 `highestRewardedMinerLevel`보다 높은 레벨을 처음 통과할 때만 지급한다. 기능 상태는 별도 불리언을 저장하지 않고 현재 광부 레벨과 `FeatureUnlockDefinition`에서 파생해 콘텐츠 추가와 저장 호환을 단순화한다. 연구는 실제 화면과 규칙이 연결됐고 장비·박물관·미션·상점·이벤트는 아직 Placeholder다.

### 설계도 코어와 영구 연구

보스 처치 시 `ChapterDefinition`의 최초·반복 코어 보상값을 적용한다. 현재 1~3챕터 최초 보상은 3/6/9, 반복 보상은 1/2/3 코어다. 광부 Lv.4부터 광부 등급 버튼이 연구 모달을 열며 다음 세 노드를 제공한다.

- `core_output`: 최대 5레벨, 비용 1부터 레벨마다 +1, 레벨당 총 채굴력 +5%
- `precision_tools`: `core_output` Lv.2 필요, 최대 4레벨, 비용 2부터 +2, 레벨당 +7%
- `deep_automation`: `precision_tools` Lv.2 필요, 최대 3레벨, 비용 4부터 +4, 레벨당 +10%

연구 정의는 `MiningContentCatalog.researchNodes`에 있고 진행도는 노드 ID와 레벨만 저장한다. 구매 서비스는 잠금, 선행 조건, 코어 잔액, 최대 레벨을 순서대로 검사하며 같은 노드 ID의 중복 저장 항목은 마이그레이션에서 가장 높은 레벨 하나로 정규화한다.

### 오프라인 진행

`OfflineProgressResult`는 실제 경과 시간, 보상이 적용된 시간, 파밍 stage, 처리 광석 수, Credits와 `MinerProgressionResult`를 반환한다.

- 최대 적용 시간은 기존 `MiningContentCatalog.MaxOfflineRewardSeconds`의 14,400초(4시간)다.
- 파밍 stage는 `furthestStage - 1`에서 시작해 보스 stage를 건너뛴 가장 가까운 일반 stage다. 신규 저장만 stage 1을 사용한다.
- 처리 광석 수는 `floor(자동 채굴력 × 적용 시간 ÷ 일반 광석 내구도)`다.
- Credits는 `처리 광석 수 × 해당 일반 광석 보상`이며 희귀·보스 배율은 사용하지 않는다.
- XP는 `처리 광석 수 × 파밍 stage의 챕터 번호`이며, 오프라인 시작 시점의 광부 등급 배율로 생산량을 먼저 계산한 뒤 경험치와 레벨업 보상을 한 번 적용한다.
- 이전 챕터를 재도전 중이어도 최장 진행 기준 일반 stage를 파밍한다.
- 오프라인 시간은 현재 stage, 최장 진행, 챕터 완료와 실행 중 광석 체력을 변경하지 않는다.
- 저장 시각이 없으면 보상 없이 현재 시각으로 초기화한다. 이미 처리한 시각과 같거나 더 과거인 시각은 보상을 반환하지 않는다.
- `SaveService.Save`는 저장 시각을 단조 증가시켜 기기 시간이 뒤로 이동해도 체크포인트가 역행하지 않게 한다.
- 앱 시작뿐 아니라 `OnApplicationPause(false)` 복귀도 같은 청구 경로를 사용한다.

### 챕터와 보스

`ChapterDefinition`은 다음 값을 가진다.

- `contentId`, `chapterNumber`, `startStage`, `stagesPerChapter`
- `firstClearCredits`, `firstClearGems`
- `bossDurabilityMultiplier`, `bossRewardMultiplier`
- `bossVisualScaleMultiplier`
- `bossTimeLimitSeconds`

카탈로그는 현재 `stage` 이하에서 가장 큰 `startStage`를 가진 챕터를 선택한다. 각 챕터의 마지막 로컬 스테이지가 보스다.

| 챕터 | 범위 | 최초 보상 | 보스 배율 |
|---|---:|---:|---|
| Crystal Cavern | 1~10 | 100 Credits, 5 Gems | 내구도 ×3, 보상 ×5 |
| Magma Depths | 11~20 | 250 Credits, 10 Gems | 내구도 ×3.5, 보상 ×6 |
| Ancient City | 21~30 | 500 Credits, 15 Gems | 내구도 ×4, 보상 ×7 |

현재 세 챕터는 규칙·저장 확장용 임시 데이터이며 보스 제한 시간은 모두 30초다. 최초 클리어 보상 패널과 동적 챕터 선택 화면은 구현됐지만 챕터 전용 배경과 보스 모델은 아직 없다. 마지막 정의 이후 단계는 가장 최근 챕터 정의를 계속 사용한다.

## 저장 스키마

`GameSaveData` 버전 8:

- 64비트 증가형 재화 `credits`, `gems`, `blueprintCores`
- 확장 가능한 `researchProgress[]` (`nodeId`, `level`)
- `stage`, `furthestStage`
- `highestCompletedChapter`
- `pickaxeLevel`, `drillLevel`, `robotLevel`
- `minerLevel`, `minerExperience`, `highestRewardedMinerLevel`
- `adsRemoved`
- `lastSavedUnixSeconds`

`GameSaveMigrator.Normalize`는 null과 음수 값을 안전한 기본값으로 정규화하고 현재 버전으로 올린다. 기존 JSON 정수 재화는 64비트 필드로 그대로 읽히며 더 이상 약 21억에서 포화되지 않는다. 광부 레벨과 마지막 보상 레벨은 최소 1이며 보상 레벨은 현재 광부 레벨보다 높아질 수 없다. `SaveService`는 기존 `lastSavedUnixSeconds`보다 과거 시각으로 저장하지 않는다. 최초 챕터 보상은 `highestCompletedChapter`보다 큰 챕터를 처음 완료할 때만 지급한다.

## UI·비주얼

- UGUI 런타임 HUD와 설정 모달을 사용한다.
- 증가 가능한 재화·보상·비용·XP·채굴력은 `CompactNumberFormatter`를 거쳐 K/M/B/T/Qa/Qi로 표시한다. 레벨·챕터·스테이지·시간·진행 개수는 식별 의미를 보존하기 위해 축약하지 않는다.
- `CanvasScaler`, Safe Area, 세로 화면비 회귀 테스트로 다양한 Android 화면비를 대응한다.
- HUD·설정 표면은 사용자 결정에 따라 `Image.Type.Simple`을 사용한다.
- `MineUiSkin`이 생성 UI 스프라이트를 캐시하고, `UiFontProvider`가 CJK 대응 폰트를 제공한다.
- `MobileButtonFeedback`, `PositiveFeedbackBurst`, `CasualFeedbackText`가 버튼·긍정 보상 피드백을 담당한다.
- 진행 바 위 `ChapterStatus`는 일반 stage에서 챕터 내 진행도와 자동 채굴력, 보스에서 카운트다운과 현재/권장 채굴력을 네 언어로 표시한다. 보스 실패 후에는 `보스 준비` 상태와 현재/권장치를 표시하며 같은 버튼이 재도전 진입점이 된다.
- 기존 자원 카운터 좌표를 이동하지 않고 헤더 왼쪽의 비어 있던 영역에 `MinerRankButton` 260×94를 배치했다. 현재 광부 레벨·XP를 표시하고 누르면 총 등급 보너스와 다음 기능 해금을 네 언어로 안내한다.
- 기존 `OfflineRewardSurface`는 위치·크기를 유지하며 보상이 적용된 시간, 처리한 일반 광석 수, Credits와 XP를 두 줄로 표시한다.
- 광석은 Meshy 생성 모델을 모바일용 Unity 메시·텍스처로 변환한 자산을 사용한다.
- Task 13-1에서는 기존 모델을 보스일 때 확대할 뿐 전용 보스 그래픽은 아직 사용하지 않는다.

## 광고 경계

```text
MineAdCoordinator
  ├─ IAdsService
  │   └─ GoogleMobileAdsService
  ├─ InterstitialAdPolicy
  ├─ MiningGameService
  └─ MineHudView
```

- 보상은 광고 SDK의 완료 콜백에서만 지급한다.
- 전면 광고 기본 조건은 광석 5개 파괴와 마지막 노출 후 180초 경과다.
- 광고 로드·표시 실패는 채굴 진행을 막지 않는다.
- `adsRemoved`가 참이면 강제 전면 광고만 건너뛰고 선택형 보상 광고는 유지한다.
- 현재 광고 단위는 Google 공식 테스트 ID다. 실기기에서 보상형 완료, 전면 광고 노출과 복귀를 검증했다.
- 운영 광고 ID, UMP 동의, 스토어 데이터 공개는 출시 전 구성 대상이다.

## 인앱 결제 경계

```text
MineIapCoordinator
  ├─ IIapService
  │   └─ UnityIapService
  ├─ GameSaveData.adsRemoved
  └─ MineHudView
```

- Google Play 비소모성 상품 ID는 `remove_ads`다.
- 새 구매는 권한 저장 성공 후에만 Pending 주문을 승인한다.
- 기존 구매 조회로 로컬 권한을 복원하며, 오프라인 조회 실패만으로 권한을 취소하지 않는다.
- Play 내부 테스트에서 무청구 구매, 명시적 복원, 재설치 후 자동 복구를 검증했다.
- 환불·취소 반영과 서버 영수증 검증은 운영 백엔드 범위다.

## 씬과 Inspector

- `Mine.unity`에는 Main Camera, Directional Light, 배경·지면, `MineGameController`가 있다.
- `MineGameController.contentCatalog`은 `Assets/PocketForge/Content/MiningContentCatalog.asset`을 참조한다.
- 광석 정의는 각 모바일 최적화 모델과 표시 설정을 참조한다.
- HUD는 런타임에 구성되며, 설정·현지화·광고·IAP 서비스는 Controller가 조립한다.
- 새 `ChapterDefinition`은 카탈로그 내부 직렬화 데이터라 추가 Inspector 연결이 필요하지 않다.

## 검증 경계

- 2026-07-30 Unity Editor 컴파일 오류 0건
- EditMode 105/105 통과
- 챕터 10번째 스테이지 보스 판정·내구도 검증
- 최초 보스 클리어 보상과 재도전 중복 방지 검증
- 자동 채굴력·탭 피해·능동 채굴력·미래 성장 배율 합성과 세 보스 권장치 검증
- 초당 최대 5회 탭 제한과 마지막 시간 경계 자동 피해의 보스 처치 검증
- 보스 타이머 감소, 시간 초과 시 무보상·직전 일반 stage 파밍·명시적 재도전 검증
- 일반 stage 자동 채굴력, 보스 준비 상태와 현재/권장 채굴력 HUD, 네 언어 피드백 검증
- 저장 버전 7 경험치·광부 레벨·마지막 보상 레벨 정규화와 기존 최장 진행·보스 재도전 호환 검증
- 전체 자동 채굴력 기반 오프라인 생산, 4시간 상한과 정수 광석 보상 검증
- 보스·재도전 상태의 최장 진행 기준 일반 stage 파밍과 진행 상태 불변 검증
- 신규 저장 무보상 초기화, 같은 체크포인트 중복 수령과 시간 역행 차단 검증
- 접속 중 일반·보스 XP, 다중 레벨업, 기능 해금, 1회 보상과 5레벨 Gem 보상 검증
- 광부 등급 배율의 자동·탭·오프라인 생산 반영 검증
- 적용 시간·처리 광석·Credits·XP의 네 언어 복귀 HUD와 기존 치수 유지 검증
- 기존 자원 카운터를 이동하지 않는 광부 레벨·XP 헤더 표시와 Simple 이미지 타입 검증
- 기존 HUD·설정·광고·IAP·현지화 회귀 테스트 통과
- Play Mode에서 실제 저장 상태의 자동 채굴과 일반 stage `ChapterStatus`의 `5.6/s` 표시를 확인했다.
- 저장 데이터를 변경하지 않는 런타임 시나리오로 보스 실패→직전 stage 파밍→명시적 재도전을 확인했고 Console 오류는 0건이다.
- Play Mode 임시 저장에서 보스 stage 10을 유지한 채 stage 9 기준 1시간 결과 `광석 31 / 837 C`를 확인했다.
- 같은 Play 세션에서 2분 백그라운드 복귀를 모사해 `광석 1 / 27 C` 추가와 HUD 갱신을 확인했고 검증 후 기존 PlayerPrefs 원문을 복원했다.
- Android 빌드와 실기기 동작은 미검증

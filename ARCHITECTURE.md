# Pocket Forge 현재 아키텍처

마지막 구현 확인: 2026-08-01

이 문서는 현재 소스·Unity Editor·테스트에서 확인한 구현 사실만 기록한다. 예정 기능은 `PROJECT_PLAN.md`, 현재 범위는 `TASKS.md`를 따른다.

## 프로젝트 기준선

- Unity `6000.5.4f1`, URP `17.5.0`
- Android Portrait
- 시작 씬: `Assets/PocketForge/Scenes/Mine.unity`
- 패키지명: `com.jacob015.pocketforge`
- 앱 버전: `0.1.0 (1)`
- 저장 형식: `GameSaveMigrator.CurrentVersion = 10`
- 검증 기준: Unity 컴파일·Console 오류 0건, EditMode 151/151 통과
- 이번 Task 13-5A 변경은 Android APK/AAB와 실기기에서 아직 검증하지 않았다.

## 계층과 책임

```text
MineGameController (Unity composition root)
  ├─ SaveService → GameSaveMigrator → GameSaveData
  ├─ MiningContentCatalog
  │   ├─ OreDefinition[]
  │   ├─ UpgradeDefinition[]
  │   ├─ ChapterDefinition[]
  │   ├─ FeatureUnlockDefinition[]
  │   ├─ ResearchNodeDefinition[]
  │   ├─ EquipmentDefinition[]
  │   ├─ AchievementDefinition[]
  │   └─ MissionDefinition[]
  ├─ MiningGameService
  │   ├─ MiningPowerService
  │   ├─ MinerProgressionService
  │   ├─ ResearchService
  │   ├─ EquipmentService
  │   ├─ CollectionService
  │   ├─ AchievementService
  │   ├─ MissionService
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
| `EquipmentService` | 장비 인벤토리 정규화, 획득·장착·합성·자동 장착과 장비 배율 계산 |
| `CollectionService` | 광석별 발견·누적 채굴 기록, 도감 정규화와 영구 배율 계산 |
| `AchievementService` | 기존 성장 상태에서 업적 진행도를 파생하고 단계별 보상 수령을 검증 |
| `MissionService` | UTC 일일·주간 기간, 시작 기준선, 시간 역행, 개별·완료 보상과 중복 수령을 검증 |
| `MiningGameState` / `OreState` | 실행 중 플레이어·광석 상태 |
| `MiningContentCatalog` | 현재 단계의 광석·챕터·강화·연구·장비·업적·미션 정의 선택 |
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
  → GameSaveMigrator.Normalize(version 11)
  → MiningGameService.CreateInitialState
  → 단계에 맞는 OreDefinition + ChapterDefinition 조회
  → MineGameController가 3D 광석과 HUD 표시

탭 / 자동 채굴
  → MiningPowerService.Calculate
  → 자동 채굴력과 탭 피해 계산
  → MiningGameService.Tick / Mine
  → 체력 감소
  → 파괴 시 Credits와 챕터 기준 XP 지급
  → 일반·보스 광석의 content ID별 누적 채굴 수 기록
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
  → 계산 시작 시점의 도감 배율로 생산량을 고정한 뒤 처리 광석 수를 도감에 기록
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

현재 실제 저장 데이터에서는 곡괭이·드릴·로봇 레벨, 광부 등급, 연구, 장비, 광물 도감이 기여한다. 광부 등급은 Lv.1을 기준으로 레벨당 총 채굴력 `+2%`를 `MinerRankMultiplier`에 전달한다. 연구 노드의 누적 보너스는 `ResearchMultiplier`, 장착 장비의 합산 보너스는 `EquipmentMultiplier`, 도감 보너스는 `CollectionMultiplier`로 전달되므로 자동·탭·오프라인·보스 비교가 같은 계산 경계를 공유한다. 일시 버프는 아직 `1`인 후속 확장 지점이다. 기본 레벨의 능동 채굴력은 5.5/s이고 현재 챕터 보스 권장치는 각각 약 5.5/s, 14.58/s, 26/s다.

### 광부 경험치와 기능 해금

`MinerProgressionService`는 Unity 표시 계층과 분리된 순수 규칙 서비스다. `MiningGameService`가 접속 중 광석 파괴와 오프라인 광석 처리 결과를 이 서비스에 전달한다.

- 다음 레벨 요구 XP: `20 + 15 × (현재 레벨 - 1)`
- 일반 광석 XP: `챕터 번호 × 1`
- 보스 XP: `챕터 번호 × 10`
- 광부 등급 배율: `1 + (광부 레벨 - 1) × 0.02`
- 자동 레벨업 보상: `새 레벨 × 25 Credits`, 5레벨마다 `1 Gem`
- 해금: Lv.2 장비, Lv.3 박물관, Lv.4 연구, Lv.5 미션, Lv.6 상점, Lv.7 이벤트

레벨업 보상은 `highestRewardedMinerLevel`보다 높은 레벨을 처음 통과할 때만 지급한다. 기능 상태는 별도 불리언을 저장하지 않고 현재 광부 레벨과 `FeatureUnlockDefinition`에서 파생해 콘텐츠 추가와 저장 호환을 단순화한다. 장비·박물관·연구는 실제 화면과 규칙이 연결됐고 미션·상점·이벤트는 아직 Placeholder다.

### 설계도 코어와 영구 연구

보스 처치 시 `ChapterDefinition`의 최초·반복 코어 보상값을 적용한다. 현재 1~3챕터 최초 보상은 3/6/9, 반복 보상은 1/2/3 코어다. 광부 Lv.4부터 광부 등급 버튼이 연구 모달을 열며 다음 세 노드를 제공한다.

- `core_output`: 최대 5레벨, 비용 1부터 레벨마다 +1, 레벨당 총 채굴력 +5%
- `precision_tools`: `core_output` Lv.2 필요, 최대 4레벨, 비용 2부터 +2, 레벨당 +7%
- `deep_automation`: `precision_tools` Lv.2 필요, 최대 3레벨, 비용 4부터 +4, 레벨당 +10%

연구 정의는 `MiningContentCatalog.researchNodes`에 있고 진행도는 노드 ID와 레벨만 저장한다. 구매 서비스는 잠금, 선행 조건, 코어 잔액, 최대 레벨을 순서대로 검사하며 같은 노드 ID의 중복 저장 항목은 마이그레이션에서 가장 높은 레벨 하나로 정규화한다.

### 장비 인벤토리와 합성

`EquipmentDefinition`은 정의 ID, 현지화 키, 슬롯, 기본 채굴력 보너스를 가진다. 현재 런타임 기본 장비는 곡괭이·드릴·로봇·부적 각 1종이며 등급은 Common, Rare, Epic, Legendary 네 단계다. 등급 배율은 기본 보너스의 1/2/4/8배다.

`EquipmentService`는 Unity 표시 계층과 분리해 다음 규칙을 담당한다.

- 인벤토리의 알 수 없는 정의, 빈 ID, 중복 인스턴스 ID와 잘못된 장착 슬롯 정규화
- 광부 Lv.2 해금 이후 장착·해제와 슬롯별 최고 보너스 자동 장착
- 장착하지 않은 동일 정의·동일 등급 3개를 다음 등급 1개로 합성
- 전설 등급, 재료 부족, 존재하지 않는 장비, 잠긴 기능 차단
- 장착 장비 보너스 합계를 `1 + 보너스 합` 형태의 `EquipmentMultiplier`로 계산
- 보스 처치마다 카탈로그 순서로 다음 슬롯 장비 지급, 1~2챕터 Common·3챕터 이후 Rare 지급

장비 획득은 확률 테이블이나 신규 재료 없이 초기 흐름만 제공한다. 미션·상점 획득처, 랜덤 옵션, 프리셋, 세트 효과, 유료 장비는 후속 범위다.

### 광물 박물관과 업적

`CollectionService`는 카탈로그에 존재하는 광석 ID만 보존하고 중복 항목은 가장 큰 누적 채굴 수 하나로 정규화한다. 온라인 광석 파괴와 오프라인 처리 광석을 같은 배열에 기록한다.

- 광석 최초 발견: 총 채굴력 `+1%`
- 광석별 누적 25·100·500개 달성: 단계마다 `+1%`
- 광석 한 종류가 제공하는 현재 최대 보너스: `+4%`
- 오프라인 생산은 청구 시작 시점 배율로 처리량을 계산한 뒤 도감 수를 기록해 한 번의 청구 안에서 자기 증폭하지 않는다.

`AchievementService`는 별도 진행 카운터를 중복 저장하지 않고 채굴 수, 최고 완료 챕터, 시설 레벨 합, 광부 레벨, 연구 레벨 합, 장비 획득 수에서 진행도를 파생한다. 6개 업적은 각각 3단계이며 `achievementClaims[]`에는 업적 ID별 수령 완료 단계만 저장한다. 다음 단계 목표를 충족한 경우에만 Credits·Gem·설계도 코어를 지급하고 완료 단계 초과, 알 수 없는 ID, 잠긴 기능, 중복 수령을 차단한다.

### 일일·주간 미션

`MissionService`는 Unity 표시 계층과 분리된 기간 목표 규칙이다. `MissionDefinition[]`은 기간, 진행 지표, 목표, 기존 보상 타입을 데이터로 보유한다. 채굴·시설·연구·장비는 `MissionPeriodData.baseline`에 저장한 기간 시작 스냅샷과 현재 누적 상태의 차이를 사용하므로 기존 시스템과 진행 이벤트를 중복 집계하지 않는다. 반복 보스 처치는 기존 누적 상태가 없어 `bossesDefeated` 하나만 추가했다.

- 일일 기간: UTC 날짜 키, 다음 UTC 00:00 갱신
- 주간 기간: 월요일 UTC 날짜 키, 다음 월요일 UTC 00:00 갱신
- 시간 역행: `lastObservedMissionUnixSeconds`보다 과거인 기기 시각은 마지막 관찰 시각으로 고정해 이전 기간을 재개하지 않음
- 개별 수령: 기간별 `claimedMissionIds[]`
- 전체 완료 수령: 기간별 `completionRewardClaimed`
- 주간 장비 완료 보상: 새 지급 체계를 만들지 않고 `EquipmentService.GrantBossReward` 재사용

기간 초기화는 광부 Lv.5 해금 상태에서 게임 시작과 실행 중 경계 통과 시 수행한다. 경계가 바뀌면 즉시 저장을 요청하며, 일반 저장과 앱 일시정지 저장이 마지막 관찰 시각을 함께 보존한다. 서버 기준 시각이나 계정 간 동기화는 제공하지 않는다.

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

`GameSaveData` 버전 11:

- 64비트 증가형 재화 `credits`, `gems`, `blueprintCores`
- 확장 가능한 `researchProgress[]` (`nodeId`, `level`)
- 고유 ID 장비 인벤토리 `equipmentInventory[]` (`instanceId`, `definitionId`, `rarity`)
- 슬롯 참조 `equippedEquipment[]` (`slot`, `instanceId`)과 `equipmentRewardSequence`
- 광석별 누적 채굴 `oreCollection[]` (`contentId`, `minedCount`)
- 일일·주간 기간 상태 `dailyMissions`, `weeklyMissions` (`periodKey`, `baseline`, `claimedMissionIds`, `completionRewardClaimed`)
- 로컬 시간 역행 기준 `lastObservedMissionUnixSeconds`와 반복 보스 누적 `bossesDefeated`
- 업적별 수령 단계 `achievementClaims[]` (`achievementId`, `claimedTiers`)
- `stage`, `furthestStage`
- `highestCompletedChapter`
- `pickaxeLevel`, `drillLevel`, `robotLevel`
- `minerLevel`, `minerExperience`, `highestRewardedMinerLevel`
- `adsRemoved`
- `lastSavedUnixSeconds`

`GameSaveMigrator.Normalize`는 null과 음수 값을 안전한 기본값으로 정규화하고 현재 버전으로 올린다. 장비는 빈 ID를 제거하고 같은 인스턴스 ID 중 가장 높은 등급 하나만 남기며, 등급을 0~3으로 제한하고 존재하지 않는 장비를 가리키는 슬롯 참조를 제거한다. 도감과 업적은 빈 ID를 제거하고 같은 ID의 가장 큰 누적 수·수령 단계를 보존하며 음수를 0으로 제한한다. 미션은 null 기준선과 중복·빈 수령 ID를 정규화하고 음수 진행·시각·보스 수를 0 이상으로 제한한다. 카탈로그 정의와의 일치는 각 서비스가 추가 검증한다. 기존 JSON 정수 재화는 64비트 필드로 그대로 읽히며 더 이상 약 21억에서 포화되지 않는다. 광부 레벨과 마지막 보상 레벨은 최소 1이며 보상 레벨은 현재 광부 레벨보다 높아질 수 없다. `SaveService`는 기존 `lastSavedUnixSeconds`보다 과거 시각으로 저장하지 않는다. 최초 챕터 보상은 `highestCompletedChapter`보다 큰 챕터를 처음 완료할 때만 지급한다.

## UI·비주얼

- UGUI 런타임 HUD와 설정 모달을 사용한다.
- 증가 가능한 재화·보상·비용·XP·채굴력은 `CompactNumberFormatter`를 거쳐 K/M/B/T/Qa/Qi로 표시한다. 레벨·챕터·스테이지·시간·진행 개수는 식별 의미를 보존하기 위해 축약하지 않는다.
- `CanvasScaler`, Safe Area, 세로 화면비 회귀 테스트로 다양한 Android 화면비를 대응한다.
- HUD·설정 표면은 사용자 결정에 따라 `Image.Type.Simple`을 사용한다.
- `MineUiSkin`이 생성 UI 스프라이트를 캐시하고, `UiFontProvider`가 CJK 대응 폰트를 제공한다.
- `MobileButtonFeedback`, `PositiveFeedbackBurst`, `CasualFeedbackText`가 버튼·긍정 보상 피드백을 담당한다.
- 진행 바 위 `ChapterStatus`는 일반 stage에서 챕터 내 진행도와 자동 채굴력, 보스에서 카운트다운과 현재/권장 채굴력을 네 언어로 표시한다. 보스 실패 후에는 `보스 준비` 상태와 현재/권장치를 표시하며 같은 버튼이 재도전 진입점이 된다.
- 기존 자원 카운터 좌표를 이동하지 않고 헤더 왼쪽의 비어 있던 영역에 `MinerRankButton` 260×94를 배치했다. 현재 광부 레벨·XP를 표시하고 누르면 총 등급 보너스와 다음 기능 해금을 네 언어로 안내한다.
- `MineHudViewEquipment` 부분 클래스가 V5 하단 장비 탭의 모달을 담당한다. 4개 장착 슬롯, 페이지당 6개 인벤토리 행, 현재 장비 대비 보너스, 장착·해제·3개 합성·자동 장착을 제공하며 기존 설정 모달의 Simple 스킨 자산을 재사용한다.
- `MineHudViewCollection` 부분 클래스가 V5 하단 박물관 탭의 모달을 담당한다. 박물관에는 광석별 발견·누적 수·보너스를, 업적 탭에는 6개 목표의 진행·다음 보상·수령 상태를 표시하며 기존 Simple 모달 스킨을 재사용한다.
- `MineHudViewMissions` 부분 클래스가 V5 미션 진입점의 일일·주간 탭, 4개 공용 행, 갱신 시간, 전체 완료 보상을 담당한다. 기존 Task13 표면·업적 아이콘·재화 아이콘을 재사용하며 전용 배너·미션 아이콘·완료 상자는 후속 교체 대상으로 분리한다.
- `MineHudViewCommerce` 부분 클래스가 광부 Lv.6 상점과 Lv.7 주간 이벤트 탭을 담당한다. 상점 6행과 이벤트 4행은 아이콘·유동 텍스트·수치·고정 행동 열을 공유하고, 기존 Task13 표면과 버튼을 재사용한다.
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
- 상점 광고 상품은 표시 전에 `ShopService`의 UTC 일일 잔여 횟수를 검사하고 완료 콜백에서만 상품 보상을 지급한다.
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
  ├─ GameSaveData.adsRemoved / starterPackPurchased
  ├─ ShopService
  └─ MineHudView
```

- Google Play 비소모성 상품 ID는 `remove_ads`다.
- 신규 비소모성 상품 ID는 `starter_pack`이며 Credits·Gem·설계도 코어의 1회 지급을 포함한다.
- 새 구매는 권한 저장 성공 후에만 Pending 주문을 승인한다.
- 스타터 패키지는 지급 전 세 재화 스냅샷을 보관하고 저장 실패 시 전부 롤백한다.
- 기존 구매 조회로 로컬 권한을 복원하며, 오프라인 조회 실패만으로 권한을 취소하지 않는다.
- Play 내부 테스트에서 무청구 구매, 명시적 복원, 재설치 후 자동 복구를 검증했다.
- 환불·취소 반영과 서버 영수증 검증은 운영 백엔드 범위다.
- `starter_pack`은 Play Console 상품 생성 전에는 조회 결과가 비활성으로 표시되며 실제 구매·복원은 내부 테스트에서 별도 검증한다.

## 상점·기간 이벤트 경계

```text
MiningContentCatalog
  ├─ ShopProductDefinition[]
  └─ MiningEventDefinition[]
          ↓
ShopService / MiningEventService
          ↓
MiningGameService → MineHudPresenter → MineHudViewCommerce
          ↓
GameSaveData v12
```

- `ShopService`는 무료·광고·Gem·IAP 상품 상태와 UTC 일일 갱신, 기기 시간 역행 방어를 계산한다.
- `MiningEventService`는 주간 월요일 UTC 경계, 기간 시작 광석 기준선, 누적 토큰, 소비 잔액, 보상·교환 중복 방어를 담당한다.
- 이벤트 누적 진척과 소비 잔액은 서로 다른 값이다. 교환 소비가 이미 달성한 누적 보상 단계를 되돌리지 않는다.
- 서비스는 Unity UI와 SDK를 참조하지 않고 `GameSaveData`와 정의 데이터만 변경한다. 광고·IAP의 비동기 완료 경계는 각 Coordinator가 담당한다.
- 저장 v12는 일일 상품·광고 횟수, 스타터 패키지 권한, 이벤트 기준선·토큰·보상·교환 상태를 정규화한다.
- 현재 시간 정책은 서버 권위가 아닌 로컬 역행 방어다. 계정 보안, 여러 기기 동기화, 이벤트 운영 종료 처리는 별도 백엔드 범위다.

## 밸런스 시뮬레이션 경계

```text
MiningContentCatalog
        ↓
ProgressionBalanceSimulator
        ↓ 실제 규칙 재생
MiningGameService → MiningPowerService / Progression / Collection
        ↓
ProgressionBalanceResult
  ├─ 첫 강화·첫 보스·챕터 완료 시간
  ├─ 채굴 수·보스 실패·강화 구매 수
  └─ 최종 시설 레벨·현재/권장 채굴력
```

- `ProgressionBalanceSimulator`는 별도의 축약 공식을 복제하지 않고 실제 채굴·강화·경험치·도감·보스 실패/재도전 서비스를 결정론적으로 재생한다.
- 희귀 판정은 항상 실패하는 값으로 고정해 운이 나쁠 때도 목표 시간 안에 들어오는 보수적 기준선을 만든다.
- 능동 모드는 기준 탭 속도, 자동 모드는 자동 채굴력만 사용하며, 보스 실패 뒤에는 권장 채굴력에 도달할 때까지 마지막 일반 스테이지를 반복한다.
- UI나 저장소에 연결되지 않는 분석 경계이므로 런타임 진행을 변경하지 않고 콘텐츠 자산 조정의 회귀 테스트로 사용한다.

## 씬과 Inspector

- `Mine.unity`에는 Main Camera, Directional Light, 배경·지면, `MineGameController`가 있다.
- `MineGameController.contentCatalog`은 `Assets/PocketForge/Content/MiningContentCatalog.asset`을 참조한다.
- 광석 정의는 각 모바일 최적화 모델과 표시 설정을 참조한다.
- HUD는 런타임에 구성되며, 설정·현지화·광고·IAP 서비스는 Controller가 조립한다.
- 새 `ChapterDefinition`은 카탈로그 내부 직렬화 데이터라 추가 Inspector 연결이 필요하지 않다.

## 검증 경계

- 2026-08-03 Unity Editor 컴파일 오류 0건
- Task 14-1 결정론적 능동·자동 진행 기준과 전체 EditMode 179/179 통과
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
- 저장 버전 10의 도감·업적 중복/음수/알 수 없는 ID 정규화 검증
- 온라인·오프라인 광석 집계, 도감 단계 배율과 오프라인 자기 증폭 방지 검증
- 업적 6종의 진행 파생, 3단계 보상, 잠금·미달·완료·중복 수령 방어 검증
- 4개 언어 박물관·업적 문구와 V5 실제 내비게이션·수령 경로 검증
- 일일·주간 기간 경계, 기기 시간 역행, 저장 v11, 개별·완료 보상 중복 방어와 4개 언어 미션 문구 검증
- 일일 상점 갱신·광고 횟수·Gem 부족·중복 수령, 저장 v12 정규화와 기기 시간 역행 검증
- 주간 이벤트 기준선·누적 보상·토큰 교환·기간 갱신·중복 수령 검증
- `remove_ads`와 `starter_pack`의 상품별 Pending 주문, 저장 성공 후 승인, 저장 실패 롤백 검증
- 1080×1920 세로 상점 6행의 아이콘·텍스트·수치·행동 열 비겹침과 이벤트 4행 탭 전환 검증
- 1080×1920 Play Mode 세로 화면에서 미션 4행의 아이콘·유동 텍스트·보상·고정 수령 열과 하단 완료 보상 간격 검증
- Device Simulator 1440×3088 동일 비율 세로 화면에서 광석 4행과 업적 6행의 모달 경계 검증
- Play Mode에서 실제 저장 상태의 자동 채굴과 일반 stage `ChapterStatus`의 `5.6/s` 표시를 확인했다.
- 저장 데이터를 변경하지 않는 런타임 시나리오로 보스 실패→직전 stage 파밍→명시적 재도전을 확인했고 Console 오류는 0건이다.
- Play Mode 임시 저장에서 보스 stage 10을 유지한 채 stage 9 기준 1시간 결과 `광석 31 / 837 C`를 확인했다.
- 같은 Play 세션에서 2분 백그라운드 복귀를 모사해 `광석 1 / 27 C` 추가와 HUD 갱신을 확인했고 검증 후 기존 PlayerPrefs 원문을 복원했다.
- 서명 Android Release AAB `PocketForge-0.1.0.aab` 빌드 성공: `com.jacob015.pocketforge`, versionCode 1, versionName 0.1.0, 57,822,260 bytes
- `bundletool validate`와 JAR 서명 검증을 통과했고 AAB 서명 지문은 기존 키스토어 복구 기록과 일치한다.
- 사용하지 않는 Engine Diagnostics와 Unity Localization을 제거해 두 Android 빌드 오류를 해소했다. Google Mobile Ads 패키지의 iOS `.xcframework` 비호환 로그 1건은 성공한 Android 산출물과 무관한 비치명 패키지 경고로 남아 있다.
- 신규 `starter_pack` 실제 구매·복원과 실기기 이벤트 입력은 미검증

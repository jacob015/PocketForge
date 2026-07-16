# Pocket Forge 현재 아키텍처

이 문서는 `2026-07-16`에 Unity Editor와 프로젝트 소스에서 확인한 구현 사실을 기록한다. 향후 계획이나 추정은 `PROJECT_PLAN.md`에 기록한다.

## 프로젝트 기준선

- Unity `6000.5.4f1`, URP `17.5.0`
- Build Settings 시작 씬: `Assets/PocketForge/Scenes/Mine.unity`
- 화면: Portrait 고정
- 테스트: EditMode `MiningBalanceTests` 3개 통과

## 폴더와 책임

| 경로 | 책임 |
|---|---|
| `Assets/PocketForge/Scenes/Mine.unity` | 현재 단일 플레이 씬. 카메라, Directional Light, Ground, `MineGameController`를 포함한다. |
| `Assets/PocketForge/Materials/MineOre.mat` | 런타임 광석에 명시적으로 할당하는 URP Lit Material이다. Android 빌드에서 Default-Material 누락으로 인한 분홍색 표시를 방지한다. |
| `Assets/PocketForge/Scripts/Mining/MineGameController.cs` | 게임 루프 조정, 광석 표시 생성, 자동 채굴, IMGUI 표시와 입력, 강화 요청을 처리한다. |
| `Assets/PocketForge/Scripts/Economy/MiningBalance.cs` | 강화 비용·채굴력·광석 내구도·보상 계산을 제공하는 순수 계산 모듈이다. |
| `Assets/PocketForge/Scripts/Save/GameSaveData.cs` | 저장 데이터의 직렬화 가능한 필드를 정의한다. |
| `Assets/PocketForge/Scripts/Save/SaveService.cs` | `PlayerPrefs`와 JSON을 사용해 저장 데이터를 읽고 쓴다. |
| `Assets/PocketForge/Tests/Editor/MiningBalanceTests.cs` | 경제 계산의 기본 증가 규칙을 검증한다. |

`Assets/Scripts/FirstSetting.cs`은 현재 빈 클래스이며 PocketForge 런타임 흐름에서 참조되지 않는 것으로 소스 기준 확인됐다.

## 런타임 흐름

```text
MineGameController.Awake
  → SaveService.Load
  → SpawnOre (현재 stage 기반 내구도·희귀 여부 결정)
  → `MineOre.mat`을 할당한 임시 Sphere 광석 생성

매 프레임
  → Drill 레벨의 자동 채굴력 계산
  → 광석 체력 차감 및 표시 회전·펄스

MINE 버튼 / 자동 채굴
  → DamageOre
  → 광석 파괴 시 보상 지급, stage 증가, 저장, 다음 광석 생성

강화 버튼
  → MiningBalance.GetUpgradeCost
  → 충분한 Credits일 때 레벨 증가 및 저장
```

## 데이터와 경계

`GameSaveData`는 `version`, `credits`, `stage`, `pickaxeLevel`, `drillLevel`, `robotLevel`을 저장한다. 저장 키는 `PocketForge.Save.v1`이다.

경제 계산은 `MiningBalance`에 분리되어 테스트 가능하지만, 화면 표시·입력·게임 상태 전환은 현재 `MineGameController`에 함께 있다. UI는 UGUI가 아니라 `OnGUI` 기반 임시 UI이며, 광석도 런타임 Primitive로 생성된다. `CreatePrimitive`가 Android 코드 스트리핑에서 실패하지 않도록 `MineGameController`는 필요한 primitive 컴포넌트 타입을 private 속성으로 참조하고, 광석 Material은 씬 직렬화 참조로 포함한다.

## 확인된 차이와 다음 설계 과제

- 기획의 UGUI·ScriptableObject 기반 데이터는 아직 도입되지 않았다.
- 현재는 단일 씬 프로토타입이며, 저장 데이터는 `GameSaveMigrator`를 통해 버전 2로 정규화된다. 단계별 과거 버전 변환은 다음 저장 형식 변경 시 추가한다.
- 광석 프리팹, 자원 풀, 스테이지 구성, 자동 채굴 연출, 기기 성능 측정은 구현·검증되지 않았다.
- 개발용 Android APK는 SM-S938N에서 설치·기동·화면 표시까지 검증했다. 배포용 Android 애플리케이션 식별자는 여전히 기본 템플릿 값이므로 출시 전 소유 도메인 기준으로 결정해야 한다.
## Task 3 이후 구조

```text
MineGameController (Unity composition root)
  ├─ SaveService.Load → GameSaveMigrator.Normalize → GameSaveData
  ├─ MiningGameConfig.asset (ScriptableObject)
  ├─ MiningGameService (순수 게임 규칙)
  │    └─ MiningGameState / OreState (런타임 상태)
  └─ MineHudPresenter
       └─ MineHudView (UGUI 표시와 버튼 이벤트)
```

- `MineGameController`는 Unity 생명주기, 광석 시각화, 저장 요청 연결만 담당한다.
- `MiningGameService`는 채굴 피해, 자동 채굴, 보상, 강화 구매와 다음 광석 생성을 처리한다. EditMode 테스트에서 Unity 씬 없이 검증할 수 있다.
- `MiningGameConfig.asset`은 광석 내구도·희귀 확률·보상·강화 비용의 단일 조정 지점이다. 후속 광석/강화 정의 에셋으로 확장할 수 있다.
- `MineHudView`는 UGUI 요소 생성·렌더링·입력 전달만 담당하고, `MineHudPresenter`가 서비스 결과를 뷰와 저장 요청으로 연결한다.
- 저장 데이터는 `GameSaveMigrator.CurrentVersion`을 통해 로드 시 정규화된다. 다음 저장 형식 변경은 이 진입점에 단계별 변환을 추가한다.

## 비주얼 리소스

- `Assets/PocketForge/Art/Generated/ForgeOre.png`: 생성 후 크로마키를 제거한 투명 광석 아트다.
- `Assets/PocketForge/Materials/ForgeOreBillboard.mat`: URP Unlit 머티리얼로 광석 아트를 표시한다.
- `MineGameController`는 기존 Sphere를 유지하면서 전면 Quad에 광석 아트를 배치한다. 기존 Android primitive 스트리핑 대응은 유지한다.
- `Assets/PocketForge/Art/Generated/UpgradeIcons.png`은 Pickaxe·Drill·Robot 순서의 가로 아이콘 시트이며, `MineHudView`가 각 1/3 영역을 UGUI `RawImage`로 표시한다.
## Task 6 콘텐츠 카탈로그

```text
MineGameController
  └─ MiningContentCatalog.asset
      ├─ CopperOre.asset (stage 1+)
      ├─ CrystalOre.asset (stage 10+)
      ├─ PickaxeUpgrade.asset
      ├─ DrillUpgrade.asset
      └─ RobotUpgrade.asset
            ↓
      MiningGameService
```

- `OreDefinition`은 시작 단계, 내구도 증가, 희귀 확률, 보상 배율과 시각 색상을 소유한다.
- `UpgradeDefinition`은 타입별 비용 성장과 레벨당 효과를 소유한다.
- `MiningContentCatalog`은 현재 단계에 가장 가까운 광석 정의와 타입별 강화 정의를 선택한다. 새 콘텐츠는 정의 자산을 만들고 카탈로그에 등록하는 방식으로 확장한다.
- 기존 `MiningGameConfig`은 이전 밸런스 자산으로 남아 있으나 현재 게임 루프의 런타임 입력은 `MiningContentCatalog`이다.

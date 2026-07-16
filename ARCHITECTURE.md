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
  → 임시 Sphere 광석 생성

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

경제 계산은 `MiningBalance`에 분리되어 테스트 가능하지만, 화면 표시·입력·게임 상태 전환은 현재 `MineGameController`에 함께 있다. UI는 UGUI가 아니라 `OnGUI` 기반 임시 UI이며, 광석도 런타임 Primitive로 생성된다.

## 확인된 차이와 다음 설계 과제

- 기획의 UGUI·ScriptableObject 기반 데이터는 아직 도입되지 않았다.
- 현재는 단일 씬·단일 컨트롤러 프로토타입이며, 저장 데이터의 버전 마이그레이션·오류 복구는 없다.
- 광석 프리팹, 자원 풀, 스테이지 구성, 자동 채굴 연출, 기기 성능 측정은 구현·검증되지 않았다.
- 배포용 Android 애플리케이션 식별자와 Android 빌드는 아직 확정·검증되지 않았다.

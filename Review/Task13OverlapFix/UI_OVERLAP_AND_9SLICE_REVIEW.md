# Pocket Forge Task 13 UI Slice 재구축 검수

검수일: 2026-08-02

## 교정 결과

- 모든 런타임 `Image.Type.Sliced` 자산은 호출부 숫자가 아니라 `MineUiSkin`의 단일 측정 카탈로그를 사용한다.
- Border는 알파 160 기준으로 둥근 모서리가 끝나고 직선이 시작되는 지점에 2~3px 안전 여백을 더해 결정했다.
- 장비 인벤토리 카드의 베이스·희귀도·선택 테두리는 같은 RectTransform, 같은 Sliced 렌더 규칙을 사용한다.
- 장비 카드에 구워져 있던 내부 Divider를 제거한 클린 베이스를 새로 만들었다.
- 장비 비교와 연구 행은 세로 모달을 눌러 쓰지 않고, 내부 장식이 없는 전용 가로 패널을 사용한다.
- 박물관 카드의 구워진 하단 패널·게이지를 제거했다. 발견 항목에는 런타임 게이지만 하나 표시하고 미발견 항목은 게이지를 숨긴다.
- 박물관 요약과 다음 보상 영역의 내용 없는 장식 슬롯도 제거하고 단일 가로 패널로 정리했다.
- 작은 런타임 버튼은 원본 곡선 위치를 유지한 채 축소 정규화하여 `border 합 + 16px center`가 실제 최소 Rect 안에 들어오도록 했다.
- Settings/Final/V5 UI 경로에는 Task 13 Sliced 호출이 없으며, 기존 요청대로 고정 비율 Simple 또는 값 기반 Filled만 유지했다.

## 실제 렌더 검수

- [장비 재구축](./Equipment_Rebuilt_Portrait.png)
- [연구 재구축](./Research_Rebuilt_Portrait.png)
- [박물관 재구축](./Museum_Rebuilt_Portrait.png)
- [업적 재구축](./Achievements_Rebuilt_Portrait.png)

캡처 과정에서만 Canvas를 Screen Space Camera로 전환해 MCP 카메라가 Overlay UI를 포함하게 했다. 이 변경은 Play Mode 종료와 함께 폐기했으며 씬에는 저장하지 않았다.

## 측정된 Sliced 카탈로그

Border 순서는 `left, bottom, right, top`이다.

| 런타임 자산 | 원본 px | Border px | 최소 사용 Rect |
|---|---:|---:|---:|
| UiCollectionModalBody | 459×1024 | 39,39,39,39 | 920×1200 |
| UiEquipmentModalBody | 485×1024 | 40,42,40,42 | 920×1200 |
| UiEquipmentSlotCardBase | 302×512 | 38,38,38,38 | 186×216 |
| UiEquipmentInventoryCardClean | 290×512 | 33,33,33,33 | 190×328 |
| OverlayEquipmentRarityCommon/Rare/Epic/Legendary | 268×512 | 33,34,33,34 | 190×328 |
| OverlayEquipmentSelected | 268×512 | 33,34,33,34 | 190×328 |
| ButtonEquipmentUnequipRuntime | 320×137 | 27,27,27,27 | 112×72 |
| ButtonEquipmentEquipRuntime | 384×173 | 32,31,32,31 | 230×100 |
| ButtonEquipmentMergeRuntime | 384×194 | 41,42,41,42 | 230×104 |
| ButtonEquipmentAutoEquipRuntime | 256×218 | 38,38,38,38 | 230×104 |
| ButtonAchievementClaimRuntime | 280×173 | 27,27,27,27 | 156×72 |
| TabCollectionActive | 512×116 | 29,30,29,30 | 400×92 |
| TabCollectionInactive | 512×173 | 30,31,30,31 | 400×92 |
| UiMuseumExhibitCardClean | 288×512 | 34,34,34,34 | 388×330 |
| UiAchievementInProgressState | 512×157 | 27,27,27,27 | 156×84 |
| UiTask13HorizontalPanelClean | 1024×172 | 35,35,35,35 | 824×120 |

`Task13UiSliceCatalogTests`가 위 모든 자산의 Resources 존재 여부, 카탈로그 Border, 최소 Rect 적합성을 자동 검사한다.

## 전체 Task 13 자산 렌더 분류

| 분류 | 자산 |
|---|---|
| Sliced 표면 | 위 측정 카탈로그 19종 |
| Filled 진행도 | UiMuseumProgressFill, UiAchievementProgressFill |
| Simple 진행 트랙 | UiMuseumProgressTrack, UiAchievementProgressTrack |
| Simple 배지 | BadgeAchievementComplete, BadgeEquipmentCountBlank, BadgeEquipmentEquipped, BadgeEquipmentMergeReady, BadgeMuseumBonusBlank, BadgeNotificationBlank |
| Simple 아이콘 | IconAchievementChapter, IconAchievementEquipment, IconAchievementFacility, IconAchievementMiner, IconAchievementMining, IconAchievementResearch, IconAchievementTab, IconAutoEquipRefresh, IconCloseX, IconComparisonDown, IconComparisonUp, IconEquipmentCharm, IconEquipmentDrill, IconEquipmentPickaxe, IconEquipmentRobot, IconMilestoneDiamondActive, IconMilestoneDiamondInactive, IconMuseumCopperOre, IconMuseumGoldOre, IconMuseumIronOre, IconMuseumMysteryMineral, IconMuseumTab, IconMuseumUnknownCrystal |
| Simple 고정 합성 | DividerEquipmentInventory, UiAchievementIconFrame, UiAchievementRewardSlot, UiAchievementRowBase, UiAchievementSummaryCard, UiCollectionTitlePlaque, UiEquipmentCapacityCapsule, UiEquipmentPowerSummary, UiEquipmentTitlePlaque, UiModalCloseButtonSurface, UiMuseumCollectionMilestoneTray, UiMuseumPedestal |
| 대체되어 런타임 미사용 | ButtonAchievementClaim, ButtonEquipmentAutoEquip, ButtonEquipmentEquip, ButtonEquipmentMerge, ButtonEquipmentUnequip, UiEquipmentComparisonTray, UiEquipmentInventoryCardBase, UiMuseumExhibitCardBase, UiMuseumNextRewardStrip, UiMuseumSummaryCard |

Simple 분류 자산은 내부 아이콘·구분선·텍스트 슬롯이 의미를 가지며 현재 Rect가 원본 종횡비와 일치하는 고정 합성물이다. 반복적으로 크기가 바뀌는 표면만 Sliced로 제한했다.

## 생성·후처리 자산

| 자산 | 변경 내용 |
|---|---|
| UiEquipmentInventoryCardClean | 원본 카드의 내부 Divider 제거, 투명 외곽 복원, 290×512 정규화 |
| UiMuseumExhibitCardClean | 구워진 하단 패널·게이지 제거, 투명 외곽 복원, 288×512 정규화 |
| UiTask13HorizontalPanelClean | 내부 Divider 없는 가로 표면 생성, 투명 외곽 복원, 1024×172 정규화 |
| Button*Runtime 5종 | 원본 장식은 유지하고 최소 런타임 Rect에 곡선 Border가 들어오도록 해상도 정규화 |

원본 PNG는 덮어쓰지 않았다. 생성 결과의 검정 외곽은 가장자리 연결 영역만 flood-fill 방식으로 투명화해 어두운 내부 표면을 보존했다.

## 검증 상태

- Unity 컴파일 오류: 0건
- Slice 카탈로그 EditMode: 1/1 통과
- 장비·연구·박물관·업적 Play Mode 실제 렌더: 완료
- UI·화면비·4개 언어·Slice 카탈로그 대상 EditMode: 31/31 통과
- 전체 EditMode 회귀: 155/155 통과
- Android APK/AAB·실기기: 이번 UI 재구축 범위에서는 실행하지 않음

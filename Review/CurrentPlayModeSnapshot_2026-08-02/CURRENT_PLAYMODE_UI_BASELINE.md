# Pocket Forge current Play Mode UI baseline

> Correction: the first user-facing report summarized only major regions and was not an exhaustive change list. The authoritative records are now `CURRENT_PLAYMODE_SAFEAREA_FULL_MANIFEST.tsv` and `CURRENT_PLAYMODE_DIFF_VS_CODE_DEFAULT.tsv`.
>
> - Full current SafeArea inventory: 125 RectTransforms, one row per object
> - Structural comparison against a fresh `MineHudView.Create()` build: 55 moved/resized/reordered paths, 74 geometry or hierarchy property differences
> - Deleted paths: 11
> - Total structurally affected paths: 66
> - The comparison file also records active and visual component differences. Visual-property differences must be interpreted with the final-skin application order in mind; the geometry, hierarchy, deletion list, and full current manifest are the reconstruction authorities.

- Captured: 2026-08-02 18:26:51 +09:00
- Unity: 6000.5.4f1, Android target
- Scene: `Assets/PocketForge/Scenes/Mine.unity`
- State: Play Mode running, not paused
- Canvas: `MineHud`, Screen Space Overlay
- Canvas Scaler: Scale With Screen Size, reference 1080 x 1920, match 0
- Runtime render: 1440 x 3088
- Runtime SafeArea root rect: 1080 x 2316
- Full screenshot: `CurrentPlayMode_UI_1440x3088.png`
- Main SafeArea inventory: 125 RectTransforms; 83 active in hierarchy; 42 inactive by `activeSelf`

This document is the immutable reconstruction baseline for the user's current Play Mode edits. Runtime values were read without stopping or restarting Play Mode. No UI or source code was changed while capturing it.

## Direct SafeArea geometry

All values are current runtime `RectTransform` values in the 1080-wide Canvas coordinate space. Scale is `(1,1,1)` unless noted.

| Sibling | Object | Active | Anchor | Pivot | Anchored position | Size |
|---:|---|---|---|---|---|---|
| 0 | TopSurface | yes | top-center | 0.5,0.5 | 0,-189 | 972,162 |
| 1 | BottomNavigationBar | yes | bottom-center | 0.5,0.5 | 0,120 | 948,160 |
| 2 | UpgradeSurface | yes | 0.5,0.16 | 0.5,0.5 | 0,0 | 968,470 |
| 3 | ActionSurface | yes | 0.5,0.33 | 0.5,0.5 | 0,0 | 560,220 |
| 4 | MinerRankButton | yes | top-center | 0.5,0.5 | -231.9,-185 | 184,104 |
| 5 | CreditsCurrencyIcon | yes | top-center | 0.5,0.5 | -124,-192 | 48,48 |
| 6 | Credits | yes | top-center | 0.5,0.5 | -55,-189 | 90,60 |
| 7 | DepthCurrencyIcon | yes | top-center | 0.5,0.5 | 17.5,-192 | 48,48 |
| 8 | Depth | yes | top-center | 0.5,0.5 | 83,-189 | 76,60 |
| 9 | SettingsButton | yes | top-center | 0.5,0.5 | 403,-189 | 84,84 |
| 10 | OfflineRewardSurface | yes | bottom-center | 0.5,0.5 | -363,843 | 246,186 |
| 11 | RewardedAdButton | yes | top-center | 0.5,0.5 | 337,-192 | 48,48 |
| 12 | ActionFeedbackSurface | no | 0.5,0.51 | 0.5,0.5 | 0,0 | 540,88 |
| 13 | ActionFeedback | no | 0.5,0.51 | 0.5,0.5 | 0,0 | 680,106 |
| 14 | ProgressBackground | yes | bottom-center | 0.5,0.5 | 0,1014 | 560,86 |
| 15 | MineButton | yes | bottom-center | 0.5,0.5 | 0,843 | 450,186 |
| 16 | PickaxeButton | yes | bottom-center | 0.5,0.5 | -327,420 | 302,330 |
| 17 | DrillButton | yes | bottom-center | 0.5,0.5 | 0,420 | 302,330 |
| 18 | RobotButton | yes | bottom-center | 0.5,0.5 | 327,420 | 302,330 |
| 19 | PortraitFrame | yes | top-center | 0.5,0.5 | -379.5,-191.5 | 144,144 |
| 20 | BlueprintCoreIcon | yes | top-center | 0.5,0.5 | 156,-189 | 48,48 |
| 21 | BlueprintCoreValue | yes | top-center | 0.5,0.5 | 220.9,-189 | 76,60 |
| 22 | ChapterInformationPanel | yes | top-center | 0.5,0.5 | -168,-394 | 636,188 |
| 23 | PowerComparisonPanel | yes | top-center | 0.5,0.5 | 333,-394 | 306,188 |
| 24 | BossWarningPanel | yes | top-center | 0.5,0.5 | 0,-560 | 300,96 |
| 25 | ResearchShortcut | yes | bottom-center | 0.5,0.5 | 363,843 | 246,186 |
| 26 | BossChallengeButton | yes | bottom-center | 0.5,0.5 | 0,650 | 322,90 |
| 27-34 | SuccessSpark1-8 | no | center | 0.5,0.5 | existing circular burst coordinates preserved | 13 or 20 square |

## User-deleted objects to keep deleted

These objects are created by the current source but are absent from the live hierarchy. Reconstruction must not recreate them.

- `MineButton/MineActionText`
- `BottomNavigationBar/SelectedNavigationTab`
- `EquipmentNavigation/feature_equipment` runtime navigation text
- `ResearchNavigation/feature_research` runtime navigation text
- `HomeNavigation/home` runtime navigation text
- `MuseumNavigation/feature_museum` runtime navigation text
- `MissionsNavigation/feature_missions` runtime navigation text
- `ShopNavigation/feature_shop` runtime navigation text

The six removed runtime navigation texts remain inside the `navigationLabels` list as destroyed references. `UpdateNavigationLabels()` currently touches them at `MineHudViewV5.cs:717`, producing a repeated `MissingReferenceException`. The implementation fix must remove the obsolete creation/update path rather than recreate the deleted labels.

## Deliberately hidden or state-hidden objects to preserve

- All base button `Label` children under navigation, MinerRank, Settings, Mine, Research, and Boss Challenge remain inactive.
- `SettingsButton/SettingsIcon` remains inactive; the button's own generated surface carries the gear art.
- `RewardedAdButton/VideoIcon` and its base `Label` remain inactive; `PlusIcon` remains active at 46 x 46.
- `ProgressBackground/ChapterStatus` remains inactive.
- All nine upgrade `LevelPip` objects remain inactive.
- Drill and Robot recommendation badges are inactive; Pickaxe recommendation badge is active.
- `ResearchNavigation/ResearchNotificationBadge` is inactive.
- All eight pooled success sparks are inactive at rest.
- All modal backdrops are inactive: Settings, ChapterComplete, ChapterSelection, Research, Equipment, Collection.

## Mission placement contract

`MissionsNavigation` is intentionally retained as a child of `BottomNavigationBar`, but the user moved it out of the bar to sit below the power panel.

- RectTransform: anchor/pivot center; anchored position `455,1529`; size `144,144`
- Icon: anchored position `0,20`; size `93.6,93.6`; Simple image with preserved aspect
- Label: inactive
- LockedIcon: inactive
- Do not return it to the six-column bottom navigation row.
- During the code reconstruction, keep its visual center under `PowerComparisonPanel`; only make a small proportional size adjustment if needed to avoid collision.

The remaining bottom-navigation slots are intentionally five visible cells:

| Slot | Position | Size |
|---|---:|---:|
| EquipmentNavigation | -296,-28 | 144,144 |
| ResearchNavigation | -162,-28 | 144,144 |
| HomeNavigation | 0,-13 | 144,144 |
| MuseumNavigation | 162,-32 | 144,144 |
| ShopNavigation | 291,-28 | 144,144 |

Their visible icons remain 72 x 72 at local position `0,20`.

## Progress bar contract

- `ProgressBackground`: position `0,1014`, size `560,86`, Simple
- `ProgressTrack`: local position `0,0`, size `520,46`, Simple, sprite null/transparent
- `ProgressFill`: left anchor/pivot, local position `16,0`, captured base size `488,53`, Filled horizontal from left
- `OreHealthLabel`: position `0,64`, size `270,38`, font size 24
- `OreHealthValue`: position `0,0`, size `430,40`, font size 24
- Dynamic `fillAmount` and health text are gameplay state and were not frozen.
- Applied exception: `ProgressFill` base width was reduced from the captured `488` to `468` without moving its left origin.

## Top HUD and information details

- PortraitFrame: `-379.5,-191.5`, 144 x 144; MinerPortrait 108 x 108 centered.
- MinerRankButton: `-231.9,-185`, 184 x 104.
  - MinerRank text: `-146.3,-86.7`, 142 x 42, font 24.
  - MinerExperience text: `0,0`, 140 x 32, font 18.
  - ExperienceTrack: `0,23`, 140 x 18.
  - ExperienceProgress: left origin `7,0`, runtime width is dynamic.
  - MinerExperienceIcon: `-60.2,-4.9`, 48 x 48.
- ChapterInformationPanel: `-168,-394`, 636 x 188.
  - ChapterStage: `70,34`, 430 x 42, font 25.
  - MineName: `70,-22.4`, 430 x 66, font 42.
- PowerComparisonPanel: `333,-394`, 306 x 188.
  - PowerValue: `28,8`, 220 x 72, font 43.
  - PowerLabel: `38,58`, 196 x 30, font 19.
  - RecommendedPower: `18,-50`, 236 x 34, font 18.
- BossWarningPanel: `0,-560`, 300 x 96; BossWarning text `38,0`, 202 x 72, font 24.

## Main actions and upgrade-card details

- MineButton: `0,843`, 450 x 186. The deleted `MineActionText` must not return.
- MineIcon: centered at `0,0`, approximately 78.91 x 85.25, original aspect preserved.
- OfflineRewardSurface: `-363,843`, 246 x 186.
  - Title `0,70`, 202 x 36, font 18.
  - Chest `0,8`, 112 x 92.
  - Reward text `0,-56`, 202 x 50, font 18.
- ResearchShortcut: `363,843`, 246 x 186.
  - Icon `0,0`, 116 x 116; title `0,70`, 204 x 36, font 18; badge `-27,-27` from top-right, 48 x 48.
- BossChallengeButton: `0,650`, 322 x 90.
  - Icon left `52,0`, 76 x 76.
  - BossActionText `43.05,0`, 209.9 x 64, font 25.
- Upgrade cards: x = -327, 0, 327; y = 420; each 302 x 330.
  - Level text `0,-35`, 250 x 42, font 27.
  - Pickaxe icon `0,68`, 144 x 156.
  - Drill icon `0,68`, about 164.26 x 140.79.
  - Robot icon `0,68`, 117 x 156.
  - Cost icon `-60.2,-124.6`, 38 x 38.
  - Cost text `11.8,-120.7`, 140 x 42, font 26.
  - UpgradeAction `101,-128`, 48 x 48; arrow 46 x 46.
  - RecommendationBadge top-right `-34,-34`, 54 x 54.

## Reconstruction constraints

1. Preserve every recorded position, size, anchor, pivot, sibling order, and active state unless the user explicitly requested the two exceptions below.
2. Keep every recorded deletion deleted and remove stale field/list/update logic that assumes those objects still exist.
3. Keep MissionsNavigation under the power panel, not in the bottom row.
4. Change only ProgressFill base width `488 -> 468` while keeping its left origin.
5. Do not change the settings modal or other inactive modal layouts in this pass.
6. Verify the reconstructed runtime at 1440 x 3088 before considering additional responsive adjustments.

## Implementation result

- Applied to source: 2026-08-02, after stopping Play Mode.
- Permanent layout source: `Assets/PocketForge/Scripts/Mining/MineHudViewV5.cs`.
- Shared progress width: `Assets/PocketForge/Scripts/Mining/MineHudView.cs`, `OreProgressWidth = 468f`.
- Regression coverage: `Assets/PocketForge/Tests/Editor/MineHudResponsiveLayoutTests.cs`.
- Removed-object update paths were deleted together with the objects, eliminating the repeated destroyed-Text `MissingReferenceException`.
- Verification: Unity compile errors 0, Console errors 0, targeted EditMode tests 30/30 and full EditMode tests 154/154 passed.
- Play Mode remained stopped; a post-change 1440 x 3088 runtime recapture and Android device verification were not performed in this pass.

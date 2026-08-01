# Task 13 Gas-Shot UI Asset Manifest

## Source references

- Equipment: `Task13_4_Equipment_GasShot_v2.png` (1440×3088)
- Museum: `Task13_5_Museum_GasShot_v2.png` (1440×3088)
- Achievements: `Task13_5_Achievements_GasShot_v2.png` (1440×3088)
- Main HUD geometry: `POCKET_FORGE_MAIN_UI_LAYOUT_CORRECTION_PROMPT.txt`

## Runtime output

- Runtime path: `Assets/PocketForge/Resources/PocketForge/UI/Task13`
- Runtime textures: 67 transparent PNG files
- Import contract: Sprite (2D and UI), Single, mipmaps off, Clamp, compressed, `Image.Type.Simple`
- Exceptions: progress fill sprites use `Image.Type.Filled` at runtime.
- Source-chroma files are review inputs only and are not loaded by the game.

## Composition groups

| Group | Runtime composition |
|---|---|
| Equipment | modal/title/capacity/power, four slot cards, six inventory cards, rarity/selected/equipped/merge/count overlays, comparison tray, equip/unequip/merge/auto actions |
| Museum | shared modal/title/tabs, summary, four exhibit cards, pedestals, ore/locked states, progress tracks/fills, next-reward strip |
| Achievements | shared modal/title/tabs, summary, six fixed-column rows, category icons, progress tracks/fills, reward slots, claim/in-progress/completed states |

## Reused and derived assets

- V5 pickaxe, drill, robot, museum-tab, lock, currency, gem, and blueprint-core icons are reused to keep HUD and modal silhouettes identical.
- Equipment rarity borders share one geometry and use Common/Rare/Epic/Legendary color variants.
- Comparison down arrow is a rotated/tinted variant of the existing upgrade arrow.

## Review captures

- `Review/Task13AssetContactSheet.png`
- `Review/Task13_MainHud_Final.png`
- `Review/Task13_MainHud_Overlay50.png`
- `Review/Task13_Equipment_Runtime.png`
- `Review/Task13_Museum_Runtime_v2.png`
- `Review/Task13_Achievements_Runtime.png`

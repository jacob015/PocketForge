# Pocket Forge V5 HUD Asset Manifest

## 기준

- 원본 가이드: `Reference/PocketForge_UI_Guide_852x1846.png`
- 생성 프롬프트: 프로젝트 루트 `POCKET_FORGE_UI_ASSET_PROMPTS.txt`
- 생성 원본: `SourceChroma/*_Chroma.png`
- 투명 검수본: `Sprites/*.png`
- 런타임 복사본: `Assets/PocketForge/Resources/PocketForge/UI/V5/*.png`
- 전체 검수 시트: `Review/PocketForge_UI_AssetContactSheet.png`

## 파일 세트

| ID | 자산 | 런타임 용도 |
|---:|---|---|
| 01 | HudPanel | 상단 통합 HUD |
| 02 | PortraitFrame | 광부 초상 프레임 |
| 03 | MinerPortrait | 광부 초상 |
| 04 | CreditsIcon | Credits 및 강화 가격 |
| 05 | GemIcon | Gem |
| 06 | BlueprintCoreIcon | 설계도 코어 |
| 07 | MinerExperienceIcon | 광부 XP |
| 08 | SettingsButton | 설정 진입 |
| 09 | ChapterPanel | 챕터·스테이지·광산명 |
| 10 | PowerPanel | 현재·권장 채굴력 |
| 11 | BossWarningPanel | 보스 거리·타이머 |
| 12 | OreHealthFrame | 광석 체력 프레임 |
| 13 | OreHealthFill | 광석 체력 Filled 이미지 |
| 14 | OfflineShortcutCard | 방치 보상·연구 바로가기 카드 |
| 15 | OfflineChestIcon | 방치 보상 |
| 16 | ResearchCompleteIcon | 연구 바로가기 |
| 17 | CompletionBadge | 연구 해금 상태 |
| 18 | MineButton | 채굴 버튼 |
| 19 | MinePickaxeIcon | 채굴 기능 아이콘 |
| 20 | BossButton | 보스 도전 버튼 |
| 21 | BossChallengeIcon | 보스 도전 기능 아이콘 |
| 22 | UpgradeCard | 곡괭이·드릴·로봇 강화 |
| 23 | RecommendationBadge | 구매 추천 표시 |
| 24 | UpgradeArrow | 강화 가능 표시 |
| 25 | PickaxeEquipmentIcon | 곡괭이 강화 |
| 26 | DrillEquipmentIcon | 드릴 강화 |
| 27 | RobotEquipmentIcon | 로봇 강화 |
| 28 | BottomNavigationBar | 하단 메뉴 |
| 29 | SelectedNavigationTab | Home 선택 상태 |
| 30 | EquipmentMenuIcon | 장비 메뉴 |
| 31 | ResearchMenuIcon | 연구 메뉴 |
| 32 | HomeMenuIcon | Home |
| 33 | MuseumMenuIcon | 박물관 메뉴 |
| 34 | MissionMenuIcon | 미션 메뉴 |
| 35 | ShopMenuIcon | 상점 메뉴 |
| 36 | NotificationBadge | 연구 가능 알림 |
| 37 | LockedIcon | 레벨 잠금 |
| 38 | AddActionIcon | Credits 보상형 광고 진입 |

## 적용 규칙

- 생성 이미지에는 문자·숫자·가격·진행도·번역 문구를 포함하지 않는다.
- 동적 정보는 Unity `Text`와 `LanguageService`의 한국어·영어·일본어·중국어 데이터로 표시한다.
- UI Sprite는 `Image.Type.Simple`을 사용한다. 실시간 체력 표현이 필요한 13번만 `Image.Type.Filled`를 사용한다.
- Android 임포트는 최대 1024px, ASTC 6×6, 밉맵 끔, Clamp, Bilinear다.
- 이번 세트는 배경을 포함하지 않는다. 동굴 배경과 애니메이션은 후속 작업 범위다.

## 검수 상태

- 38개 `#00FF00` 크로마키 원본, 38개 투명 PNG, 38개 런타임 사본이 일대일로 존재하며 런타임 사본과 투명본의 SHA-256이 모두 일치한다.
- 투명본 네 모서리 알파 0, 잔류 녹색 픽셀 0과 크로마 원본 네 모서리의 정확한 `#00FF00`을 Unity 픽셀 검사로 확인했다.
- 전체 자산을 접촉 시트로 검수해 가짜 문자·잘린 외곽이 없는 것을 확인했다.
- 38개 자산 모두 `MineHudViewV5`에서 참조되며 일반 이미지는 `Simple`, 체력 Fill만 `Filled`로 적용됐다.
- Unity 최종 컴파일과 Play Mode Console 오류는 0건이고 전체 EditMode 123/123을 통과했다.
- `Review/V5Hud_1080x2340_Runtime.png`에서 19.5:9 전체 HUD 합성을 검수했다. 해상도 전환 직후 이전 Safe Area가 보고되는 경우도 앵커가 0..1을 벗어나지 않도록 보정했다.

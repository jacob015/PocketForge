# Pocket Forge UI 겹침·9-slice 교정 검수

검수일: 2026-08-02

## 결과

- 메인 HUD, 장비, 박물관, 업적, 연구 화면의 충돌·잘림·강제 늘어남을 교정했다.
- Task13 전용 `Task13Sliced` 경로는 실제 픽셀 Border를 가진 Sprite만 생성한다.
- 공용 적용 헬퍼는 Border 존재와 `target >= border 합 + center 최소 크기`를 검사한다.
- 장비 비교 트레이는 구분선이 구워진 원본 전체 이미지를 늘리지 않고, 빈 Sliced 배경·코드 구분선·3개 텍스트 영역으로 분리했다.
- 외부 UI 패키지는 추가하지 않았다. 현재 문제는 새 레이아웃 시스템보다 UGUI RectTransform·Sprite Border 교정이 적합했다.

## 최종 렌더

- [메인 HUD](./MainHud_Final.png)
- [장비](./Equipment_Final.png)
- [연구](./Research_Final.png)
- [박물관](./Museum_Final.png)
- [업적](./Achievements_Final.png)

캡처는 401×726 Game View에서 Screen Space Camera로 동일 프레임을 확인하기 위한 검수본이다. 런타임 Canvas 기본 설정은 Screen Space Overlay이며 캡처 과정의 변경은 저장하지 않았다.

## 렌더 타입 분류

| 분류 | 자산·구조 | 이유 |
|---|---|---|
| Sliced | 모달 본체, 장착 슬롯, 박물관 전시 카드, 탭, 공용 행동 버튼 | 중앙이 균일하고 런타임 Rect가 변함 |
| Simple | 제목 플라크, 닫기 표면·아이콘, 장비 인벤토리 카드, 박물관·업적 요약, 업적 행, 다음 보상 스트립 | 내부 장식·구분선이 있어 고정 종횡비로 사용 |
| Filled | 광석 체력, 박물관 진행, 업적 진행 | 실제 수치에 따라 수평 채움 |
| 분리 조립 | 장비 비교 영역 | 빈 배경, 고정 Divider, 선택/차이/현재 텍스트를 독립 배치 |

`UiEquipmentInventoryCardBase`와 `UiAchievementRowBase`는 내부 선·장식이 중앙 영역을 지나므로 기계적인 Sliced 적용에서 제외했다. 현재 Rect를 원본 종횡비에 맞춘 Simple로 고정했다.

## Sliced 원본 크기와 Pixel Border

Border 순서는 `left, bottom, right, top`이다.

| 자산 | 원본 px | Border px |
|---|---:|---:|
| UiEquipmentModalBody | 485×1024 | 30, 30, 30, 30 |
| UiCollectionModalBody | 459×1024 | 30, 30, 30, 30 |
| UiEquipmentSlotCardBase | 302×512 | 32, 32, 32, 32 |
| UiMuseumExhibitCardBase | 288×512 | 30, 30, 30, 30 |
| TabCollectionActive | 512×116 | 28, 18, 28, 18 |
| TabCollectionInactive | 512×173 | 28, 18, 28, 18 |
| ButtonEquipmentEquip | 512×231 | 36, 28, 36, 28 |
| ButtonEquipmentUnequip | 512×222 | 36, 28, 36, 28 |
| ButtonEquipmentMerge | 512×258 | 42, 34, 42, 34 |
| ButtonEquipmentAutoEquip | 512×436 | 48, 44, 48, 44 |
| ButtonAchievementClaim | 512×317 | 36, 28, 36, 28 |
| UiAchievementInProgressState | 512×157 | 24, 16, 24, 16 |

연구 모달과 연구 행은 별도 전용 자산을 억지로 늘리지 않고 `UiCollectionModalBody`의 균일 표면을 같은 Border로 재사용한다.

## 자동·시각 검증

- Unity 컴파일 오류: 0건
- UI·화면비·다국어 대상 EditMode: 30/30 통과
- 전체 EditMode: 154/154 통과
- 화면비 계약: 기준 너비 1080, 세로 높이 1920·2340·2400에서 HUD 순서와 하단 내비게이션 경계 검사
- 다국어 계약: 한국어·영어·일본어·중국어에서 업적 보상/수령, 연구 텍스트/버튼 열 비교차 검사
- 실제 렌더: 메인·장비·연구·박물관·업적 5개 화면 육안 검사

Android APK/AAB 빌드와 실기기 검증은 이번 작업에서 실행하지 않았다.

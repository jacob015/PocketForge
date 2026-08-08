# Play Console 등록정보 초안

마지막 갱신: 2026-08-08 · 대상: `com.jacob015.pocketforge` · 버전 `0.1.0`

배포 범위는 **대한민국 단독**으로 시작한다. UMP(EU 사용자 동의)가 미구현이라 EEA·영국을 포함하면 Google EU 사용자 동의 정책을 만족하지 못한다. 정식 출시 시점에 재검토한다.

기본 언어는 한국어, 추가 언어는 영어로 등록한다. 앱 자체는 한국어·영어·일본어·중국어를 지원하지만, 배포 국가를 한국으로 제한하는 동안 스토어 등록정보는 두 언어만 유지해 관리 비용을 줄인다.

---

## 1. 앱 이름 (최대 30자)

| 언어 | 문구 | 길이 |
|---|---|---:|
| 한국어 | `포켓 포지: 방치형 광산 키우기` | 17 |
| 영어 | `Pocket Forge: Idle Mining` | 25 |

## 2. 간단한 설명 (최대 80자)

| 언어 | 문구 | 길이 |
|---|---|---:|
| 한국어 | `탭 한 번으로 광석을 캐고, 자리를 비운 사이에도 광산이 자란다. 방치형 채굴 RPG.` | 45 |
| 영어 | `Tap to mine, and keep growing while you are away. An idle mining adventure.` | 74 |

## 3. 자세한 설명 (최대 4000자)

### 한국어

```
화면을 두드리면 광석이 부서지고, 손을 떼도 광산은 계속 자랍니다.

포켓 포지는 짧은 순간의 손맛과 긴 호흡의 성장을 함께 담은 방치형 광산 게임입니다.
출근길 한 손으로 몇 번 두드리고, 다시 켜면 자동 채굴기가 모아 둔 보상이 기다립니다.

■ 두드리면 바로 부서지는 손맛
곡괭이를 강화할수록 광석이 더 빨리 부서집니다. 강화 한 번의 효과가 다음 탭에서 바로 보입니다.

■ 꺼 두는 동안에도 자라는 광산
드릴과 로봇을 배치하면 앱을 종료한 사이에도 채굴이 이어집니다.
다시 접속하면 최대 4시간까지 쌓인 오프라인 보상을 받습니다.

■ 챕터와 보스
챕터마다 마지막에 거대한 보스 광석이 기다립니다.
지금 채굴력으로는 부족합니다. 시설을 키우고 장비를 갖춰 다시 도전하세요.

■ 모아서 강해지는 재미
- 장비: 등급별 곡괭이를 획득하고 합성해 채굴력을 올립니다
- 연구: 설계도 코어로 영구 강화를 해금합니다
- 박물관: 캐낸 광물을 도감에 채울수록 채굴력이 영구히 오릅니다
- 업적과 일일·주간 미션: 매일 접속할 이유를 만듭니다

■ 편하게 즐기는 설계
- 세로 화면 한 손 조작
- 한국어, 영어, 일본어, 중국어 지원
- 강제 전면 광고는 최소한으로. 광고 제거 상품을 구매하면 강제 광고가 사라지고, 원할 때 보는 보상형 광고는 그대로 유지됩니다

지금 바로 첫 광석을 두드려 보세요.
```

### 영어

```
Tap the screen and the ore cracks. Put the phone down and the mine keeps working.

Pocket Forge is an idle mining game built around two rhythms: the immediate
satisfaction of a tap, and the long arc of a mine that grows without you.

■ Tap-first mining
Every pickaxe upgrade shows up on your very next tap. No waiting to feel it.

■ Progress while you are away
Place drills and robots and mining continues after you close the app.
Come back to collect up to four hours of offline rewards.

■ Chapters and bosses
Each chapter ends with a massive boss ore. Your current mining power will not be
enough. Grow your facilities, gear up, and come back for it.

■ Many ways to get stronger
- Equipment: collect and fuse pickaxes by rarity
- Research: spend blueprint cores on permanent upgrades
- Museum: filling the mineral collection permanently raises mining power
- Achievements and daily/weekly missions

■ Built to be easy to play
- One-handed portrait play
- Korean, English, Japanese, and Chinese
- Forced interstitials are kept sparse. The ad-removal purchase stops them entirely
  while optional rewarded ads stay available when you want them

Crack your first ore now.
```

---

## 4. 데이터 보안 양식 답변

근거는 `WORKING_CONTEXT.md` 수정사항 78 (측정 결과). 개인정보처리방침은 `docs/privacy-policy.html`.

목적·임시 처리 항목은 추측하지 않고 Google 공식 안내를 확인했다:
[Google Play data disclosure (AdMob Unity)](https://developers.google.com/admob/unity/privacy/play-data-disclosure),
[Provide information for Google Play's Data safety section](https://support.google.com/googleplay/android-developer/answer/10787469).

| 항목 | 답변 |
|---|---|
| 데이터를 수집·공유합니까? | 예 (제3자 광고 SDK 한정) |
| 기기 또는 기타 ID | **수집됨 + 공유됨**. 목적 3개 전부: 광고 또는 마케팅 · 분석 · 부정 행위 방지·보안 및 규정 준수. AdMob이 광고 ID와 앱 세트 ID를 자동 수집한다 |
| 위 데이터가 임시로 처리됩니까? | **아니요**. Play의 임시 처리는 메모리에만 두고 실시간 요청 처리에 필요한 시간 이상 보관하지 않는 경우다. 광고 ID는 개인 최적화·빈도 제한·전환 측정에 쓰이며 그 이상 보관된다. 임시 처리로 신고하면 등록정보에 표시되지 않으므로 잘못된 신고가 된다 |
| 위 데이터 수집이 필수입니까? | **필수**. 앱 내에서 사용자가 끌 수 없다. 기기 설정의 광고 ID 재설정은 OS 수준이라 Play의 "사용자 선택 가능"에 해당하지 않는다 |
| 앱 활동(앱 상호작용) | **공유함** — 광고 노출·클릭 측정 목적 |
| 대략적 위치 | 수집 안 함 (IP 기반 처리는 AdMob 측 광고 게재 목적) |
| 개인정보(이름·이메일 등) | 수집 안 함 |
| 금융 정보 | 수집 안 함 — 결제는 Google Play가 처리하며 앱은 결제수단에 접근하지 않음 |
| 사진·동영상·파일·연락처·메시지 | 수집 안 함 |
| 앱 성능(충돌·진단) | 수집 안 함 — Unity 충돌·성능 리포팅 비활성 |
| 전송 중 데이터 암호화 | 예 |
| 데이터 삭제 요청 경로 | 앱 삭제 또는 앱 데이터 삭제 (기기 로컬 저장만 존재) |

## 5. 콘텐츠 등급 설문 예상 답변

| 항목 | 답변 |
|---|---|
| 폭력 | 없음 (광석을 채굴하는 표현만 존재) |
| 성적 콘텐츠 · 언어 · 약물 | 없음 |
| 도박 · 시뮬레이션 도박 | 없음 (확률형 장비 획득은 있으나 현금 구매 대상이 아님 — 설문 시 재확인 필요) |
| 사용자 간 상호작용 | 없음 (온라인 기능·채팅 없음) |
| 위치 공유 | 없음 |
| 디지털 구매 | **있음** (인앱 상품) |
| 광고 표시 | **있음** |

## 6. 그래픽 자산

모두 `docs/store-assets/`에 있다. 기존 게임 아트에서 합성해 스크린샷과 같은 팔레트를 유지했다(동굴 파랑 `#2b3e91` / `#121a42`, 액션 금색 `#fdb916` — 스크린샷에서 직접 추출).

| 자산 | 요구 규격 | 파일 | 상태 |
|---|---|---|---|
| 앱 아이콘 | 512×512 32비트 PNG, 1024KB 이하 | `app_icon_512.png` (229 KB) | 준비 완료 |
| 정사각 런처 아이콘 | — | `app_icon_432_square.png` | 준비 완료 |
| 그래픽 이미지(피처) | 1024×500 24비트 PNG(알파 없음) | `feature_graphic_1024x500.png` | 준비 완료 |
| 휴대전화 스크린샷 | 2~8장, 장당 최대 8MB, **비율 16:9 또는 9:16**, 320~3840px | `screenshot_01_mining` ~ `screenshot_05_settings` (1440×2560) | 준비 완료 |

스크린샷 규격 확정 경위: 처음에는 Play 도움말의 하드 제한("긴 변이 짧은 변의 2배 이하")만 보고 1440×2880(2.0:1)으로 만들었다. 그런데 Play Console 등록정보 폼은 비율을 **16:9 또는 9:16**으로 명시한다. 어느 쪽이 검증되는지 따지는 대신 정확히 9:16(1440×2560)으로 맞췄고, 그러면 프로모션 자격 조건("4개 이상 중 3개는 16:9 또는 9:16에 1,080px 이상")도 함께 충족한다.

1440×3120 원본에서 9:16으로 자르려면 560px을 빼야 하는데 그중 하단 320px이 하단 내비게이션 바 전체다. 그래서 전체 화면을 높이에 맞춰 축소(1182×2560)하고 좌우 129px 여백은 같은 화면을 cover 스케일·블러·감광한 배경으로 채웠다. 처음 시도한 가장자리 컬럼 늘리기는 가로 줄무늬가 눈에 띄어 기각했다. UI 손실은 0이다.

필수 장수는 **2장**이다. 폼 안내문의 "4개 이상 + 최소 3개는 9:16" 조건은 프로모션(추천) 자격용이다.

태블릿 스크린샷은 불필요하다. 필수 문구가 "휴대전화나 태블릿 스크린샷을 2개 이상"이므로 휴대전화만 채우면 충족된다. 세로 고정 폰 전용 게임이다.

박물관 샷은 2026-08-09에 빌드 #22(수정 후)로 재캡처했다. `4 / 4` 옆이 미발견 `?`가 아니라 초록 완료 배지로 나온다. 5장 모두 업로드 가능하다.

인게임 런처 아이콘 (2026-08-08 해결):

`Assets/PocketForge/Art/AppIcon/`의 Adaptive 전경·배경, Round, Legacy 레이어를 `PocketForgeAppIcon`이 `PlayerSettings` API로 지정한다. 릴리스 빌드가 매번 재적용하므로 CI 워크스페이스에서도 적용된다.

1차 시도는 kind를 `"Adaptive"` 문자열로 매칭했는데 실제 이름이 `"Adaptive (API 26)"`·`"Round (API 25)"`여서 Legacy만 적용됐고, 기본값에 있던 adaptive 아이콘을 오히려 제거했다. `icon.maxLayerCount`로 판정하도록 고쳤다.

빌드 #17 AAB 실측 (빌드 #6 기본값 → #17):

| 리소스 | 기본값 | 적용 후 |
|---|---|---|
| `app_icon.png` | 1종 926 B | 6종 91,936 B |
| `app_icon_round.png` | 없음 | 6종 91,591 B |
| `ic_launcher_foreground.png` | 1종 1,612 B | 6종 157,352 B |
| `ic_launcher_background.png` | 1종 101 B | 6종 167,290 B |
| `mipmap-anydpi-v26` XML | 1개 | 2개 |

미검증: 기기에 설치해 런처 아이콘을 눈으로 확인하는 단계는 기기 연결이 끊겨 수행하지 못했다.

## 7. 등록 전 남은 차단 요소

1. 개인정보처리방침 게시 — 파일은 `docs/privacy-policy.html`에 준비 완료. 저장소 Settings > Pages에서 `main` / `/docs` 활성화 시 `https://jacob015.github.io/PocketForge/privacy-policy.html`
2. 광고 단위 ID가 Google 공식 테스트 ID 상태 — 실 ID 교체 전에는 수익이 발생하지 않음
3. `bundleVersion`이 `0.1.0` — 내부 테스트 시작 버전으로 확정할지 결정 필요

업로드할 AAB: 빌드 #17의 `PocketForge-17.aab` (52.11 MiB, versionCode 17). 아이콘·박물관 수정·폰트 하한·밸런싱이 모두 포함된 첫 산출물이다. 50MiB 예산 초과로 UNSTABLE 표시가 붙지만 업로드에는 지장이 없다.

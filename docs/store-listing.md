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

| 항목 | 답변 |
|---|---|
| 데이터를 수집·공유합니까? | 예 (제3자 광고 SDK 한정) |
| 기기 또는 기타 ID | **수집·공유함** — 광고 목적. AdMob이 광고 ID를 수집 |
| 앱 활동(앱 상호작용) | **공유함** — 광고 노출·클릭 측정 목적 |
| 대략적 위치 | 수집 안 함 (IP 기반 처리는 AdMob 측 광고 게재 목적) |
| 개인정보(이름·이메일 등) | 수집 안 함 |
| 금융 정보 | 수집 안 함 — 결제는 Google Play가 처리하며 앱은 결제수단에 접근하지 않음 |
| 사진·동영상·파일·연락처·메시지 | 수집 안 함 |
| 앱 성능(충돌·진단) | 수집 안 함 — Unity 충돌·성능 리포팅 비활성 |
| 전송 중 데이터 암호화 | 예 |
| 데이터 삭제 요청 경로 | 앱 삭제 또는 앱 데이터 삭제 (기기 로컬 저장만 존재) |
| 데이터 수집이 선택사항입니까? | 아니요 (광고 게재에 필요) |

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

## 6. 필요한 그래픽 자산

| 자산 | 요구 규격 | 현재 상태 |
|---|---|---|
| 앱 아이콘 | 512×512 PNG (32비트, 알파 없음) | **미준비** |
| 그래픽 이미지(피처) | 1024×500 PNG/JPG | **미준비** |
| 휴대전화 스크린샷 | 최소 2장, 320~3840px | 기기 캡처 5장 보유(`Review/device/*.png`) — 단 1440×3120은 세로비 2.167:1로 Play의 9:16(1.78:1) 상한을 넘을 가능성이 높아 **1440×2560 등으로 재구성 필요**. 등록 시 실제 허용 여부 확인 |
| 7인치·10인치 태블릿 스크린샷 | 선택 | 미준비 (태블릿 미지원 선언 시 불필요) |

## 7. 등록 전 남은 차단 요소

1. 개인정보처리방침 게시 URL 확정 — GitHub Pages 활성화 예정. 문의 이메일 확정 후 게시
2. 광고 단위 ID가 Google 공식 테스트 ID 상태 — 실 ID 교체 전에는 수익이 발생하지 않음
3. 앱 아이콘·피처 그래픽 미준비
4. 스크린샷 종횡비 재구성
5. `bundleVersion`이 `0.1.0` — 내부 테스트 시작 버전으로 확정할지 결정 필요

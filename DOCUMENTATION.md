# Pocket Forge 문서 안내

이 문서는 루트 Markdown의 목적과 갱신 위치를 빠르게 찾기 위한 색인이다. 구현되지 않은 계획을 구현 사실처럼 기록하지 않는다.

## 기준 문서

| 문서 | 기준 정보 | 갱신 시점 |
|---|---|---|
| `RULES.md` | 파일 수정, 승인, 검증, 완료 보고 규칙 | 운영 규칙을 변경할 때만 |
| `TASKS.md` | 현재 작업의 범위·상태·제외 항목 | 작업 시작·완료·범위 변경 시 |
| `PROJECT_PLAN.md` | 제품 목표, MVP 루프, 마일스톤, 기술 방향 | 기획 변경이 승인되었을 때 |
| `ARCHITECTURE.md` | 현재 구현의 폴더·씬·런타임·데이터 구조 | 구현 구조가 변경되었을 때 |
| `WORKING_CONTEXT.md` | 실제로 수행한 변경과 검증 결과의 누적 이력 | 모든 작업 완료 후 |
| `AI_USAGE.md` | AI 산출물과 사람 또는 도구로 수행한 검증 | AI 산출물을 사용·검증할 때 |
| `AGENTS.md` / `CLAUDE.md` | 도구별 작업 진입 지침 | 협업 절차가 바뀔 때 |

## 참조 순서

작업자는 `AGENTS.md` 또는 `CLAUDE.md`를 먼저 읽고, 이어서 `RULES.md` → `WORKING_CONTEXT.md` → `TASKS.md` → `PROJECT_PLAN.md` → `AI_USAGE.md` 순서로 확인한다.

`TASKS.md`와 `WORKING_CONTEXT.md`가 충돌하면, 현재 범위는 `TASKS.md`를 우선하고 과거 사실은 `WORKING_CONTEXT.md`를 보존한다.

## 아키텍처 문서화 기준

Unity 프로젝트가 만들어지고 구현 사실이 생긴 뒤에만 다음 주제를 별도 문서로 기록한다.

- 실제 프로젝트·Assets 폴더 구조와 모듈 책임
- 씬 구성, GameObject 및 Inspector 연결
- 런타임 흐름과 데이터 흐름
- ScriptableObject·저장 데이터의 실제 스키마
- 외부 SDK의 버전, 설정, 검증 상태

각 문서는 근거가 되는 코드·Unity 설정·검증 방법을 함께 적고, 미구현 항목은 `예정`으로 표시한다.

# Pocket Forge Agent Guide

이 저장소에서 작업하는 Codex는 작업을 시작하기 전에 다음 파일을 순서대로 읽는다.

1. `RULES.md`
2. `WORKING_CONTEXT.md`
3. `TASKS.md`
4. `PROJECT_PLAN.md`
5. `AI_USAGE.md`

문서별 역할과 갱신 기준은 `DOCUMENTATION.md`에서 확인한다.

## 작업 원칙

- `RULES.md`의 작업 전 확인, 사용자 승인, 작업 완료 보고, 작업 기록 규칙을 최우선으로 따른다.
- `TASKS.md`에서 `현재 작업`으로 지정된 항목만 수행한다.
- 외부 패키지, SDK, Unity 설정 변경, 배포, 계정·결제 관련 작업은 이유와 영향 범위를 먼저 설명하고 승인받는다.
- 작업 완료 후에는 `WORKING_CONTEXT.md`에 사실만 기록하고 `TASKS.md`의 현재 작업을 갱신한다.
- 프로젝트 폴더 밖의 파일, OS 설정, 자격 증명, Play Console 및 AdMob 계정은 명시적 요청 없이 변경하지 않는다.

## 검증

- 실행하지 못한 테스트는 완료로 보고하지 않는다.
- Unity 씬 연결이나 Inspector 설정이 필요하면 작업 완료 보고와 `TASKS.md`에 남긴다.
- AI가 제안한 코드·설정은 공식 문서, 컴파일 로그 또는 실제 기기 테스트 중 가능한 방법으로 검증한다.

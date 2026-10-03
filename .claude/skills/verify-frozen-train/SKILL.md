---
name: verify-frozen-train
description: Low-cost final verification for FrozenTrain changes. Invoke manually before commit/PR when you want a concise static review without broad codebase exploration.
disable-model-invocation: true
---
# Verify FrozenTrain change

Keep verification cheap and deterministic.

1. `git status --short`
2. `git diff --check`
3. `git diff --stat`
4. `git diff --name-only`
5. Inspect only the changed hunks with `git diff -- <changed source files>`.
6. For renamed/removed symbols, use targeted `git grep` for those exact names.
7. Flag only concrete issues: compile-risk references, duplicate systems, blocked paths/windows/ladders, broken interaction names, or architecture-rule violations.

Do not scan unrelated folders. Do not spawn subagents. Do not restate the whole project architecture.

Output only:
- blocking issues;
- non-blocking risks;
- verification commands run;
- whether Unity runtime compilation was actually executed.
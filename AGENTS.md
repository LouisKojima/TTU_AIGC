# Repository Guidelines

## Project Structure & Module Organization
- Source: `Assets/IAV/Scripts` — primary C# runtime code (MonoBehaviours, managers, utilities). Keep custom gameplay/UI logic here.
- Assets: `Assets/IAV` — project-specific art, prefabs, scenes, and resources. Third‑party packages live under their vendor folders (e.g., `Assets/Barmetler`, `Assets/PathCreator`, `Assets/Sirenix`). Avoid modifying vendor code.
- Resources: `Assets/Resources` — shared assets loaded at runtime. Use sparingly and document addressable keys.
- Tests: Prefer `Assets/Tests` or `Assets/IAV/Tests` with `EditMode` and `PlayMode` subfolders.

## Build, Test, and Development Commands
- Open project (Editor): launch Unity 2024 and open the repo root.
- Play in Editor: use the main scene in `Assets/IAV` (or active scene) and verify console is clean.
- CLI build (example):
  ```powershell
  $env:UNITY_EDITOR="C:\\Program Files\\Unity\\Hub\\Editor\\<version>\\Editor\\Unity.exe"
  & $env:UNITY_EDITOR -batchmode -projectPath . -quit -buildWindows64Player "Builds/Win64/IAV.exe"
  ```
- CLI tests (example):
  ```powershell
  & $env:UNITY_EDITOR -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -quit
  & $env:UNITY_EDITOR -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults TestResults/playmode.xml -quit
  ```

## Coding Style & Naming Conventions
- Indentation: 4 spaces; UTF‑8; one class per file.
- C#: `PascalCase` for classes/methods/properties, `camelCase` for locals/fields, `_camelCase` for private serialized fields, `UPPER_SNAKE_CASE` for constants.
- Files: match class name (e.g., `VehicleController.cs`). Namespaces: `IAV.<Feature>`.
- Formatting: use Rider/VS “Format Document”. Optional: `dotnet format IAV_Unity_2024Sep.sln` (if SDK installed).

## Testing Guidelines
- Framework: Unity Test Framework (NUnit‑style). Place `[Test]` in EditMode, `[UnityTest]` for coroutine/PlayMode.
- Structure: `Assets/IAV/Tests/{EditMode|PlayMode}/*Tests.cs`.
- Coverage: aim ≥60% on core utilities and gameplay logic.
- Run: via Test Runner or CLI commands above. Keep tests deterministic and scene‑independent where possible.

## Commit & Pull Request Guidelines
- Commits: small, focused, imperative tense. Prefer Conventional Commits, e.g., `feat: add lane change HUD`, `fix: null check in PathFollower`.
- Commit granularity & grouping:
  - Split by feature/concern. Do not batch unrelated changes into a single commit.
  - Example: AIGC skybox pipeline in one commit; Spark/LLM integration in a separate commit; UI-only tweaks in their own commit.
  - If fixing an incidental issue discovered during work (e.g., stray token/formatting), use a separate `fix:` commit before or after the main feature commit.
  - When touching runtime + tests + docs, prefer distinct commits: `feat:` (runtime), `test:` (tests), `docs:` (notes).
- PRs: clear description, linked issue, steps to test, and Editor/Play screenshots or short clips. Include any performance notes and known limitations.

## Agent Workflow & Approvals

- Do NOT run `git commit` or `git push` without explicit user approval.
- 在整理日志、总结或回复变更说明时，请尽量使用中文解释已完成的工作和后续建议步骤，方便团队理解。
- Default flow: prepare changes in working tree, share a concise diff/summary, and wait for review/approval.
- When approved, prefer pushing to a feature branch (e.g., `feature/<topic>`) and open a PR instead of pushing directly to `main`.
- Group commits by feature as above; avoid mixing unrelated changes in one commit.
- For potentially disruptive actions (history rewrite, force-push, deletes), always ask first and document rationale.

## Security & Configuration Tips
- Unity version: keep in sync with `ProjectSettings/ProjectVersion.txt`.
- Do not commit `Library/`, `Temp/`, or build outputs. Keep large binaries in `Assets/IAV` organized and referenced.
- Review serialized field defaults to avoid shipping debug flags.

## Agent Daily Session Notes (Routine)

- Location: create a daily summary markdown under `AIGC/Agent Markdown/` named `YYYY-MM-DD_Session_Summary.md`.
- When: at the end of each working day (or when the user says "收工"/"总结"), generate or update that day's file.
- Scope: summarize changes, usage, pitfalls, and next steps for this Unity project.
- Recommended sections:
  - What We Built/Fixed Today (bullets with file paths)
  - How To Use (End-to-End)
  - Inspector Pitfalls / Wiring Notes
  - Pusher / Networking Notes (if relevant)
  - File/Runtime Notes (StreamingAssets vs Assets)
  - Known Limitations / Ideas
  - Next Steps (Suggestions)
- Keep entries concise, actionable, and aligned with repo structure/naming.

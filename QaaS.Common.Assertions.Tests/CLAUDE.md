# CLAUDE.md — QaaS.Common.Assertions.Tests

> Test operating manual. See repo root `CLAUDE.md`.

## Purpose

Behavioural coverage for every assertion in `QaaS.Common.Assertions`.
One test folder per source folder; no production code is excluded.

## Layout

- `HermeticTests/`, `DelayTests/`, `ContentLogicTests/`,
  `SchemaLogicTests/`, `DeserializationLogicTests/`,
  `HttpMetaDataLogicTests/` — one test class per assertion class.
- `Mocks/` — hand-rolled stand-ins:
  `MockGenerator.cs`, `MockDeserializerWorksWithAllData.cs`,
  `MockDeserializerWorksWithNoData.cs`, `OutputDeserializableToMock.cs`.
- `TestData/` — JSON / CSV / schema fixtures loaded by tests.
- `Utils/` — shared assertion helpers.
- `Globals.cs` — Serilog → MEL bridge writing to `NUnitOutput()`,
  exposing `Globals.Logger` and a shared `Context` (see
  `Globals.cs:11-19`).

## Conventions

- **NUnit**. `Assert.Equal(expected, actual)` order matters in xUnit;
  here use NUnit's `Assert.That(actual, Is.EqualTo(expected))`.
- Hand-written mocks live under `Mocks/`; do **not** introduce Moq /
  NSubstitute for new tests without aligning with existing style.
- Test data is loaded relative to the test assembly — keep
  `<None Update="TestData/...">` `CopyToOutputDirectory` in the csproj.
- Logger is shared via `Globals.Logger`; do not new up another Serilog
  pipeline per test.

## Forbidden

1. `[Test(Ignore=...)]` / `[Explicit]` to dodge a red test — diagnose.
2. Touching real network / filesystem outside `TestData/`.
3. Adding Moq / NSubstitute when a `Mocks/` shim already exists.
4. Static mutable state across tests (parallel runs will flake).
5. `Thread.Sleep` for timing-sensitive delay tests — use the assertion
   under test plus a deterministic clock fake.

## Run

```bash
dotnet test ../QaaS.Common.Assertions.sln --nologo --no-build
```

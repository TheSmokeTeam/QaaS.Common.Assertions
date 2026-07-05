# Copilot Instructions — QaaS.Common.Assertions

Read `AGENTS.md` at the repo root first — it contains the complete hook contract, assembly
scanning rules, tier constraints, and process requirements for this library.

## Essentials
- **TFM**: net10.0; `<Nullable>enable</Nullable>`; `<ImplicitUsings>enable</ImplicitUsings>`
- **Test framework**: NUnit 4.x + NUnit3TestAdapter + Microsoft.NET.Test.Sdk; coverage collected and reported
- **Build**: `dotnet build` | **Test**: `dotnet test`
- **Hook contract**: every assertion inherits `BaseAssertion<TConfiguration>` (from
  `QaaS.Framework.SDK`); must be public + non-abstract for assembly scanner to discover it
- **Tier-1 rule**: no dependencies on QaaS.Runner, QaaS.Mocker, or any Tier-2+ package
- **Top gotcha**: class/assembly name must stay in `QaaS.*` or `Common.*` namespace prefix —
  renaming breaks runtime hook discovery silently
- **Commit style**: conventional commits (`feat:`, `fix:`, `chore(release):`) + `dotnet format` clean
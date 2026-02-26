# eShopSupport .NET 10.0 Upgrade Tasks

## Overview

This document tracks the execution of the eShopSupport solution upgrade from .NET 8.0 to .NET 10.0. All 12 projects will be upgraded simultaneously in a single atomic operation, followed by comprehensive testing and validation.

**Progress**: 3/4 tasks complete (75%) ![0%](https://progress-bar.xyz/75)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-02-26 20:49)*
**References**: Plan §Executive Summary, Plan §Implementation Timeline Phase 0

- [✓] (1) Verify .NET 10.0 SDK installed on system per Plan §Phase 0 Prerequisites
- [✓] (2) .NET 10.0 SDK version meets minimum requirements (**Verify**)
- [✓] (3) Check global.json compatibility (if file exists in solution root)
- [✓] (4) global.json compatible with .NET 10.0 SDK (**Verify**)

---

### [✓] TASK-002: Atomic framework and dependency upgrade with compilation fixes *(Completed: 2026-02-26 21:27)*
**References**: Plan §Implementation Timeline Phase 1, Plan §Package Update Reference, Plan §Breaking Changes Catalog, Plan §Project-by-Project Migration Plans

- [✓] (1) Update TargetFramework to net10.0 in all 12 project files listed in Plan §Implementation Timeline Phase 1
- [✓] (2) All project files updated to net10.0 (**Verify**)
- [✓] (3) Update all 19 package references across all projects per Plan §Package Update Reference (key packages: Aspire 8.2.2 → 13.1.2, ASP.NET Core 8.0.10 → 10.0.3, Microsoft.Extensions 8.x → 10.x, OpenTelemetry 1.9.0 → 1.15.0, SemanticKernel.Core 1.25.0 → 1.72.0)
- [✓] (4) All package references updated to target versions (**Verify**)
- [✓] (5) Restore all dependencies for entire solution
- [✓] (6) All dependencies restored successfully (**Verify**)
- [✓] (7) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog (focus areas: 8 binary incompatible APIs in EvaluationTests, 51 source incompatible APIs across projects including JWT authentication, Blazor components, Razor Pages, TensorPrimitives, ApiKeyCredential)
- [✓] (8) Solution builds with 0 errors (**Verify**)

---

### [✓] TASK-003: Run full test suite and validate upgrade *(Completed: 2026-02-26 15:30)*
**References**: Plan §Testing & Validation Strategy, Plan §Implementation Timeline Phase 2

- [✓] (1) Run tests in EvaluationTests and E2ETest projects per Plan §Phase 2 Testing
- [✓] (2) Fix any test failures (reference Plan §Breaking Changes Catalog for common issues; focus on binary incompatible API updates in EvaluationTests and Aspire.Hosting.Testing changes in E2ETest)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests pass with 0 failures (**Verify**)

---

### [▶] TASK-004: Final commit
**References**: Plan §Source Control Strategy

- [▶] (1) Commit all changes with message: "chore: Upgrade solution to .NET 10.0

- Update all 12 projects from net8.0 to net10.0
- Upgrade 19 NuGet packages to .NET 10.0-compatible versions
- Fix security vulnerability: Microsoft.SemanticKernel.Core 1.25.0 → 1.72.0
- Upgrade Aspire packages from 8.2.2 to 13.1.2
- Fix 8 binary incompatible API calls in EvaluationTests
- Fix 51 source incompatible API calls across projects
- Address breaking changes in JWT authentication, Blazor, TensorPrimitives
- All tests passing: EvaluationTests, E2ETest"

---









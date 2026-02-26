# .NET 10.0 Upgrade Plan

## Table of Contents
- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario
Upgrade eShopSupport solution from .NET 8.0 to **.NET 10.0 (Long Term Support)**.

### Scope
- **Total Projects**: 12 projects requiring upgrade
- **Current State**: All projects (except PythonInference.pyproj) currently target .NET 8.0
- **Target State**: All .NET projects will target .NET 10.0
- **Total Issues Identified**: 158 issues
  - Mandatory: 20 (12 target framework changes, 8 binary incompatible APIs)
  - Potential: 125 (51 source incompatible APIs, 52 behavioral changes, 22 package upgrades)
  - Optional: 13 (12 deprecated packages, 1 security vulnerability)

### Selected Strategy
**All-At-Once Strategy** - All projects upgraded simultaneously in single coordinated operation.

**Rationale**: 
- Medium-sized solution (12 projects)
- All projects currently on .NET 8.0 (except Python project on net472)
- Clear dependency structure with 5 levels
- All NuGet packages have target framework versions available
- No circular dependencies detected
- Aspire-based microservices architecture benefits from unified upgrade

### Complexity Classification
**Medium Complexity Solution**

**Discovered Metrics**:
- Projects: 12
- Dependency Depth: 5 levels (Level 0-4)
- High-Risk Projects: 0 (largest project has 30 issues but manageable)
- Security Vulnerabilities: 1 (Microsoft.SemanticKernel.Core)
- Target Technology: Homogeneous (.NET 10.0)
- Circular Dependencies: None

**Classification Justification**:
- ≤15 projects ✓
- Depth ≤5 levels ✓
- No high-risk blocking issues ✓
- Single security vulnerability (easily addressable) ✓
- Foundation project (ServiceDefaults) affects 7 dependent projects but has only 26 issues

### Critical Issues

#### Security Vulnerabilities
⚠️ **Package with Security Vulnerability** (must be addressed during upgrade):
- **Microsoft.SemanticKernel.Core**: 1.25.0 → 1.72.0
  - Project: DataIngestor
  - Severity: Optional (but recommended to fix immediately)
  - Remediation: Upgrade to version 1.72.0

#### Breaking Changes
- **8 Binary Incompatible APIs** (mandatory fixes required)
- **51 Source Incompatible APIs** (code changes needed for compilation)
- **52 Behavioral Changes** (runtime behavior may differ)

### Recommended Approach
**All-At-Once Atomic Upgrade**:
- Single coordinated operation updating all projects simultaneously
- All project files and packages updated together
- One comprehensive build and fix cycle
- Unified testing phase
- Single commit workflow (preferred)

### Iteration Strategy
**Phase-based approach** (one iteration per migration phase):
- Phase 1: Discovery & Classification ✅
- Phase 2: Foundation (dependency analysis, strategy, stubs) 
- Phase 3: Dynamic detail generation (one iteration per dependency level)
- Expected remaining iterations: 7-8

---

## Migration Strategy

### Approach Selection: All-At-Once Strategy

**Decision**: Upgrade all 12 projects simultaneously in a single atomic operation.

### Justification

#### Why All-At-Once is Optimal

**Solution Characteristics Favor All-At-Once**:
- ✅ Medium-sized solution (12 projects, under the 30-project threshold)
- ✅ All projects currently on .NET 8.0 (homogeneous starting point)
- ✅ Clean dependency structure with no circular dependencies
- ✅ All NuGet packages have known target versions available
- ✅ Aspire-based microservices architecture designed for coordinated updates
- ✅ No high-complexity individual projects (largest has 30 issues)
- ✅ Single security vulnerability easily addressable in atomic update

**Advantages for This Solution**:
- **Fastest completion time**: One coordinated operation vs 5 phases
- **No multi-targeting complexity**: Clean .NET 8.0 → .NET 10.0 transition
- **Simplified testing**: Test entire solution at once, not incremental states
- **Clean dependency resolution**: No version conflicts from mixed TFMs
- **Aspire alignment**: Aspire projects benefit from unified .NET 10.0 stack

**Avoided Complexity**:
- No need for multi-targeting (TargetFrameworks with net8.0;net10.0)
- No interim solution states to maintain
- No incremental package version coordination
- No phased deployment complexity

### All-At-Once Strategy Principles

#### Simultaneity
All projects migrate together in **one pass**:
- All TargetFramework properties updated to `net10.0`
- All package versions updated to .NET 10.0-compatible versions
- Single restore → build → fix → verify cycle
- No intermediate working states

#### Atomic Operation Structure
```
[Start: All projects on .NET 8.0]
    ↓
[Update all project files + all packages]
    ↓
[Restore dependencies for entire solution]
    ↓
[Build solution - identify ALL compilation errors]
    ↓
[Fix ALL compilation errors using breaking changes catalog]
    ↓
[Rebuild solution - verify 0 errors]
    ↓
[Run all test projects - verify functionality]
    ↓
[End: All projects on .NET 10.0, tests passing]
```

This is **ONE task**, not sequential tasks. Operations are combined because:
- Cannot test project updates without package updates
- Cannot verify package updates without building
- Compilation errors only appear after building with new packages
- Splitting creates artificial checkpoints that don't provide value

#### Dependency-Based Understanding (Not Sequencing)

While execution is atomic, understanding dependency order helps predict:

1. **Compilation Order**: MSBuild will compile:
   - ServiceDefaults first (no dependencies)
   - Then Backend, IdentityServer, StaffWebUI (depend on ServiceDefaults)
   - Then CustomerWebUI, Evaluator, EvaluationTests (depend on Backend)
   - Then AppHost, DataIngestor (depend on Level 2 projects)
   - Finally E2ETest (depends on AppHost)

2. **Error Propagation**: Errors in foundation projects (ServiceDefaults) may cause cascading errors in dependent projects. Fix foundation issues first.

3. **Testing Strategy**: Test in dependency order:
   - Unit tests in EvaluationTests (tests Backend/Evaluator)
   - Integration tests in E2ETest (tests entire AppHost orchestration)

### Risk Mitigation for All-At-Once

#### Risk: Large Blast Radius
**Mitigation**: 
- Comprehensive breaking changes catalog prepared in advance
- Single security vulnerability identified and remediation planned
- All package versions researched and validated
- Work performed in dedicated upgrade branch (`upgrade-to-NET10`)
- Can roll back entire operation if critical issues found

#### Risk: All Compilation Errors at Once
**Mitigation**:
- Breaking changes catalog organized by:
  - Binary incompatible APIs (8 occurrences) - highest priority
  - Source incompatible APIs (51 occurrences) - code changes needed
  - Behavioral changes (52 occurrences) - runtime verification needed
- Fix in order: binary → source → behavioral
- Reference documentation links provided for each breaking change

#### Risk: Testing Large Surface Area
**Mitigation**:
- Two test projects provide validation:
  - EvaluationTests: Unit/integration tests for Backend/Evaluator
  - E2ETest: End-to-end tests via AppHost orchestration
- Aspire AppHost enables easy local testing of all services
- Smoke test checklist for manual validation

#### Risk: Merge Conflicts if Parallel Development
**Mitigation**:
- Upgrade performed in dedicated branch (`upgrade-to-NET10`)
- Coordinate with team to minimize changes to `upgrade` branch during upgrade window
- Single atomic commit of all changes (preferred) minimizes conflict surface

### Execution Phases

While this is an atomic operation, logical phases organize the work:

**Phase 0: Preparation** (if needed)
- ✅ Verify .NET 10.0 SDK installed
- ✅ Update global.json if present and incompatible
- ✅ Switched to upgrade branch `upgrade-to-NET10`

**Phase 1: Atomic Upgrade** (single coordinated operation)
- Update all project files to `net10.0`
- Update all package references (see Package Update Reference)
- Restore dependencies
- Build solution and fix ALL compilation errors
- **Deliverable**: Solution builds with 0 errors

**Phase 2: Test Validation**
- Execute EvaluationTests
- Execute E2ETest
- Address any test failures
- **Deliverable**: All tests pass

**Phase 3: Manual Validation** (optional but recommended)
- Start AppHost and verify services start
- Smoke test key workflows in CustomerWebUI
- Smoke test key workflows in StaffWebUI
- Verify AI evaluation pipeline in Evaluator/DataIngestor
- **Deliverable**: Core functionality validated

### Success Criteria

The All-At-Once migration succeeds when:
- ✅ All 12 projects target `net10.0`
- ✅ All package updates applied (19 packages upgraded)
- ✅ Security vulnerability remediated (SemanticKernel.Core → 1.72.0)
- ✅ Solution builds with 0 errors
- ✅ Solution builds with 0 warnings (stretch goal)
- ✅ EvaluationTests pass (all tests)
- ✅ E2ETest pass (all tests)
- ✅ AppHost successfully orchestrates all services
- ✅ No package dependency conflicts
- ✅ All changes committed to `upgrade-to-NET10` branch

### Rollback Strategy

If critical blocking issues discovered:
1. **Before commit**: Use `git restore .` to discard all changes
2. **After commit**: Use `git reset --hard HEAD~1` to undo commit
3. **After push**: Create revert commit or delete branch and restart

Blocking issues that warrant rollback:
- Unable to resolve compilation errors after reasonable effort
- Critical functionality broken with no clear fix
- Performance degradation >50% with no mitigation
- Security issues introduced by .NET 10.0 upgrade itself

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has a clean 5-level dependency hierarchy with **no circular dependencies**, making it ideal for All-At-Once strategy. The foundation ServiceDefaults library affects 7 projects, creating natural synchronization points.

```
Level 0 (Foundation - no project dependencies)
├── ServiceDefaults.csproj (Used by 7 projects) ⭐ CRITICAL PATH
├── DataGenerator.csproj (Standalone utility)
└── PythonInference.pyproj (Python project, minimal changes)

Level 1 (Depends on Level 0 only)
├── Backend.csproj → ServiceDefaults (Used by 5 projects) ⭐ CRITICAL PATH
├── IdentityServer.csproj → ServiceDefaults
└── StaffWebUI.csproj → ServiceDefaults

Level 2 (Depends on Levels 0-1)
├── CustomerWebUI.csproj → ServiceDefaults, Backend
├── Evaluator.csproj → ServiceDefaults, Backend
└── EvaluationTests.csproj → ServiceDefaults, Backend

Level 3 (Depends on Levels 0-2)
├── AppHost.csproj → Backend, CustomerWebUI, IdentityServer, StaffWebUI ⭐ ORCHESTRATOR
└── DataIngestor.csproj → Backend, Evaluator

Level 4 (Depends on Levels 0-3)
└── E2ETest.csproj → ServiceDefaults, AppHost
```

### Project Groupings for All-At-Once Migration

Since this is an **All-At-Once** strategy, all projects are updated simultaneously. However, understanding dependency relationships helps predict compilation order and testing strategy:

**Group 1: Foundation Layer** (Level 0)
- **ServiceDefaults.csproj** - Shared library used by 7 projects
  - Issues: 26 (1 mandatory, 24 potential, 1 optional)
  - Impact: Changes here propagate to all dependent projects
  - Priority: Update first logically (though all updated atomically)

**Group 2: Core Services** (Levels 1-2)
- **Backend.csproj** - Core API service (used by 5 projects)
  - Issues: 19 (1 mandatory, 18 potential)
  - Dependencies: ServiceDefaults
  - Impact: High - affects CustomerWebUI, Evaluator, DataIngestor, EvaluationTests

- **IdentityServer.csproj** - Authentication service
  - Issues: 1 (1 mandatory)
  - Dependencies: ServiceDefaults
  - Impact: Required by AppHost

- **StaffWebUI.csproj** - Blazor UI for staff
  - Issues: 30 (1 mandatory, 27 potential, 2 optional)
  - Dependencies: ServiceDefaults
  - Impact: Required by AppHost

- **CustomerWebUI.csproj** - Razor Pages UI for customers
  - Issues: 21 (1 mandatory, 20 potential)
  - Dependencies: ServiceDefaults, Backend
  - Impact: Required by AppHost

- **Evaluator.csproj** - AI evaluation service
  - Issues: 11 (1 mandatory, 10 potential)
  - Dependencies: ServiceDefaults, Backend
  - Impact: Used by DataIngestor

**Group 3: Application Layer** (Level 3)
- **AppHost.csproj** - Aspire orchestration host
  - Issues: 11 (1 mandatory, 10 potential)
  - Dependencies: Backend, CustomerWebUI, IdentityServer, StaffWebUI
  - Impact: Required by E2ETest, orchestrates all services

- **DataIngestor.csproj** - Data processing service
  - Issues: 5 (1 mandatory, 2 potential, 2 optional)
  - Dependencies: Backend, Evaluator
  - Impact: Contains security vulnerability (Microsoft.SemanticKernel.Core)

**Group 4: Testing & Utilities** (Levels 0, 2, 4)
- **E2ETest.csproj** - End-to-end tests (Level 4)
  - Issues: 5 (1 mandatory, 4 potential)
  - Dependencies: ServiceDefaults, AppHost

- **EvaluationTests.csproj** - Evaluation unit tests (Level 2)
  - Issues: 19 (9 mandatory, 10 potential)
  - Dependencies: ServiceDefaults, Backend

- **DataGenerator.csproj** - Seed data utility (Level 0)
  - Issues: 9 (1 mandatory, 8 potential)
  - Dependencies: None

- **PythonInference.pyproj** - Python inference (Level 0)
  - Issues: 1 (1 mandatory - target framework change from net472)
  - Dependencies: None

### Critical Path Identification

**Primary Critical Path**: ServiceDefaults → Backend → CustomerWebUI → AppHost → E2ETest
- This path touches the most projects
- ServiceDefaults changes ripple through entire solution
- AppHost orchestrates all services and must be working for E2E tests

**Secondary Critical Path**: ServiceDefaults → Backend → Evaluator → DataIngestor
- Includes the project with security vulnerability
- AI evaluation pipeline must remain functional

### All-At-Once Execution Implications

Since all projects update atomically:
1. **Update all TargetFramework properties** across all 12 projects simultaneously
2. **Update all package references** across all projects in one operation
3. **Restore dependencies** for entire solution
4. **Build entire solution** - compilation errors will appear from all projects
5. **Fix all compilation errors** identified (using breaking changes catalog)
6. **Rebuild entire solution** to verify all fixes applied
7. **Run test projects** (EvaluationTests, E2ETest) to validate functionality

**No intermediate states** - solution goes from fully .NET 8.0 to fully .NET 10.0 in one operation.

### Circular Dependency Analysis
✅ **No circular dependencies detected** - Clean dependency graph enables smooth upgrade.

---

## Package Update Reference

Consolidated view of all package updates across projects, organized by scope.

### Common Package Updates (affecting multiple projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| **Aspire Packages** (all deprecated, upgrade required) |
| Aspire.Hosting.AppHost | 8.2.2 | 13.1.2 | 1 (AppHost) | Aspire major version alignment with .NET 10.0 |
| Aspire.Hosting.Azure.Storage | 8.2.2 | 13.1.2 | 1 (AppHost) | Aspire major version alignment |
| Aspire.Hosting.PostgreSQL | 8.2.2 | 13.1.2 | 1 (AppHost) | Aspire major version alignment |
| Aspire.Hosting.Qdrant | 8.2.2 | 13.1.2 | 1 (AppHost) | Aspire major version alignment |
| Aspire.Hosting.Redis | 8.2.2 | 13.1.2 | 1 (AppHost) | Aspire major version alignment |
| Aspire.Hosting.Testing | 8.2.2 | 13.1.2 | 1 (E2ETest) | Aspire major version alignment |
| Aspire.Azure.Storage.Blobs | 8.2.2 | 13.1.2 | 1 (Backend) | Aspire major version alignment |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 8.2.2 | 13.1.2 | 1 (Backend) | Aspire major version alignment |
| Aspire.StackExchange.Redis | 8.2.2 | 13.1.2 | 1 (Backend) | Aspire major version alignment |
| **Microsoft.Extensions Packages** |
| Microsoft.Extensions.ServiceDiscovery | 8.2.2 | 10.3.0 | 1 (ServiceDefaults) | Deprecated; framework alignment |
| Microsoft.Extensions.Http.Resilience | 8.10.0 | 10.3.0 | 2 (ServiceDefaults, DataIngestor) | Framework alignment |
| Microsoft.Extensions.Configuration.Json | 8.0.1 | 10.0.3 | 1 (DataGenerator) | Framework alignment |
| Microsoft.Extensions.Hosting | 8.0.1 | 10.0.3 | 1 (DataGenerator) | Framework alignment |
| **ASP.NET Core Packages** |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.10 | 10.0.3 | 1 (Backend) | Framework alignment; breaking API changes |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 8.0.10 | 10.0.3 | 1 (CustomerWebUI) | Framework alignment |
| Microsoft.AspNetCore.Components.QuickGrid | 8.0.10 | 10.0.3 | 1 (StaffWebUI) | Framework alignment |
| **OpenTelemetry Packages** |
| OpenTelemetry.Instrumentation.AspNetCore | 1.9.0 | 1.15.0 | 1 (ServiceDefaults) | .NET 10.0 compatibility |
| OpenTelemetry.Instrumentation.Http | 1.9.0 | 1.15.0 | 1 (ServiceDefaults) | .NET 10.0 compatibility |

### Security-Critical Updates

| Package | Current | Target | Projects Affected | CVE / Advisory |
|---------|---------|--------|-------------------|----------------|
| **Microsoft.SemanticKernel.Core** | **1.25.0** | **1.72.0** | 1 (DataIngestor) | **🔒 Security Vulnerability** (details in assessment) |

### Project-Specific Package Updates

**ServiceDefaults** (4 packages upgraded):
- Microsoft.Extensions.ServiceDiscovery: 8.2.2 → 10.3.0
- Microsoft.Extensions.Http.Resilience: 8.10.0 → 10.3.0
- OpenTelemetry.Instrumentation.AspNetCore: 1.9.0 → 1.15.0
- OpenTelemetry.Instrumentation.Http: 1.9.0 → 1.15.0

**Backend** (4 packages upgraded):
- Aspire.Azure.Storage.Blobs: 8.2.2 → 13.1.2
- Aspire.Npgsql.EntityFrameworkCore.PostgreSQL: 8.2.2 → 13.1.2
- Aspire.StackExchange.Redis: 8.2.2 → 13.1.2
- Microsoft.AspNetCore.Authentication.JwtBearer: 8.0.10 → 10.0.3

**StaffWebUI** (1 package upgraded):
- Microsoft.AspNetCore.Components.QuickGrid: 8.0.10 → 10.0.3

**CustomerWebUI** (1 package upgraded):
- Microsoft.AspNetCore.Authentication.OpenIdConnect: 8.0.10 → 10.0.3

**AppHost** (5 packages upgraded):
- All Aspire.Hosting.* packages: 8.2.2 → 13.1.2

**DataIngestor** (2 packages upgraded):
- Microsoft.Extensions.Http.Resilience: 8.10.0 → 10.3.0
- Microsoft.SemanticKernel.Core: 1.25.0 → 1.72.0 (security fix)

**DataGenerator** (2 packages upgraded):
- Microsoft.Extensions.Configuration.Json: 8.0.1 → 10.0.3
- Microsoft.Extensions.Hosting: 8.0.1 → 10.0.3

**E2ETest** (1 package upgraded):
- Aspire.Hosting.Testing: 8.2.2 → 13.1.2

**No packages to upgrade**:
- IdentityServer (all packages compatible)
- Evaluator (no packages)
- EvaluationTests (all packages compatible)
- PythonInference (no packages)

### Compatible Packages (No Update Required)

These packages remain at current versions (compatible with .NET 10.0):
- Azure.AI.OpenAI 2.1.0-beta.1
- Duende.IdentityServer 7.0.8
- IdentityModel 7.0.0
- Markdown2Pdf 2.2.1
- Microsoft.Extensions.AI (all sub-packages) 9.4.4-preview.1.25259.16
- Microsoft.FluentUI.AspNetCore.Components (all variants) 4.10.3
- Microsoft.ML.Tokenizers (all variants) 1.0.1
- Microsoft.NET.Test.Sdk 17.11.1
- Microsoft.Playwright 1.48.0
- Microsoft.SemanticKernel.Connectors.Qdrant 1.16.0-alpha
- OpenTelemetry.Exporter.OpenTelemetryProtocol 1.9.0
- OpenTelemetry.Extensions.Hosting 1.9.0
- OpenTelemetry.Instrumentation.Runtime 1.9.0
- PdfPig 0.1.9
- Serilog.AspNetCore 8.0.3
- SmartComponents.* (all variants) 0.1.0-preview10148
- StatefulReconnection 0.1.0
- xunit 2.9.2
- xunit.runner.visualstudio 2.8.2

---

## Breaking Changes Catalog

Comprehensive catalog of .NET 8.0 → .NET 10.0 breaking changes identified in this solution.

### Binary Incompatible APIs (Mandatory Fixes)

**8 occurrences** - APIs changed in a way that prevents binaries from loading; requires code changes.

| API | Location | Project | Severity | Action Required |
|-----|----------|---------|----------|-----------------|
| *Binary incompatible APIs in EvaluationTests* | Various test files | EvaluationTests | Mandatory | Review compilation errors; update test code to use .NET 10.0 APIs |

**Note**: Specific binary incompatible APIs will be identified during build; likely in Microsoft.Extensions.AI.Evaluation framework.

### Source Incompatible APIs (Code Changes Required)

**51 occurrences** - APIs require code changes to compile.

#### JWT Bearer Authentication (4 occurrences)

| API | Location | Project | Fix Required |
|-----|----------|---------|--------------|
| `JwtBearerExtensions.AddJwtBearer` | `Program.cs:35` | Backend | Update method signature for .NET 10.0 |
| `JwtBearerOptions.Authority` | `Program.cs:37` | Backend | Update property usage |
| `JwtBearerOptions.TokenValidationParameters` | `Program.cs:38` | Backend | Update property usage |
| `Microsoft.Extensions.DependencyInjection.JwtBearerExtensions` | `Program.cs:35` | Backend | Update extension method usage |

**Documentation**: https://go.microsoft.com/fwlink/?linkid=2262679

#### TensorPrimitives (2 occurrences)

| API | Location | Project | Fix Required |
|-----|----------|---------|--------------|
| `TensorPrimitives.CosineSimilarity` | `Api/CatalogApi.cs:44` | Backend | Update method signature (ReadOnlySpan parameters changed) |

#### TimeSpan (1 occurrence)

| API | Location | Project | Fix Required |
|-----|----------|---------|--------------|
| `TimeSpan.FromSeconds` | `Data/AppDbContext.cs:36` | Backend | Update method signature |

#### Azure OpenAI ApiKeyCredential (2 occurrences)

| API | Location | Project | Fix Required |
|-----|----------|---------|--------------|
| `System.ClientModel.ApiKeyCredential` | `Clients/ChatCompletion/ServiceCollectionChatClientExtensions.cs:85` | ServiceDefaults | Update constructor for .NET 10.0 Azure SDK |
| `ApiKeyCredential.#ctor(string)` | `Clients/ChatCompletion/ServiceCollectionChatClientExtensions.cs:85` | ServiceDefaults | Update constructor call |

#### Blazor Component APIs (21 occurrences)

| API | Location | Project | Fix Required |
|-----|----------|---------|--------------|
| *Various Blazor component APIs* | Multiple `.razor` files | StaffWebUI | Update component parameter binding, event callbacks, rendering |

**Common Blazor Changes**:
- Component parameter binding syntax
- Event callback signatures
- Cascade parameter usage
- Rendering fragment patterns

#### Razor Pages APIs (15 occurrences)

| API | Location | Project | Fix Required |
|-----|----------|---------|--------------|
| *Various Razor Pages APIs* | Multiple `.cshtml` files | CustomerWebUI | Update page model binding, tag helpers, view components |

**Common Razor Pages Changes**:
- Model binding patterns
- Tag helper usage
- View component invocation

#### Other Source Incompatible APIs (6 occurrences)

- DataGenerator: 4 occurrences (not specified; identify during build)
- Evaluator: 2 occurrences (not specified; identify during build)

### Behavioral Changes (Runtime Behavior May Differ)

**52 occurrences** - Code compiles but behavior may differ at runtime.

#### HttpContent (Multiple occurrences)

| API | Affected Projects | Behavioral Change |
|-----|------------------|-------------------|
| `HttpContent.ReadFromJsonAsync` | ServiceDefaults, Backend | JSON deserialization behavior may differ |
| `HttpContent.ReadAsStreamAsync` | ServiceDefaults, Backend | Stream handling may differ |

**Action**: Verify HTTP client behavior in tests; no code changes expected.

#### System.Uri (Multiple occurrences)

| API | Affected Projects | Behavioral Change |
|-----|------------------|-------------------|
| `Uri` constructor | ServiceDefaults, Backend | Uri parsing or validation may differ |

**Action**: Verify Uri handling; no code changes expected.

#### General Behavioral Changes

- **ServiceDefaults**: 18 behavioral changes (mostly HttpContent, Uri)
- **Backend**: 4 behavioral changes (HttpContent, Uri)
- **StaffWebUI**: 4 behavioral changes (Blazor rendering)
- **CustomerWebUI**: 4 behavioral changes (Razor Pages rendering)
- **Evaluator**: 8 behavioral changes (AI evaluation logic)
- **EvaluationTests**: 10 behavioral changes (test assertions)
- **E2ETest**: 2 behavioral changes (Playwright test execution)
- **DataGenerator**: 2 behavioral changes (data generation)

**General Action**: Run tests after upgrade; verify behavior matches expectations; update test assertions if behavior legitimately changed in .NET 10.0.

### Breaking Changes by Category

| Category | Count | Severity | Projects Affected |
|----------|-------|----------|-------------------|
| **Binary Incompatible** | 8 | Mandatory | EvaluationTests |
| **JWT Authentication** | 4 | Mandatory | Backend |
| **Blazor Components** | 21 | Source Incompatible | StaffWebUI |
| **Razor Pages** | 15 | Source Incompatible | CustomerWebUI |
| **Azure SDK** | 2 | Source Incompatible | ServiceDefaults |
| **TensorPrimitives** | 2 | Source Incompatible | Backend |
| **HttpContent Behavioral** | Multiple | Behavioral | ServiceDefaults, Backend |
| **Uri Behavioral** | Multiple | Behavioral | ServiceDefaults, Backend |
| **AI Evaluation** | 8 | Behavioral | Evaluator |
| **Other** | 6 | Source Incompatible | DataGenerator, Evaluator |

### Breaking Changes Documentation

- **.NET 10.0 Breaking Changes**: https://go.microsoft.com/fwlink/?linkid=2262679
- **Aspire 13.x Migration**: https://aka.ms/dotent/aspire/support
- **ASP.NET Core Breaking Changes**: https://go.microsoft.com/fwlink/?linkid=2262679 (filter for ASP.NET Core)

### Breaking Change Resolution Strategy

1. **Build First**: Run `dotnet build` to identify all compilation errors
2. **Fix by Severity**:
   - **Mandatory**: Binary incompatible (8) and source incompatible (51) first
   - **Behavioral**: Test and verify after compilation succeeds
3. **Fix by Project**:
   - Start with Level 0 (ServiceDefaults, DataGenerator, PythonInference)
   - Proceed through dependency levels (1 → 2 → 3 → 4)
   - Foundation fixes may resolve cascading errors in dependent projects
4. **Use Breaking Changes Catalog**: Reference specific API fixes above
5. **Consult Documentation**: Use provided links for detailed guidance

---

---

## Project-by-Project Migration Plans

The following sections detail migration specifications for each project. Projects are organized by dependency level.

### Level 0: ServiceDefaults.csproj (Foundation Library)

**Current State**:
- Target Framework: net8.0
- Project Type: ClassLibrary
- Files: 11
- Dependencies: None
- Used By: 7 projects (Backend, CustomerWebUI, Evaluator, IdentityServer, StaffWebUI, E2ETest, EvaluationTests)
- Packages: 12 total (4 require upgrade)
- Issues: 26 (1 mandatory, 24 potential, 1 optional)

**Target State**:
- Target Framework: net10.0
- Packages: 12 total (8 compatible, 4 upgraded)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.Extensions.ServiceDiscovery | 8.2.2 | 10.3.0 | **Deprecated** - Out of support; upgrade required |
| Microsoft.Extensions.Http.Resilience | 8.10.0 | 10.3.0 | Framework alignment |
| OpenTelemetry.Instrumentation.AspNetCore | 1.9.0 | 1.15.0 | .NET 10.0 compatibility |
| OpenTelemetry.Instrumentation.Http | 1.9.0 | 1.15.0 | .NET 10.0 compatibility |
| Azure.AI.OpenAI | 2.1.0-beta.1 | *(no change)* | ✅ Compatible |
| IdentityModel | 7.0.0 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI.Ollama | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI.OpenAI | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.9.0 | *(no change)* | ✅ Compatible |
| OpenTelemetry.Extensions.Hosting | 1.9.0 | *(no change)* | ✅ Compatible |
| OpenTelemetry.Instrumentation.Runtime | 1.9.0 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**API Behavioral Changes** (18 occurrences - runtime behavior may differ):
- `System.Net.Http.HttpContent` - Behavioral changes in HttpContent handling
  - Files affected: `Clients/PythonInference/PythonInferenceClient.cs`, `Clients/Backend/StaffBackendClient.cs`
  - **Impact**: Methods like `ReadFromJsonAsync`, `ReadAsStreamAsync` may behave differently
  - **Action**: No code changes required; verify behavior in tests

- `System.Uri` - Constructor and usage behavioral changes
  - Files affected: `Clients/ChatCompletion/ServiceCollectionChatClientExtensions.cs`
  - **Impact**: Uri parsing or validation may differ
  - **Action**: No code changes required; verify Uri handling in tests

**Source Incompatible APIs** (2 occurrences - code changes required):
- `System.ClientModel.ApiKeyCredential` - Constructor signature changed
  - File: `Clients/ChatCompletion/ServiceCollectionChatClientExtensions.cs:85`
  - **Current**: `new ApiKeyCredential(apiKey)`
  - **Required Change**: Review Azure.AI.OpenAI migration guide for ApiKeyCredential replacement
  - **Likely Fix**: Use updated constructor or alternative credential type
  - Documentation: https://go.microsoft.com/fwlink/?linkid=2262679

4. **Code Modifications**

**File**: `src/ServiceDefaults/Clients/ChatCompletion/ServiceCollectionChatClientExtensions.cs`
- **Line 85**: Update `new ApiKeyCredential(apiKey)` to use .NET 10.0-compatible credential API
- **Context**: Azure OpenAI client initialization
- **Verification**: Ensure Azure OpenAI chat client still authenticates correctly

5. **Testing Strategy**
- **Build Test**: ServiceDefaults must compile without errors (foundation for 7 projects)
- **Integration Test**: Verify clients (StaffBackendClient, PythonInferenceClient, Chat clients) still function
- **Dependent Projects**: Changes here will ripple to all 7 dependent projects; test after they upgrade

6. **Validation Checklist**
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] ApiKeyCredential usage updated and compiles
- [ ] No package dependency conflicts
- [ ] Dependent projects (Backend, StaffWebUI, etc.) still compile after this change

**Risk Level**: Medium-High (foundation library affects 7 projects)

---

### Level 0: DataGenerator.csproj (Seed Data Utility)

**Current State**:
- Target Framework: net8.0
- Project Type: DotNetCoreApp
- Files: 23
- Dependencies: None
- Used By: None (standalone utility)
- Packages: 7 total (2 require upgrade)
- Issues: 9 (1 mandatory, 8 potential)

**Target State**:
- Target Framework: net10.0
- Packages: 7 total (5 compatible, 2 upgraded)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.Extensions.Configuration.Json | 8.0.1 | 10.0.3 | Framework alignment |
| Microsoft.Extensions.Hosting | 8.0.1 | 10.0.3 | Framework alignment |
| Azure.AI.OpenAI | 2.1.0-beta.1 | *(no change)* | ✅ Compatible |
| Markdown2Pdf | 2.2.1 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI.OpenAI | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| SmartComponents.LocalEmbeddings | 0.1.0-preview10148 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Source Incompatible APIs** (4 occurrences):
- Details not specified in assessment; expect minor API signature changes
- **Action**: Review compilation errors after build and apply fixes based on error messages

**Behavioral Changes** (2 occurrences):
- Runtime behavior may differ in .NET 10.0
- **Action**: Verify seed data generation produces expected results

4. **Code Modifications**
- None preemptively identified
- Address compilation errors as they appear during build

5. **Testing Strategy**
- **Build Test**: DataGenerator must compile without errors
- **Functional Test**: Run data generation utility and verify output
- **Isolation**: Standalone utility; failures don't block other projects

6. **Validation Checklist**
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] Data generation utility runs successfully
- [ ] Generated seed data is valid

**Risk Level**: Low (standalone utility, no dependents)

---

### Level 0: PythonInference.pyproj (Python Inference Service)

**Current State**:
- Target Framework: net472
- Project Type: ClassicDotNetApp (Python project)
- Files: 3
- Dependencies: None
- Used By: None
- Packages: 0
- Issues: 1 (1 mandatory - target framework change)

**Target State**:
- Target Framework: net10.0
- Packages: 0

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net472</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
   - **Note**: This is a Python project (.pyproj); target framework change is primarily symbolic

2. **Package Updates**
   - No packages to update

3. **Expected Breaking Changes**
   - **None expected**: Python code doesn't directly use .NET APIs
   - Target framework change is a project metadata update

4. **Code Modifications**
   - **None required**: Python scripts remain unchanged

5. **Testing Strategy**
   - **Build Test**: Verify project loads in Visual Studio with new TFM
   - **Runtime Test**: Verify Python inference service still executes (if used)

6. **Validation Checklist**
   - [ ] Project builds/loads without errors
   - [ ] Python scripts execute correctly (if applicable)

**Risk Level**: Very Low (Python project, minimal .NET integration)

---

### Level 1: Backend.csproj (Core API Service)

**Current State**:
- Target Framework: net8.0
- Project Type: AspNetCore
- Files: 19
- Dependencies: ServiceDefaults
- Used By: 5 projects (AppHost, CustomerWebUI, DataIngestor, Evaluator, EvaluationTests)
- Packages: 6 total (4 require upgrade)
- Issues: 19 (1 mandatory, 15 potential, 3 optional)

**Target State**:
- Target Framework: net10.0
- Packages: 6 total (2 compatible, 4 upgraded)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Aspire.Azure.Storage.Blobs | 8.2.2 | 13.1.2 | **Deprecated** - Aspire upgrade required |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 8.2.2 | 13.1.2 | **Deprecated** - Aspire upgrade required |
| Aspire.StackExchange.Redis | 8.2.2 | 13.1.2 | **Deprecated** - Aspire upgrade required |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.10 | 10.0.3 | Framework alignment |
| Microsoft.SemanticKernel.Connectors.Qdrant | 1.16.0-alpha | *(no change)* | ✅ Compatible |
| SmartComponents.LocalEmbeddings.SemanticKernel | 0.1.0-preview10148 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Source Incompatible APIs** (7 occurrences - code changes required):

- **JWT Bearer Authentication** (4 occurrences in `Program.cs`):
  - `JwtBearerExtensions.AddJwtBearer` - Method signature changed
  - `JwtBearerOptions.Authority` - Property changed
  - `JwtBearerOptions.TokenValidationParameters` - Property changed
  - **File**: `src/Backend/Program.cs:35-38`
  - **Action**: Update JWT Bearer authentication configuration for .NET 10.0
  - **Documentation**: https://go.microsoft.com/fwlink/?linkid=2262679

- **TensorPrimitives.CosineSimilarity** (2 occurrences in `Api/CatalogApi.cs:44`):
  - Method signature changed in .NET 10.0
  - **File**: `src/Backend/Api/CatalogApi.cs`
  - **Action**: Update CosineSimilarity call to use new signature
  - **Context**: Product search similarity calculation

- **TimeSpan.FromSeconds** (1 occurrence in `Data/AppDbContext.cs:36`):
  - Method signature changed
  - **File**: `src/Backend/Data/AppDbContext.cs`
  - **Action**: Update TimeSpan.FromSeconds call
  - **Context**: Retry policy configuration

**Behavioral Changes** (4 occurrences):
- `System.Net.Http.HttpContent` - Runtime behavior may differ
  - File: `Services/ProductManualSemanticSearch.cs:37`
  - **Action**: Verify HttpContent.ReadFromJsonAsync behavior in tests

- `System.Uri` - Constructor behavioral changes (2 occurrences in `Program.cs:27`)
  - **Action**: Verify Uri parsing for HttpClient base addresses

4. **Code Modifications**

**File**: `src/Backend/Program.cs`
- **Lines 35-38**: Update JWT Bearer authentication configuration
  - Review Microsoft.AspNetCore.Authentication.JwtBearer 10.0.3 API changes
  - Update `AddJwtBearer` method call and options configuration

**File**: `src/Backend/Api/CatalogApi.cs`
- **Line 44**: Update `TensorPrimitives.CosineSimilarity` call to .NET 10.0 signature

**File**: `src/Backend/Data/AppDbContext.cs`
- **Line 36**: Update `TimeSpan.FromSeconds` call to .NET 10.0 signature

5. **Testing Strategy**
- **Build Test**: Backend must compile without errors (used by 5 projects)
- **Unit Tests**: Run EvaluationTests (depends on Backend)
- **Integration Test**: Verify JWT authentication still works
- **Functional Test**: 
  - Test product search with semantic similarity (CatalogApi change)
  - Test database retry policy (AppDbContext change)
  - Test Qdrant vector search (SemanticKernel integration)

6. **Validation Checklist**
- [ ] Project builds without errors
- [ ] JWT Bearer authentication compiles and functions
- [ ] TensorPrimitives.CosineSimilarity compiles
- [ ] TimeSpan.FromSeconds compiles
- [ ] EvaluationTests pass
- [ ] API endpoints respond correctly
- [ ] Azure Storage, PostgreSQL, Redis connections work
- [ ] No package dependency conflicts

**Risk Level**: Medium (Core API used by 5 projects; JWT auth changes require careful validation)

---

### Level 1: IdentityServer.csproj (Authentication Service)

**Current State**:
- Target Framework: net8.0
- Project Type: AspNetCore
- Files: 81
- Dependencies: ServiceDefaults
- Used By: AppHost
- Packages: 2 total (all compatible)
- Issues: 1 (1 mandatory - target framework only)

**Target State**:
- Target Framework: net10.0
- Packages: 2 total (all compatible)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| Duende.IdentityServer | 7.0.8 | *(no change)* | ✅ Compatible |
| Serilog.AspNetCore | 8.0.3 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**
   - **None**: Only target framework change required

4. **Code Modifications**
   - **None required**: No API breaking changes detected

5. **Testing Strategy**
   - **Build Test**: IdentityServer must compile without errors
   - **Integration Test**: Verify authentication flows work with AppHost
   - **Functional Test**: Test token issuance and validation

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] IdentityServer starts successfully
   - [ ] Token issuance works
   - [ ] Backend and CustomerWebUI can authenticate

**Risk Level**: Low (only TFM change, Duende.IdentityServer 7.0.8 compatible with .NET 10.0)

---

### Level 1: StaffWebUI.csproj (Blazor UI for Staff)

**Current State**:
- Target Framework: net8.0
- Project Type: AspNetCore
- Files: 98
- Dependencies: ServiceDefaults
- Used By: AppHost
- Packages: 7 total (1 requires upgrade)
- Issues: 30 (1 mandatory, 28 potential, 1 optional)

**Target State**:
- Target Framework: net10.0
- Packages: 7 total (6 compatible, 1 upgraded)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| Microsoft.AspNetCore.Components.QuickGrid | 8.0.10 | 10.0.3 | Framework alignment |
| Aspire.StackExchange.Redis | 8.2.2 | *(no change)* | ✅ Compatible |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 8.0.10 | *(no change)* | ✅ Compatible |
| Microsoft.FluentUI.AspNetCore.Components | 4.10.3 | *(no change)* | ✅ Compatible |
| Microsoft.FluentUI.AspNetCore.Components.DataGrid.EntityFrameworkAdapter | 4.10.3 | *(no change)* | ✅ Compatible |
| Microsoft.FluentUI.AspNetCore.Components.Icons | 4.10.3 | *(no change)* | ✅ Compatible |
| StatefulReconnection | 0.1.0 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Source Incompatible APIs** (21 occurrences - largest Blazor project):
- Blazor component API changes in .NET 10.0
- **Impact**: Component lifecycle methods, parameter passing, or rendering may have changed
- **Action**: Review compilation errors after build and update component code
- **Common Changes**: 
  - Component parameter binding
  - Event callbacks
  - Cascade parameters
  - Rendering fragments

**Behavioral Changes** (4 occurrences):
- Runtime behavior differences in Blazor components
- **Action**: Verify UI rendering and interactivity in browser testing

4. **Code Modifications**
- **98 files**: Large Blazor project; expect changes across multiple components
- **Approach**: Build first, then address compilation errors systematically
- **Focus Areas**:
  - `.razor` components with complex parameter bindings
  - Components using QuickGrid (package upgraded to 10.0.3)
  - Components with FluentUI controls
  - Authentication/authorization components (OpenIdConnect)

5. **Testing Strategy**
   - **Build Test**: StaffWebUI must compile without errors
   - **UI Test**: Manual browser testing of key workflows
   - **Component Test**: Verify major Blazor components render correctly
   - **Authentication Test**: Verify OpenIdConnect authentication flow
   - **Data Grid Test**: Verify QuickGrid components work with 10.0.3

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] StaffWebUI starts and loads in browser
   - [ ] Authentication works (OpenIdConnect login)
   - [ ] QuickGrid components display data correctly
   - [ ] FluentUI components render properly
   - [ ] Key user workflows function (navigation, forms, data display)

**Risk Level**: Medium (Largest Blazor project with 21 source incompatible APIs; UI changes require manual validation)

---

### Level 2: CustomerWebUI.csproj (Razor Pages UI for Customers)

**Current State**:
- Target Framework: net8.0
- Project Type: AspNetCore
- Files: 48
- Dependencies: ServiceDefaults, Backend
- Used By: AppHost
- Packages: 2 total (1 requires upgrade)
- Issues: 21 (1 mandatory, 20 potential)

**Target State**:
- Target Framework: net10.0
- Packages: 2 total (1 compatible, 1 upgraded)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 8.0.10 | 10.0.3 | Framework alignment |
| SmartComponents.AspNetCore | 0.1.0-preview10148 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Source Incompatible APIs** (15 occurrences):
- Razor Pages API changes in .NET 10.0
- **Common Areas**:
  - Page model binding
  - Tag helpers
  - View components
  - OpenIdConnect authentication middleware
- **Action**: Review compilation errors and update Razor Pages code

**Behavioral Changes** (4 occurrences):
- Runtime behavior differences in Razor Pages
- **Action**: Verify page rendering and form submission in browser testing

4. **Code Modifications**
- **48 files**: Moderate Razor Pages project
- **Focus Areas**:
  - `.cshtml` pages with model binding
  - Pages using SmartComponents
  - Authentication pages (OpenIdConnect upgraded to 10.0.3)
  - Tag helpers and partial views

5. **Testing Strategy**
   - **Build Test**: CustomerWebUI must compile without errors
   - **UI Test**: Manual browser testing of customer workflows
   - **Authentication Test**: Verify OpenIdConnect login flow
   - **Form Test**: Verify form submissions and validation
   - **SmartComponents Test**: Verify SmartComponents.AspNetCore functionality

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] CustomerWebUI starts and loads in browser
   - [ ] OpenIdConnect authentication works
   - [ ] Customer can browse products
   - [ ] Customer can submit support requests
   - [ ] SmartComponents render correctly

**Risk Level**: Low-Medium (Razor Pages with 15 source incompatible APIs; depends on Backend)

---

### Level 2: Evaluator.csproj (AI Evaluation Service)

**Current State**:
- Target Framework: net8.0
- Project Type: DotNetCoreApp
- Files: 2
- Dependencies: ServiceDefaults, Backend
- Used By: DataIngestor
- Packages: 0
- Issues: 11 (1 mandatory, 10 potential)

**Target State**:
- Target Framework: net10.0
- Packages: 0

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**
   - No packages to update (dependencies come from ServiceDefaults and Backend)

3. **Expected Breaking Changes**

**Source Incompatible APIs** (2 occurrences):
- Small service with minimal API surface
- **Action**: Review compilation errors after build

**Behavioral Changes** (8 occurrences):
- AI evaluation logic may behave differently
- **Action**: Verify evaluation results match expectations in tests

4. **Code Modifications**
- **2 files only**: Very small codebase
- Address compilation errors as they appear

5. **Testing Strategy**
   - **Build Test**: Evaluator must compile without errors
   - **Unit Test**: Run EvaluationTests to verify evaluation logic
   - **Integration Test**: Verify DataIngestor can call Evaluator service

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] EvaluationTests pass
   - [ ] Evaluation service responds correctly
   - [ ] AI evaluation metrics are accurate

**Risk Level**: Low (Small 2-file service; comprehensive tests available)

---

### Level 2: EvaluationTests.csproj (Unit Tests)

**Current State**:
- Target Framework: net8.0
- Project Type: DotNetCoreApp
- Files: 6
- Dependencies: ServiceDefaults, Backend
- Used By: None (test project)
- Packages: 8 total (all compatible)
- Issues: 19 (9 mandatory - **8 binary incompatible APIs**, 10 potential)

**Target State**:
- Target Framework: net10.0
- Packages: 8 total (all compatible)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| Microsoft.Extensions.AI.Evaluation | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI.Evaluation.Quality | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| Microsoft.Extensions.AI.Evaluation.Reporting | 9.4.4-preview.1.25259.16 | *(no change)* | ✅ Compatible |
| Microsoft.ML.Tokenizers | 1.0.1 | *(no change)* | ✅ Compatible |
| Microsoft.ML.Tokenizers.Data.O200kBase | 1.0.1 | *(no change)* | ✅ Compatible |
| Microsoft.NET.Test.Sdk | 17.11.1 | *(no change)* | ✅ Compatible |
| xunit | 2.9.2 | *(no change)* | ✅ Compatible |
| xunit.runner.visualstudio | 2.8.2 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Binary Incompatible APIs** (8 occurrences - **HIGHEST MANDATORY COUNT**):
- **Severity**: Mandatory - these APIs changed in a way that prevents binaries from loading
- **Impact**: Test code must be recompiled with updated API usage
- **Common Areas**: 
  - AI evaluation framework APIs (Microsoft.Extensions.AI.Evaluation)
  - Test assertions or framework methods
- **Action**: Review compilation errors and update test code to use .NET 10.0 APIs
- **Documentation**: https://go.microsoft.com/fwlink/?linkid=2262679

**Behavioral Changes** (10 occurrences):
- Test behavior or assertions may differ
- **Action**: Verify all tests still pass; update test expectations if behavior legitimately changed

4. **Code Modifications**
- **6 test files**: Focused test project
- **Priority**: Fix binary incompatible APIs first (compilation will fail)
- **Approach**: 
  1. Build project to identify binary incompatible API errors
  2. Review error messages for specific API changes
  3. Update test code to use .NET 10.0-compatible APIs
  4. Run tests to verify behavioral changes

5. **Testing Strategy**
   - **Build Test**: EvaluationTests must compile without errors
   - **Unit Test Execution**: Run all tests to verify evaluation logic
   - **Test Failure Analysis**: Distinguish between:
     - Test infrastructure failures (need API updates)
     - Actual bugs introduced by .NET 10.0 (investigate further)

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] All 8 binary incompatible API calls updated
   - [ ] All tests run successfully
   - [ ] Test pass rate unchanged (or failures explained)
   - [ ] AI evaluation metrics remain accurate

**Risk Level**: Low-Medium (**Highest mandatory issue count** but isolated to test project; failures non-blocking for production code)

---

### Level 3: AppHost.csproj (Aspire Orchestration Host)

**Current State**:
- Target Framework: net8.0
- Project Type: DotNetCoreApp
- Files: 4
- Dependencies: StaffWebUI, IdentityServer, Backend, CustomerWebUI
- Used By: E2ETest
- Packages: 5 total (all require upgrade)
- Issues: 11 (1 mandatory, 5 potential, 5 optional)

**Target State**:
- Target Framework: net10.0
- Packages: 5 total (all upgraded to 13.1.2)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| Aspire.Hosting.AppHost | 8.2.2 | 13.1.2 | **Deprecated** - Aspire major upgrade |
| Aspire.Hosting.Azure.Storage | 8.2.2 | 13.1.2 | **Deprecated** - Aspire major upgrade |
| Aspire.Hosting.PostgreSQL | 8.2.2 | 13.1.2 | **Deprecated** - Aspire major upgrade |
| Aspire.Hosting.Qdrant | 8.2.2 | 13.1.2 | **Deprecated** - Aspire major upgrade |
| Aspire.Hosting.Redis | 8.2.2 | 13.1.2 | **Deprecated** - Aspire major upgrade |

**Note**: All 5 Aspire packages upgrade from 8.2.2 → 13.1.2 (major version jump)

3. **Expected Breaking Changes**

**Aspire 13.x Breaking Changes**:
- Aspire 8.2.2 → 13.1.2 represents Aspire evolving with .NET 10.0
- **Potential Changes**:
  - Service registration API changes
  - Configuration model updates
  - Resource definition syntax
  - Health check or telemetry configuration
- **Action**: Review Aspire 13.x migration guide
- **Documentation**: https://aka.ms/dotent/aspire/support

**Behavioral Changes** (5 occurrences):
- Orchestration behavior may differ
- Service discovery or startup sequencing changes
- **Action**: Verify AppHost starts all services correctly

4. **Code Modifications**
- **4 files only**: Small AppHost project
- **Focus**: Program.cs or AppHost configuration file
- **Expected Changes**:
  - Update service resource definitions if API changed
  - Update Azure Storage, PostgreSQL, Qdrant, Redis configurations
  - Verify project references still orchestrate correctly

5. **Testing Strategy**
   - **Build Test**: AppHost must compile without errors
   - **Startup Test**: Verify AppHost starts all services:
     - Backend
     - CustomerWebUI
     - StaffWebUI
     - IdentityServer
   - **Service Discovery Test**: Verify services can discover each other
   - **Resource Test**: Verify Azure Storage, PostgreSQL, Qdrant, Redis connections
   - **E2E Test**: Run E2ETest project (depends on AppHost)

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] AppHost starts without exceptions
   - [ ] All 4 dependent services start successfully
   - [ ] Service discovery works
   - [ ] Azure Storage connection works
   - [ ] PostgreSQL connection works
   - [ ] Qdrant connection works
   - [ ] Redis connection works
   - [ ] Aspire dashboard displays correctly
   - [ ] E2ETest can run against AppHost

**Risk Level**: Low (Small project; Aspire well-aligned with .NET versions; breaking changes well-documented)

---

### Level 3: DataIngestor.csproj (Data Processing Service)

**Current State**:
- Target Framework: net8.0
- Project Type: DotNetCoreApp
- Files: 8
- Dependencies: Backend, Evaluator
- Used By: None
- Packages: 5 total (2 require upgrade, 1 security vulnerability)
- Issues: 5 (1 mandatory, 2 potential, 2 optional)

**Target State**:
- Target Framework: net10.0
- Packages: 5 total (3 compatible, 2 upgraded)

**⚠️ SECURITY**: Contains Microsoft.SemanticKernel.Core 1.25.0 with security vulnerability - must upgrade to 1.72.0

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| **Microsoft.SemanticKernel.Core** | **1.25.0** | **1.72.0** | **🔒 SECURITY VULNERABILITY** |
| Microsoft.Extensions.Http.Resilience | 8.10.0 | 10.3.0 | Framework alignment |
| Aspire.Hosting.AppHost | 8.2.2 | *(no change)* | ✅ Compatible |
| PdfPig | 0.1.9 | *(no change)* | ✅ Compatible |
| SmartComponents.LocalEmbeddings | 0.1.0-preview10148 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Security Vulnerability Remediation**:
- **Microsoft.SemanticKernel.Core 1.25.0 → 1.72.0**:
  - **Impact**: Large version jump may include breaking API changes
  - **Action**: Review SemanticKernel 1.72.0 release notes and migration guide
  - **Risk**: AI pipeline functionality may need code updates
  - **Testing**: Critical to verify AI evaluation pipeline still works

**Potential Breaking Changes** (2 occurrences):
- Not specified in assessment
- **Action**: Build and address compilation errors

4. **Code Modifications**
- **8 files**: Small data processing service
- **Focus**: Files using SemanticKernel APIs
- **Expected Changes**:
  - Update SemanticKernel API calls if signatures changed
  - Verify embedding generation still works
  - Verify Qdrant vector operations still function

5. **Testing Strategy**
   - **Build Test**: DataIngestor must compile without errors
   - **Security Test**: Verify vulnerability remediated (1.72.0 installed)
   - **AI Pipeline Test**: **CRITICAL** - Verify data ingestion and AI processing:
     - PDF ingestion (PdfPig)
     - Embedding generation (SemanticKernel + LocalEmbeddings)
     - Vector storage (Qdrant via Backend/SemanticKernel)
   - **Integration Test**: Verify DataIngestor can call Backend and Evaluator

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] **SemanticKernel.Core 1.72.0 installed (vulnerability fixed)**
   - [ ] Data ingestion pipeline runs successfully
   - [ ] PDF documents are processed
   - [ ] Embeddings are generated correctly
   - [ ] Vectors are stored in Qdrant
   - [ ] AI evaluation works (via Evaluator service)

**Risk Level**: Medium (**Security vulnerability fix required**; SemanticKernel major version jump may have breaking changes)

---

### Level 4: E2ETest.csproj (End-to-End Tests)

**Current State**:
- Target Framework: net8.0
- Project Type: DotNetCoreApp
- Files: 9
- Dependencies: ServiceDefaults, AppHost
- Used By: None (test project)
- Packages: 5 total (1 requires upgrade)
- Issues: 5 (1 mandatory, 3 potential, 1 optional)

**Target State**:
- Target Framework: net10.0
- Packages: 5 total (4 compatible, 1 upgraded)

**Migration Steps**:

1. **Update Target Framework**
   - Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

2. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|--------|
| Aspire.Hosting.Testing | 8.2.2 | 13.1.2 | **Deprecated** - Aspire upgrade required |
| Microsoft.NET.Test.Sdk | 17.11.1 | *(no change)* | ✅ Compatible |
| Microsoft.Playwright | 1.48.0 | *(no change)* | ✅ Compatible |
| xunit | 2.9.2 | *(no change)* | ✅ Compatible |
| xunit.runner.visualstudio | 2.8.2 | *(no change)* | ✅ Compatible |

3. **Expected Breaking Changes**

**Aspire.Hosting.Testing 8.2.2 → 13.1.2**:
- Test host setup API may have changed
- **Action**: Review Aspire.Hosting.Testing 13.x migration guide

**Behavioral Changes** (2 occurrences):
- E2E test execution behavior may differ
- **Action**: Run tests and verify pass rates

4. **Code Modifications**
- **9 test files**: Playwright-based E2E tests
- **Focus**: Test host initialization using Aspire.Hosting.Testing
- **Expected Changes**:
  - Update test host creation if API changed
  - Verify Playwright integration still works
  - Update test expectations if AppHost behavior changed

5. **Testing Strategy**
   - **Build Test**: E2ETest must compile without errors
   - **Test Host Test**: Verify Aspire test host starts AppHost correctly
   - **E2E Execution**: Run Playwright tests against orchestrated services
   - **Test Failure Analysis**: Distinguish between:
     - Test infrastructure failures (need API updates)
     - Actual application bugs (investigate AppHost/services)

6. **Validation Checklist**
   - [ ] Project builds without errors
   - [ ] Aspire test host initializes AppHost
   - [ ] Playwright tests execute
   - [ ] E2E tests pass (or failures explained)
   - [ ] Browser automation works correctly
   - [ ] Services orchestrated by AppHost are reachable

**Risk Level**: Low (Test project; Aspire.Hosting.Testing breaking changes unlikely; failures non-blocking for production)

---

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation |
|---------|-----------|-------------|------------|
| **EvaluationTests** | Medium | 8 binary incompatible API calls requiring mandatory fixes | Catalog of breaking changes prepared; test project failures won't block production code |
| **DataIngestor** | Medium | Security vulnerability in Microsoft.SemanticKernel.Core 1.25.0 | Immediate upgrade to 1.72.0 in atomic operation; verify AI pipeline functionality post-upgrade |
| **StaffWebUI** | Medium | 21 source incompatible API calls (largest Blazor project, 98 files) | Most source changes are Blazor API updates; breaking changes catalog provides replacements |
| **ServiceDefaults** | Medium-High | Foundation library used by 7 projects; 24 potential issues | Changes propagate to all dependents; fix at foundation level prevents cascading errors |
| **Backend** | Medium | Core API service used by 5 projects; 7 source incompatible APIs | API changes may affect multiple consumers; comprehensive testing required |

### All-At-Once Strategy Risk Factors

**Large Blast Radius**
- **Risk**: All 12 projects change simultaneously; any blocking issue affects entire solution
- **Mitigation**: 
  - Dedicated upgrade branch (`upgrade-to-NET10`) enables safe rollback
  - Breaking changes catalog prepared in advance
  - All package versions validated before execution
  - Single commit workflow enables atomic revert if needed

**Compilation Error Volume**
- **Risk**: 51 source incompatible APIs + 8 binary incompatible APIs = potential 59+ compilation errors appearing at once
- **Mitigation**:
  - Errors categorized by severity (binary → source → behavioral)
  - Fix in priority order: mandatory binary incompatible first, then source incompatible
  - Breaking changes catalog provides specific replacements for each API
  - MSBuild compilation order follows dependency graph, exposing foundation issues first

**Testing Surface Area**
- **Risk**: Must validate entire solution functionality in single test pass
- **Mitigation**:
  - Two test projects provide coverage: EvaluationTests (unit/integration), E2ETest (end-to-end)
  - Aspire AppHost enables local testing of all microservices
  - Smoke test checklist for manual validation
  - Test failures are non-blocking for plan completion (can be addressed post-upgrade)

### Security Vulnerabilities

#### Critical Security Issue

**Microsoft.SemanticKernel.Core**
- **Current Version**: 1.25.0
- **Target Version**: 1.72.0
- **CVE/Advisory**: Security vulnerability (details in assessment)
- **Affected Project**: DataIngestor
- **Severity**: Optional (per assessment) but **recommended to fix immediately**
- **Impact**: AI/ML pipeline in data ingestion workflow
- **Remediation Plan**:
  1. Update package reference in DataIngestor.csproj during atomic upgrade
  2. Verify no breaking changes in SemanticKernel 1.25.0 → 1.72.0 API surface
  3. Test AI evaluation functionality in DataIngestor after upgrade
  4. Validate EvaluationTests pass (tests the evaluation pipeline)

**⚠️ This security fix is included in the atomic upgrade operation - no separate action required.**

### Contingency Plans

#### Blocking Issue: Unresolvable Compilation Errors

**Scenario**: Breaking changes in .NET 10.0 APIs have no clear replacement, preventing compilation.

**Contingency**:
1. Consult .NET 10.0 breaking changes documentation: https://go.microsoft.com/fwlink/?linkid=2262679
2. Search GitHub issues for migration guidance
3. Implement temporary workaround (compatibility shim, alternative API)
4. If no solution within reasonable timeframe: rollback entire upgrade and escalate to team

**Rollback**: `git reset --hard HEAD~1` (if committed) or `git restore .` (if uncommitted)

#### Blocking Issue: Test Failures with No Clear Cause

**Scenario**: Tests fail after upgrade but error messages are unclear or unrelated to known breaking changes.

**Contingency**:
1. Isolate failing test(s) and run individually
2. Compare test behavior between .NET 8.0 and .NET 10.0 (use git stash to temporarily revert)
3. Review behavioral changes catalog for runtime behavior differences
4. If test failure is environmental (not code issue): update test expectations
5. If test failure indicates real bug introduced by .NET 10.0: file issue and determine criticality

**Decision Point**: Test failures are **non-blocking** for plan completion if:
- Production code compiles successfully
- Failures are in test infrastructure (not testing actual bugs)
- Manual smoke testing shows core functionality works

#### Blocking Issue: Performance Degradation

**Scenario**: Application performance drops significantly after upgrade (>50% slower).

**Contingency**:
1. Profile application to identify bottleneck (use .NET diagnostic tools)
2. Review .NET 10.0 performance-related breaking changes
3. Check for configuration changes needed (GC settings, threading model)
4. If degradation is in framework itself: file performance issue with .NET team
5. If no mitigation available: evaluate whether performance impact is acceptable

**Decision Point**: Performance issues are **non-blocking** unless they make application unusable.

#### Blocking Issue: Aspire Compatibility

**Scenario**: Aspire 13.1.2 packages have breaking changes that prevent AppHost orchestration.

**Contingency**:
1. Review Aspire 13.x migration guide
2. Check AppHost configuration for deprecated patterns
3. Update service registration code if needed
4. Verify Aspire.Hosting.* packages are all same version (13.1.2)
5. If Aspire blocking: consider incremental Aspire upgrade separate from .NET upgrade

**Note**: Aspire is well-aligned with .NET versioning; major incompatibilities unlikely.

### Risk Acceptance

The following risks are **accepted** for this upgrade:

- **Behavioral Changes**: 52 potential behavioral changes may alter runtime behavior without compilation errors. These will be discovered through testing and addressed as needed.
- **Deprecated Packages**: 12 packages are deprecated but still functional. Replacement planning can occur post-upgrade.
- **Python Project**: PythonInference.pyproj targeting net472 → net10.0 may require Python interop validation (minimal risk, 3 files only).

### Risk Monitoring

Post-upgrade monitoring for:
- Application startup time (watch for regression)
- Memory usage patterns (GC behavior may differ)
- API response times (performance baseline)
- Error rates in logs (new exceptions)
- AI evaluation accuracy (SemanticKernel upgrade impact)

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing follows dependency order to catch issues early and isolate failures.

### Phase 1: Atomic Upgrade Validation

**After completing atomic upgrade (all projects updated, solution builds with 0 errors):**

#### Build Validation
1. **Full Solution Build**
   - Command: `dotnet build eShopSupport.slnx`
   - **Success Criteria**: 0 errors
   - **Acceptable**: Warnings allowed (review for severity)
   - **Action on Failure**: Review errors, fix using breaking changes catalog, rebuild

2. **Per-Project Build Check** (optional, if full solution build fails)
   - Build projects in dependency order (Level 0 → Level 4)
   - Identify which project introduces errors
   - Fix at that level before proceeding

#### Dependency Validation
- **Package Restore**: `dotnet restore eShopSupport.slnx`
  - **Success Criteria**: No package conflicts
  - **Check**: No version mismatches between projects

- **Package Version Verification**:
  - Verify all 19 package upgrades applied correctly
  - Verify Microsoft.SemanticKernel.Core is 1.72.0 (security fix)

### Phase 2: Test Execution

**Test in dependency order to isolate failures:**

#### Level 1: Unit/Integration Tests (EvaluationTests)

**Project**: `test/EvaluationTests/EvaluationTests.csproj`

**Execution**:
```bash
dotnet test test/EvaluationTests/EvaluationTests.csproj
```

**Tests Coverage**:
- Backend service functionality
- Evaluator service logic
- AI evaluation metrics
- Binary incompatible APIs (8 mandatory fixes)

**Success Criteria**:
- All tests pass
- No test infrastructure failures
- Evaluation metrics produce expected results

**Action on Failure**:
- **Binary API errors**: Fix remaining API compatibility issues
- **Test logic failures**: Investigate .NET 10.0 behavioral changes
- **AI metric differences**: Verify SemanticKernel upgrade impact

#### Level 2: End-to-End Tests (E2ETest)

**Project**: `test/E2ETest/E2ETest.csproj`

**Execution**:
```bash
dotnet test test/E2ETest/E2ETest.csproj
```

**Tests Coverage**:
- AppHost orchestration
- Full service stack (Backend, CustomerWebUI, StaffWebUI, IdentityServer)
- Playwright browser automation
- Cross-service workflows

**Success Criteria**:
- All E2E tests pass
- AppHost starts all services successfully
- Browser automation works
- Service discovery functions

**Action on Failure**:
- **AppHost errors**: Review Aspire 13.1.2 configuration
- **Service startup failures**: Check individual service logs
- **Browser test failures**: Verify UI rendering (Blazor/Razor Pages changes)
- **Authentication failures**: Verify IdentityServer and JWT Bearer changes

### Phase 3: Manual Validation (Recommended)

**After automated tests pass, perform smoke testing:**

#### Smoke Test Checklist

**AppHost Startup**:
- [ ] Run AppHost: `dotnet run --project src/AppHost`
- [ ] Aspire dashboard opens (http://localhost:...)
- [ ] All services shown as "Running" in dashboard:
  - Backend
  - CustomerWebUI
  - StaffWebUI
  - IdentityServer
  - PostgreSQL
  - Redis
  - Qdrant
  - Azure Storage

**CustomerWebUI** (Razor Pages):
- [ ] Navigate to CustomerWebUI URL
- [ ] Home page loads
- [ ] User can log in (OpenIdConnect authentication)
- [ ] Product catalog displays
- [ ] Search functionality works
- [ ] Support ticket submission works
- [ ] SmartComponents render correctly

**StaffWebUI** (Blazor):
- [ ] Navigate to StaffWebUI URL
- [ ] Blazor app loads and renders
- [ ] User can log in (OpenIdConnect authentication)
- [ ] Ticket dashboard displays (QuickGrid component)
- [ ] FluentUI components render
- [ ] Navigation works
- [ ] Data grid pagination/filtering works
- [ ] Chat interface works (AI assistant)

**Backend API**:
- [ ] API responds to requests (via CustomerWebUI/StaffWebUI)
- [ ] JWT Bearer authentication works
- [ ] Database queries execute (PostgreSQL via EF Core)
- [ ] Redis caching works
- [ ] Azure Blob Storage operations work
- [ ] Qdrant vector search works (semantic product search)

**IdentityServer**:
- [ ] Token endpoint accessible
- [ ] Tokens issued successfully
- [ ] Token validation works in Backend/CustomerWebUI/StaffWebUI

**DataIngestor** (if applicable):
- [ ] Data ingestion pipeline runs
- [ ] PDF processing works (PdfPig)
- [ ] Embeddings generated (SemanticKernel 1.72.0)
- [ ] Vectors stored in Qdrant
- [ ] AI evaluation completes (via Evaluator)

#### Performance Baseline

**Compare .NET 8.0 vs .NET 10.0 performance (optional but recommended)**:

| Metric | .NET 8.0 Baseline | .NET 10.0 Actual | Delta | Acceptable? |
|--------|------------------|------------------|-------|-------------|
| AppHost startup time | (record) | (measure) | (calculate) | \u2264 +20% |
| Backend API response time (avg) | (record) | (measure) | (calculate) | \u2264 +10% |
| CustomerWebUI page load | (record) | (measure) | (calculate) | \u2264 +10% |
| StaffWebUI initial render | (record) | (measure) | (calculate) | \u2264 +10% |
| Memory usage (Backend) | (record) | (measure) | (calculate) | \u2264 +20% |

**Action on Degradation**: Investigate performance regressions; review .NET 10.0 performance guidance.

### Test Failure Decision Matrix

| Failure Type | Blocking? | Action |
|-------------|-----------|--------|
| **Compilation errors** | \u2705 **Yes** | Must fix before proceeding |
| **EvaluationTests failures (infrastructure)** | \u2705 **Yes** | Fix binary/source API issues |
| **EvaluationTests failures (logic)** | \u274c No | Investigate behavioral changes; may defer |
| **E2ETest failures (AppHost won't start)** | \u2705 **Yes** | Fix Aspire configuration |
| **E2ETest failures (individual tests)** | \u274c No | Investigate UI changes; may defer |
| **Manual smoke test failures (critical workflows)** | \u2705 **Yes** | Fix before merge |
| **Manual smoke test failures (edge cases)** | \u274c No | Log issue, address post-merge |
| **Performance degradation \u003e50%** | \u2705 **Yes** | Investigate; may require workaround or rollback |
| **Performance degradation 10-50%** | \u274c No | Log issue, investigate post-merge |

### Test Environment Requirements

- **.NET 10.0 SDK** installed
- **Docker** (for Aspire containers: PostgreSQL, Redis, Qdrant, Azure Storage emulator)
- **Playwright browsers** installed (`pwsh bin/Debug/net10.0/playwright.ps1 install`)
- **Azure Storage Emulator** or real Azure Storage account (for AppHost)
- **OpenAI API key** (for AI features) or Ollama local (if configured)

---

## Complexity & Effort Assessment

### Project Complexity Ratings

Projects rated by complexity factors: dependency count, issue count, file count, risk level.

| Project | Complexity | Dependencies | Issues | Files | Risk Factors |
|---------|-----------|--------------|--------|-------|--------------|
| **ServiceDefaults** | Medium | 0 | 26 | 11 | Foundation for 7 projects; changes propagate widely |
| **Backend** | Medium | 1 | 19 | 19 | Core API used by 5 projects; 7 source incompatible APIs |
| **StaffWebUI** | Medium | 1 | 30 | 98 | Largest project; 21 source incompatible APIs in Blazor code |
| **CustomerWebUI** | Low-Medium | 2 | 21 | 48 | Razor Pages; 15 source incompatible APIs |
| **AppHost** | Low | 4 | 11 | 4 | Orchestrator; all 5 Aspire packages upgrade 8.2.2 → 13.1.2 |
| **DataIngestor** | Low | 2 | 5 | 8 | Security vulnerability requires attention |
| **Evaluator** | Low | 2 | 11 | 2 | Small codebase; 10 potential issues but only 2 files |
| **EvaluationTests** | Low-Medium | 2 | 19 | 6 | 8 binary incompatible APIs (highest mandatory count) |
| **E2ETest** | Low | 2 | 5 | 9 | Playwright tests; 1 Aspire package upgrade |
| **IdentityServer** | Low | 1 | 1 | 81 | Only target framework change; large file count but stable |
| **DataGenerator** | Low | 0 | 9 | 23 | Standalone utility; 2 package upgrades |
| **PythonInference** | Very Low | 0 | 1 | 3 | Python project; target framework change only |

**Complexity Distribution**:
- Very Low: 1 project (PythonInference)
- Low: 5 projects (IdentityServer, DataGenerator, Evaluator, E2ETest, DataIngestor)
- Low-Medium: 2 projects (CustomerWebUI, EvaluationTests)
- Medium: 3 projects (ServiceDefaults, Backend, StaffWebUI)
- Medium-High: 0 projects
- High: 0 projects

✅ **No high-complexity projects** - all projects manageable within All-At-Once strategy.

### Phase Complexity Assessment

Since this is **All-At-Once** strategy, complexity is assessed for the single atomic operation:

**Phase 1: Atomic Upgrade**

**Complexity**: Medium-High (cumulative complexity of all projects)

**Factors**:
- 12 projects updated simultaneously
- 158 total issues (20 mandatory, 125 potential, 13 optional)
- 19 package upgrades across all projects
- 1 security vulnerability remediation
- Dependency depth of 5 levels (compilation order matters)

**Expected Actions**:
1. Update 12 TargetFramework properties (all to net10.0)
2. Update 19 package references across multiple projects
3. Restore dependencies for entire solution
4. Build solution (expect compilation errors from 51 source incompatible + 8 binary incompatible APIs)
5. Fix all compilation errors using breaking changes catalog
6. Rebuild to verify 0 errors
7. Address any residual warnings

**Relative Effort**: Medium
- Foundation projects (ServiceDefaults, Backend) require careful attention (affects many dependents)
- Blazor project (StaffWebUI) has most source changes but well-documented API updates
- Test project (EvaluationTests) has most binary incompatible calls but isolated impact
- Overall manageable due to homogeneous .NET 8.0 → .NET 10.0 path

**Phase 2: Test Validation**

**Complexity**: Low-Medium

**Factors**:
- 2 test projects (EvaluationTests, E2ETest)
- Aspire AppHost orchestration enables local testing
- Well-defined pass/fail criteria

**Expected Actions**:
1. Run EvaluationTests (unit/integration tests)
2. Run E2ETest (Playwright end-to-end tests)
3. Address test failures if any
4. Verify AI evaluation pipeline (post-SemanticKernel upgrade)

**Relative Effort**: Low
- Tests either pass or provide clear failure messages
- Test failures are non-blocking (can be addressed post-upgrade)

### Resource Requirements

**Skill Levels Required**:
- **.NET Framework Knowledge**: Intermediate
  - Understanding of .NET 8.0 → .NET 10.0 breaking changes
  - Familiarity with compilation error troubleshooting
  - Package version management

- **ASP.NET Core / Blazor**: Intermediate
  - Blazor component API changes (StaffWebUI)
  - Razor Pages patterns (CustomerWebUI)
  - Authentication middleware updates (IdentityServer, OpenIdConnect)

- **Aspire Knowledge**: Basic
  - Aspire 8.2.2 → 13.1.2 upgrade (well-documented)
  - AppHost orchestration configuration
  - Service discovery patterns

- **Testing**: Basic
  - xUnit test execution
  - Playwright E2E test interpretation
  - Test failure diagnosis

**Parallelization Capacity**:
- **Not Applicable**: All-At-Once strategy is a single atomic operation
- All project updates happen simultaneously
- Single developer can execute; no parallelization needed

### Effort Estimation by Category

**Note**: Relative complexity ratings provided below (Low/Medium/High). Avoid real-time estimates (hours/days) as agent cannot reliably predict duration.

| Category | Relative Effort | Reasoning |
|----------|----------------|-----------|
| **Project File Updates** | Low | Mechanical change: 12 TargetFramework properties net8.0 → net10.0 |
| **Package Updates** | Low-Medium | 19 packages to update; versions known and validated |
| **Compilation Error Fixes** | Medium | 51 source + 8 binary incompatible APIs; catalog provided |
| **Behavioral Change Review** | Low | 52 potential changes; most don't require code changes |
| **Security Remediation** | Low | 1 package upgrade (SemanticKernel 1.25.0 → 1.72.0) |
| **Test Execution** | Low | Run 2 test projects; well-defined |
| **Test Failure Resolution** | Low-Medium | Depends on failures found; may need debugging |
| **Manual Validation** | Low | Smoke tests via Aspire AppHost |

**Overall Relative Effort**: Medium
- Atomic operation creates single large work item
- Breaking changes catalog reduces discovery effort
- Clean dependency graph simplifies troubleshooting
- No multi-targeting or incremental coordination complexity

### Dependency Ordering Impact

While execution is atomic, understanding dependency order helps predict compilation behavior:

**Build Order** (MSBuild follows dependency graph):
1. **Level 0** builds first: ServiceDefaults, DataGenerator, PythonInference
   - If errors here, they appear first and may cascade to dependents
   - Fix Level 0 errors before addressing higher-level projects

2. **Level 1** builds next: Backend, IdentityServer, StaffWebUI
   - Depend on ServiceDefaults; errors here may cascade to Level 2+

3. **Level 2**: CustomerWebUI, Evaluator, EvaluationTests
   - Errors here are more isolated (fewer dependents)

4. **Level 3**: AppHost, DataIngestor
   - AppHost errors affect E2ETest only

5. **Level 4**: E2ETest
   - Terminal node; errors don't cascade

**Strategic Implication**: Focus compilation error fixes on lower-level projects first (ServiceDefaults, Backend) to reduce cascading errors in dependent projects.

---

## Source Control Strategy

### Branching Strategy

**Current Setup**:
- **Source Branch**: `upgrade` (starting point)
- **Upgrade Branch**: `upgrade-to-NET10` (created, currently active)
- **Main Branch**: `main` (or `master`)

**Workflow**:
```
main (or master)
 └─ upgrade (source)
     └─ upgrade-to-NET10 (all upgrade work happens here)
```

### Commit Strategy

**All-At-Once Single Commit Approach** (Preferred):

Since this is an atomic upgrade, a **single commit** is recommended:

```bash
# After atomic upgrade completes successfully (solution builds, tests pass)
git add .
git commit -m "chore: Upgrade solution to .NET 10.0

- Update all 12 projects from net8.0 to net10.0
- Upgrade 19 NuGet packages to .NET 10.0-compatible versions
- Fix security vulnerability: Microsoft.SemanticKernel.Core 1.25.0 → 1.72.0
- Upgrade Aspire packages from 8.2.2 to 13.1.2
- Fix 8 binary incompatible API calls in EvaluationTests
- Fix 51 source incompatible API calls across projects
- Address breaking changes in JWT authentication, Blazor, TensorPrimitives
- All tests passing: EvaluationTests, E2ETest

Projects updated:
- ServiceDefaults, Backend, IdentityServer, StaffWebUI (Level 0-1)
- CustomerWebUI, Evaluator, EvaluationTests (Level 2)
- AppHost, DataIngestor (Level 3)
- E2ETest (Level 4)
- DataGenerator, PythonInference (standalone)

Breaking changes addressed:
- JWT Bearer authentication API changes (Backend, CustomerWebUI, StaffWebUI)
- Blazor component API changes (StaffWebUI)
- Razor Pages API changes (CustomerWebUI)
- TensorPrimitives.CosineSimilarity signature (Backend)
- ApiKeyCredential constructor (ServiceDefaults)
- Binary incompatible evaluation APIs (EvaluationTests)
- Aspire orchestration API updates (AppHost)

Security fixes:
- CVE remediation: SemanticKernel.Core vulnerability (DataIngestor)

Tested:
✓ Solution builds with 0 errors
✓ EvaluationTests pass
✓ E2ETest pass
✓ AppHost orchestrates all services
✓ Manual smoke tests successful"
```

**Alternative: Multi-Commit Approach** (if atomic commit too large):

If the single commit becomes unwieldy or you want more granular history:

```bash
# Commit 1: Project file and package updates
git add **/*.csproj **/*.pyproj
git commit -m "chore: Update target frameworks and package versions to .NET 10.0"

# Commit 2: Code fixes for breaking changes
git add **/*.cs **/*.razor **/*.cshtml
git commit -m "fix: Address .NET 10.0 breaking API changes"

# Commit 3: Test fixes
git add test/**/*.cs
git commit -m "fix: Update test code for .NET 10.0 binary incompatible APIs"
```

**Recommended**: **Single commit** - aligns with All-At-Once strategy; easier to revert if needed.

### Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/):
- **Type**: `chore` (infrastructure changes like framework upgrades)
- **Scope**: Optional (e.g., `dotnet`, `upgrade`)
- **Description**: Clear summary of change
- **Body**: Detailed list of changes (see template above)
- **Footer**: Reference issues if applicable (e.g., `Fixes #123`)

### Review and Merge Process

#### Pre-Merge Checklist

Before creating pull request, verify:

- [ ] **Solution builds**: `dotnet build eShopSupport.slnx` succeeds with 0 errors
- [ ] **Tests pass**: `dotnet test eShopSupport.slnx` succeeds
- [ ] **No package conflicts**: `dotnet restore` completes without warnings
- [ ] **Security vulnerability fixed**: Microsoft.SemanticKernel.Core 1.72.0 confirmed
- [ ] **All 12 projects updated**: TargetFramework = net10.0
- [ ] **19 packages upgraded**: Versions match plan.md
- [ ] **Manual smoke tests**: Core workflows validated
- [ ] **Commit message**: Descriptive and follows format
- [ ] **No unintended changes**: Review git diff for accidental modifications

#### Pull Request Creation

**Title**: `chore: Upgrade solution to .NET 10.0 LTS`

**Description Template**:
```markdown
## Summary
Upgrades eShopSupport solution from .NET 8.0 to .NET 10.0 LTS.

## Changes
- 12 projects upgraded to net10.0
- 19 NuGet packages upgraded to .NET 10.0-compatible versions
- Aspire packages upgraded from 8.2.2 → 13.1.2
- Security vulnerability fixed: Microsoft.SemanticKernel.Core 1.25.0 → 1.72.0
- Breaking changes addressed (see commit message for details)

## Testing
- ✅ Solution builds with 0 errors
- ✅ EvaluationTests pass (8 binary incompatible APIs fixed)
- ✅ E2ETest pass (Aspire orchestration works)
- ✅ Manual smoke tests completed

## Breaking Changes
- JWT Bearer authentication API updated (Backend, UIs)
- Blazor component APIs updated (StaffWebUI)
- Razor Pages APIs updated (CustomerWebUI)
- TensorPrimitives API signature changed (Backend)
- Aspire orchestration API updated (AppHost)

## Security
- 🔒 Fixed CVE in Microsoft.SemanticKernel.Core (DataIngestor)

## Review Focus
- Verify all projects build and run
- Test authentication flows (IdentityServer, JWT)
- Test Aspire orchestration (AppHost starts all services)
- Verify AI evaluation pipeline (DataIngestor, Evaluator)

## Rollback Plan
If critical issues discovered post-merge:
- Revert commit: `git revert <commit-sha>`
- Or revert merge: `git revert -m 1 <merge-commit-sha>`

## References
- Assessment: `.github/upgrades/scenarios/new-dotnet-version_98b30e/assessment.md`
- Plan: `.github/upgrades/scenarios/new-dotnet-version_98b30e/plan.md`
- .NET 10.0 Breaking Changes: https://go.microsoft.com/fwlink/?linkid=2262679
```

#### Merge Criteria

**Required**:
- ✅ CI/CD pipeline passes (if configured)
- ✅ All automated tests pass
- ✅ Code review approved by at least one team member
- ✅ No merge conflicts with `upgrade` branch

**Recommended**:
- ✅ Manual testing in staging environment
- ✅ Performance baseline comparison acceptable
- ✅ Documentation updated (if .NET 10.0 introduces new patterns)

#### Merge Method

**Recommended**: **Merge commit** (preserves upgrade branch history)

```bash
# From upgrade branch
git checkout upgrade
git merge --no-ff upgrade-to-NET10
git push origin upgrade
```

**Alternative**: **Squash merge** (if you want single commit in main history)
- Only if upgrade-to-NET10 has multiple commits and you want to consolidate

### Post-Merge Actions

After merging to `upgrade`:

1. **Tag Release** (optional):
   ```bash
   git tag -a v2.0-net10.0 -m "Upgrade to .NET 10.0 LTS"
   git push origin v2.0-net10.0
   ```

2. **Delete Upgrade Branch** (keep or delete based on team preference):
   ```bash
   git branch -d upgrade-to-NET10
   git push origin --delete upgrade-to-NET10
   ```

3. **Update Documentation**:
   - README.md: Update .NET version requirement to 10.0
   - CONTRIBUTING.md: Update SDK installation instructions

4. **Monitor Production** (if deployed):
   - Watch error rates
   - Monitor performance metrics
   - Check for .NET 10.0-specific issues

### Conflict Resolution

If merge conflicts occur with `upgrade` branch:

**Strategy**:
1. Fetch latest `upgrade`: `git fetch origin upgrade`
2. Rebase upgrade-to-NET10: `git rebase origin/upgrade`
3. Resolve conflicts (prefer .NET 10.0 changes in project files)
4. Continue rebase: `git rebase --continue`
5. Force push: `git push --force-with-lease`

**Conflicts likely in**:
- `**/*.csproj` files (TargetFramework conflicts)
- `Directory.Build.props` (if exists)
- `global.json` (if exists and was updated)

**Resolution rule**: Prefer upgrade-to-NET10 changes (they contain .NET 10.0 updates).

---

## Success Criteria

The .NET 10.0 upgrade is considered successful when all criteria below are met.

### Technical Criteria

#### Build Success
- ✅ All 12 projects target `net10.0`
- ✅ Solution builds with **0 compilation errors**: `dotnet build eShopSupport.slnx`
- ✅ Solution builds with **0 warnings** (stretch goal; warnings acceptable if reviewed)
- ✅ Package restore completes without conflicts: `dotnet restore eShopSupport.slnx`
- ✅ No package dependency version mismatches

#### Package Updates
- ✅ All 19 package upgrades applied successfully:
  - Aspire packages: 8.2.2 → 13.1.2 (6 packages)
  - ASP.NET Core packages: 8.0.10 → 10.0.3 (3 packages)
  - Microsoft.Extensions packages: 8.x → 10.x (4 packages)
  - OpenTelemetry packages: 1.9.0 → 1.15.0 (2 packages)
  - Microsoft.SemanticKernel.Core: 1.25.0 → 1.72.0 (**security fix**)
  - Configuration/Hosting packages: 8.0.1 → 10.0.3 (2 packages)

#### Security
- ✅ **Security vulnerability remediated**: Microsoft.SemanticKernel.Core upgraded to 1.72.0 in DataIngestor
- ✅ No new security vulnerabilities introduced
- ✅ All deprecated packages replaced with supported versions

#### Breaking Changes Resolution
- ✅ **8 binary incompatible API calls fixed** (EvaluationTests)
- ✅ **51 source incompatible API calls fixed** across all projects
- ✅ JWT Bearer authentication updated (Backend, CustomerWebUI, StaffWebUI)
- ✅ Blazor component APIs updated (StaffWebUI)
- ✅ Razor Pages APIs updated (CustomerWebUI)
- ✅ TensorPrimitives.CosineSimilarity signature updated (Backend)
- ✅ ApiKeyCredential constructor updated (ServiceDefaults)
- ✅ Aspire orchestration APIs updated (AppHost, E2ETest)

#### Testing
- ✅ **EvaluationTests pass**: All unit/integration tests succeed
- ✅ **E2ETest pass**: All end-to-end Playwright tests succeed
- ✅ Test pass rate unchanged from .NET 8.0 baseline (or failures explained and acceptable)

#### Functionality
- ✅ **AppHost orchestration works**: All services start successfully
  - Backend API responds
  - CustomerWebUI (Razor Pages) loads
  - StaffWebUI (Blazor) renders
  - IdentityServer issues tokens
  - PostgreSQL connection works
  - Redis caching works
  - Qdrant vector search works
  - Azure Storage operations work

- ✅ **Authentication works**: 
  - IdentityServer issues tokens
  - JWT Bearer authentication validates tokens (Backend)
  - OpenIdConnect login works (CustomerWebUI, StaffWebUI)

- ✅ **AI Pipeline works**:
  - DataIngestor processes documents
  - SemanticKernel generates embeddings (1.72.0)
  - Evaluator produces evaluation metrics
  - Qdrant stores and retrieves vectors

- ✅ **Core Workflows**:
  - Customer can browse products (CustomerWebUI)
  - Customer can submit support tickets (CustomerWebUI)
  - Staff can view ticket dashboard (StaffWebUI)
  - Staff can interact with AI chat assistant (StaffWebUI)
  - Product semantic search works (Backend + Qdrant)

### Quality Criteria

#### Code Quality
- ✅ Code compiles without suppressing warnings (or suppressions justified)
- ✅ Breaking changes addressed with proper fixes (not workarounds)
- ✅ API usage follows .NET 10.0 best practices
- ✅ No obsolete APIs remain in codebase (unless deprecated but functional)

#### Test Coverage
- ✅ Test coverage maintained (no tests deleted to make tests pass)
- ✅ New behavioral differences tested and validated
- ✅ AI evaluation accuracy unchanged (or improvements documented)

#### Documentation
- ✅ README.md updated to require .NET 10.0 SDK
- ✅ CONTRIBUTING.md updated with .NET 10.0 setup instructions
- ✅ Assessment and plan files committed to `.github/upgrades/scenarios/new-dotnet-version_98b30e/`

### Process Criteria

#### All-At-Once Strategy Adherence
- ✅ All 12 projects updated simultaneously (atomic operation)
- ✅ No intermediate multi-targeting states (no `net8.0;net10.0`)
- ✅ Single coordinated build and test cycle completed
- ✅ No per-project incremental migration performed

#### Source Control
- ✅ All changes committed to `upgrade-to-NET10` branch
- ✅ Commit message(s) descriptive and follows Conventional Commits format
- ✅ **Single commit** for entire upgrade (preferred) or logical multi-commit sequence
- ✅ No unintended files committed (bin/, obj/, .vs/, etc.)

#### Validation
- ✅ Pre-merge checklist completed (see Source Control Strategy)
- ✅ Manual smoke tests performed and passed
- ✅ Performance baseline compared (degradation acceptable or explained)

### Acceptance Criteria Summary

**Minimum Viable Success** (must have):
1. ✅ All projects build without errors
2. ✅ All 19 packages upgraded
3. ✅ Security vulnerability fixed (SemanticKernel.Core → 1.72.0)
4. ✅ EvaluationTests pass
5. ✅ E2ETest pass
6. ✅ AppHost starts all services
7. ✅ Core authentication works
8. ✅ Changes committed and ready for PR

**Full Success** (should have):
- All minimum criteria above ✅
- Manual smoke tests pass ✅
- No warnings in build ✅
- Performance acceptable ✅
- Documentation updated ✅

**Exceptional Success** (nice to have):
- All full success criteria above ✅
- Performance improvements over .NET 8.0 🚀
- Test coverage increased ✅
- Code quality improvements (e.g., using new .NET 10.0 features) ✨

### Sign-Off Checklist

Before marking upgrade as complete:

- [ ] **Technical Lead**: Reviewed and approved technical changes
- [ ] **QA**: Validated testing results and smoke tests
- [ ] **Security**: Confirmed vulnerability remediation
- [ ] **DevOps**: CI/CD pipeline passes (if applicable)
- [ ] **Product Owner**: Core functionality validated

### Success Metrics

**Quantitative**:
- ✅ 12 projects migrated
- ✅ 158 issues resolved (20 mandatory, 125 potential, 13 optional)
- ✅ 19 packages upgraded
- ✅ 1 security vulnerability fixed
- ✅ 0 build errors
- ✅ 100% test pass rate (EvaluationTests + E2ETest)

**Qualitative**:
- ✅ Solution runs on modern .NET 10.0 LTS platform
- ✅ No technical debt from workarounds or hacks
- ✅ Team confidence in .NET 10.0 stability
- ✅ Ready for production deployment

---

**🎉 Upon meeting all success criteria, the .NET 10.0 upgrade is COMPLETE!**

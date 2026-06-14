# .NET 10 Upgrade Plan

## Overview

**Target**: Upgrade all 6 projects from net8.0/net9.0 to net10.0
**Scope**: 6 projects (2 class libraries, 1 shared UI lib, 1 MAUI app, 1 Blazor web app, 1 API), ~21 package updates, 27 mandatory API fixes

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 6 projects, all on modern .NET (net8.0/net9.0), clear dependency structure, manageable issue count.

## Tasks

### 01-prerequisites: Verify SDK and toolchain readiness

Confirm the .NET 10 SDK is installed and the solution's global.json (if present) is compatible with net10.0. Update or remove global.json constraints that would prevent the SDK from being used.

**Done when**: `dotnet --version` reports a .NET 10 SDK, and global.json (if it exists) allows net10.0 builds.

---

### 02-central-package-management: Set up Central Package Management

Create `Directory.Packages.props` at the solution root and migrate all PackageReference versions from the 6 project files into it. Each project file retains `<PackageReference Include="...">` without `Version` attributes. This establishes version consistency before the main upgrade changes package versions.

All 6 projects use SDK-style PackageReference today with 25 distinct packages across the solution. Moving to CPM before the TFM upgrade ensures package version bumps happen in one place during the next task.

**Done when**: `Directory.Packages.props` exists with all package versions centralized, all project files reference packages without inline versions, and the solution builds successfully on the current TFMs.

---

### 03-upgrade-all-projects: Upgrade TFMs, packages, and fix breaking changes

Update all 6 projects to net10.0 (MAUI project updates to net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-windows10.0.19041.0). Bump all 21 packages with recommended upgrades to their net10.0-compatible versions in Directory.Packages.props. Fix 27 mandatory API issues across the solution including binary incompatibilities (Api.0001), source incompatibilities (Api.0002), and behavioral changes (Api.0003).

Key areas requiring attention:
- ATL.Sankofa.Media.Business: 16 mandatory issues including binary/source incompatibilities and behavioral changes
- ATL.Sankofa.Media.UI.Maui: 6 mandatory issues with multi-TFM platform targets
- IdentityModel & Claims-based Security: 13 issues across the solution related to this technology area
- ATL.Sankofa.Media.API: 2 mandatory issues with authentication/JWT packages

**Done when**: All projects target net10.0, all packages updated to net10.0-compatible versions, all mandatory API issues resolved, and the full solution builds with zero errors and zero warnings.

---

### 04-final-validation: Validate solution build and run tests

Perform a clean build of the entire solution, run all automated tests, and verify no regressions. Document any remaining recommendations (deferred items, optional modernizations).

**Done when**: Clean solution build succeeds, all tests pass, and any deferred recommendations are documented.

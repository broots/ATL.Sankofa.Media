# 02-central-package-management: Set up Central Package Management

Create `Directory.Packages.props` at the solution root and migrate all PackageReference versions from the 6 project files into it. Each project file retains `<PackageReference Include="...">` without `Version` attributes. This establishes version consistency before the main upgrade changes package versions.

All 6 projects use SDK-style PackageReference today with 25 distinct packages across the solution. Moving to CPM before the TFM upgrade ensures package version bumps happen in one place during the next task.

**Done when**: `Directory.Packages.props` exists with all package versions centralized, all project files reference packages without inline versions, and the solution builds successfully on the current TFMs.

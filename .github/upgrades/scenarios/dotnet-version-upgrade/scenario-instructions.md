# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Package Management: Central Package Management (CPM)

### Compatibility
- Unsupported API Handling: Fix Inline

## Source Control
- **Source Branch**: feature/JB/Initial
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Strategy
**Selected**: All-at-Once
**Rationale**: 6 projects, all modern .NET (net8.0/net9.0), manageable scope and issue count.

### Execution Constraints
- All projects upgraded together — no tier ordering, no phased rollout
- Operation sequence: update TFMs → update packages → restore → build and fix all errors → validate
- Single atomic pass for the core upgrade; solution may be temporarily broken until complete
- Testing performed after atomic upgrade completes successfully
- CPM setup occurs before TFM upgrade (separate prerequisite task)

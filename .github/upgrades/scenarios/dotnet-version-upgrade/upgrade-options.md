# Upgrade Options — ATL.Sankofa.Media

Assessment: 6 projects (net8.0/net9.0 → net10.0), 218 issues (27 mandatory), 5-tier dependency graph, 0 incompatible packages, 21 packages need version bumps

## Strategy

### Upgrade Strategy
6 projects all on modern .NET with a manageable issue count — All-at-Once avoids multi-targeting overhead for this small solution.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade all projects simultaneously in a single atomic pass; fastest approach with no multi-targeting overhead |
| Top-Down | Upgrade entry-point applications first with multi-targeting on shared libraries; keeps solution buildable throughout |

## Project Structure

### Package Management
6 projects without centralized package management, all SDK-style, modern-to-modern upgrade — CPM provides consistency and easier maintenance.

| Value | Description |
|-------|-------------|
| **Central Package Management (CPM)** (selected) | Creates Directory.Packages.props, moves versions out of project files; better consistency at scale |
| Per-Project (defer CPM to post-migration) | Each project retains its own versions; CPM added later as a cleanup step |

## Compatibility

### Unsupported API Handling
27 mandatory API changes detected across projects (binary and source incompatible). Modern-to-modern changes are typically minor replacements.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve every API change in the same task including complex ones; no deferred work or stubs to clean up |
| Defer Complex Changes | Apply simple replacements inline; generate stubs for complex changes and create resolution subtasks |

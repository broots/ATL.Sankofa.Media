# 03-upgrade-all-projects: Upgrade TFMs, packages, and fix breaking changes

Update all 6 projects to net10.0 (MAUI project updates to net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-windows10.0.19041.0). Bump all 21 packages with recommended upgrades to their net10.0-compatible versions in Directory.Packages.props. Fix 27 mandatory API issues across the solution including binary incompatibilities (Api.0001), source incompatibilities (Api.0002), and behavioral changes (Api.0003).

Key areas requiring attention:
- ATL.Sankofa.Media.Business: 16 mandatory issues including binary/source incompatibilities and behavioral changes
- ATL.Sankofa.Media.UI.Maui: 6 mandatory issues with multi-TFM platform targets
- IdentityModel & Claims-based Security: 13 issues across the solution related to this technology area
- ATL.Sankofa.Media.API: 2 mandatory issues with authentication/JWT packages

**Done when**: All projects target net10.0, all packages updated to net10.0-compatible versions, all mandatory API issues resolved, and the full solution builds with zero errors and zero warnings.

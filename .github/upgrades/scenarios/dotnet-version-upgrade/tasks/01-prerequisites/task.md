# 01-prerequisites: Verify SDK and toolchain readiness

Confirm the .NET 10 SDK is installed and the solution's global.json (if present) is compatible with net10.0. Update or remove global.json constraints that would prevent the SDK from being used.

**Done when**: `dotnet --version` reports a .NET 10 SDK, and global.json (if it exists) allows net10.0 builds.

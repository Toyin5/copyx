# Chocolatey package

This directory contains the Chocolatey package template for CopyX.

Build the package from the repository root:

```powershell
.\scripts\package-chocolatey.ps1
```

The script:

1. Runs the test suite.
2. Publishes CopyX as a self-contained `win-x64` executable.
3. Copies the Chocolatey template into `artifacts\chocolatey`.
4. Runs `choco pack`.

The resulting `.nupkg` is written to `artifacts\chocolatey`.

Useful options:

```powershell
.\scripts\package-chocolatey.ps1 -Version 0.1.1
.\scripts\package-chocolatey.ps1 -Runtime win-arm64
.\scripts\package-chocolatey.ps1 -SkipTests
```

Before publishing to the public Chocolatey Community Repository, add public project metadata such as `projectUrl`, `packageSourceUrl`, and license information to `chocolatey\copyx.nuspec`.

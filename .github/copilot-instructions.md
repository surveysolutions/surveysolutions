# Copilot Coding Agent Onboarding Instructions

## Repository Overview

**Survey Solutions** is a large-scale, production-grade survey management and data collection platform developed by the World Bank. This repository contains the full stack: backend services, web frontends, integration and unit tests, build scripts, and deployment tooling.

- **Primary Languages:** C# (.NET), JavaScript/TypeScript (frontend), PowerShell, Bash
- **Frameworks/Tools:** .NET 9, SonarCloud, PostgreSQL (for integration tests), Docker (for some deployments)
- **Repo Size:** Large, with multiple solutions and subprojects
- **Target Runtimes:** .NET 9, Node.js (for frontend), Java 17 (for SonarCloud JS analysis)

## Build, Test, and Validation Instructions

### Environment Setup

- **.NET SDK:** 9.0.x (always install before building or testing)
- **Node.js:** Required for frontend (see `docs/development/frontend.md` for details)
- **PostgreSQL:** Required for integration tests (see CI for version and setup)
- **Java 17:** Required for SonarCloud JS analysis

### Bootstrap

- No explicit bootstrap script; ensure all submodules are checked out and required SDKs are installed.
- For frontend, always run `npm install` in the relevant UI directories before building or testing.

### Build

- **Backend:**  
  - Build all backend projects with:  
    `dotnet build "src/WB.sln"`
  - For installer:  
    `dotnet build "installer/build/src/SurveySolutionsBootstrap.sln"`
- **Frontend:**  
  - See `docs/development/frontend.md` for build steps per UI project (e.g., `WB.UI.Designer`, `WB.UI.Headquarters.Core`, etc.)

### Test

- **Unit Tests:**  
  - Run from repo root:  
    ```
    dotnet test src/Tests/WB.Tests.Unit
    dotnet test src/Tests/WB.Tests.Web
    dotnet test src/Tests/WB.Tests.Unit.Designer
    ```
- **Integration Tests:**  
  - Requires PostgreSQL running (see CI for env vars and connection strings)
  - Run:  
    ```
    dotnet test src/Tests/WB.Tests.Integration
    dotnet test src/Tests/WB.Tests.Integration.Designer
    dotnet test src/Services/Export/WB.Services.Export.Tests
    dotnet test src/Services/Core/WB.Services.Scheduler.Tests
    ```
  - Set up connection strings as in `.github/workflows/ci.yml` (see "integration" job for details).

### Lint and Code Analysis

- **SonarCloud:**  
  - .NET:  
    ```
    dotnet tool install --global dotnet-sonarscanner
    dotnet sonarscanner begin ... # see ci.yml for full command
    dotnet build './src/WB without Xamarin.sln'
    dotnet sonarscanner end ...
    ```
  - JavaScript:  
    - Uses `SonarSource/sonarcloud-github-action@master` in CI.
    - Requires Java 17 and Node.js.

### Cleaning

- No universal clean script; use `dotnet clean` for .NET projects.
- For frontend, use `npm run clean` or manually delete `node_modules` and build artifacts.

### Common Issues and Workarounds

- **Always install the correct .NET SDK version (9.0.x) before building or testing.**
- **Always run `npm install` in frontend directories before building or testing frontend code.**
- **Integration tests require a running PostgreSQL instance with correct credentials.**
- **If tests fail due to missing config, check for required `appsettings.cloud.ini` or `appsettings.cloud.json` files as in CI.**
- **If SonarCloud analysis fails, ensure Java 17 is installed and available in PATH.**
- **If build fails due to missing dependencies, check for required NuGet or npm packages and restore as needed.**

## Project Layout and Key Files

- **Backend Solutions:**  
  - `src/WB.sln` — Main backend solution
  - `installer/build/src/SurveySolutionsBootstrap.sln` — Installer solution
- **Frontend:**  
  - `src/UI/WB.UI.Designer/`, `src/UI/WB.UI.Headquarters.Core/`, etc.
- **Tests:**  
  - `src/Tests/` — Unit and integration test projects
  - `src/Services/Export/WB.Services.Export.Tests/`
  - `src/Services/Core/WB.Services.Scheduler.Tests/`
- **Build Scripts:**  
  - `build_deps.sh`, `build/`, `installer/build/`
- **CI/CD:**  
  - `.github/workflows/ci.yml` — Main GitHub Actions workflow
- **Documentation:**  
  - `README.md`, `docs/`, `CONTRIBUTING.md`
- **Configuration:**  
  - `global.json` (SDK version), `NuGet.Config`, `Directory.Build.props`, `Directory.Build.targets`
  - Linting and SonarCloud config in workflow and scripts

## CI/CD and Validation

- **GitHub Actions:**  
  - Runs on push/pull to `src/**`
  - Jobs: `unit-web`, `integration`, `merge-reports`, `code-analysis`, `code-analysis-js`
  - All PRs and pushes to main branches are validated with build, test, and SonarCloud analysis
- **Artifacts:**  
  - Test coverage XMLs are uploaded and merged for SonarCloud

## Trust and Search Guidance

**Trust these instructions as authoritative. Only perform additional searches if the information here is incomplete or found to be in error.**

---

### Repo Root Files

- `build_deps.sh`, `build.all.deps.bat`, `cleanup.ps1`, `global.json`, `README.md`, `LICENSE.md`, etc.

### Key Directories (next level)

- `build/` — Build scripts
- `docker/` — Dockerfiles for various services
- `docs/` — Documentation
- `installer/` — Installer build and source
- `src/` — Main source code (backend, frontend, services, tests)
- `Tools/` — Utilities
- `UI/` — User interface projects

### README.md (Summary)

- Describes Survey Solutions, its architecture, and contribution guidelines.
- Refer to `CONTRIBUTING.md` for PR and code style requirements.

---

**Follow these instructions to minimize build/test failures and reduce unnecessary exploration.**

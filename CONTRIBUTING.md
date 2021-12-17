# Development environment

In order to be able to build Survey Solutions locally, you will need to install several prerequisites fist:

- Install PostgreSQL 12 or newer.
- Install LTS version of node JS (we currently use version 14) - https://nodejs.org/en/
- Install latest version of .Net SDK https://dotnet.microsoft.com/download
- Install Xamarin build tools for your platform.

## Running locally

First you need to build javascript UI. It Can be done by running either `.build.ps1`, `build.all.deps.bat` or `build_deps.sh` scripts.
This will build frontend components for Designer, Headquarters and Web Tester applications. You can see more details regarding editing and building the frontend
[here](docs/development/frontend.md).

By default web applications use a locally installed PostgreSQL database. Review connection strings in `appsettings.ini` files to be able to run applications properly.
Each web application supports configuration override for development purposes. Add `appsettings.DEV_DEFAULTS.ini` with required configuration.

To **run Designer** application execute:

``` sh
dotnet run --project src/UI/WB.UI.Designer/WB.UI.Designer.csproj
````

To **run Web tester** execute:

``` sh
dotnet run --project src/UI/WB.UI.Designer/WB.UI.Designer.csproj
```

To **run Headquarters** application execute:

``` sh
dotnet run --project src/UI/WB.UI.Headquarters.Core/WB.UI.Headquarters.csproj
```

to **run Export service** execute:

``` sh
dotnet run --project src/Services/Export/WB.Services.Export.Host/WB.Services.Export.Host.csproj
```

In order to build Android applications you can use following command:

``` pwsh
# All apps
.\.build.ps1 Android

# interviewer
.\.build.ps1 AndroidInterviewer

# supervisor
.\.build.ps1 AndroidSupervisor

# tester
msbuild src/UI/Tester/WB.UI.Tester/WB.UI.Tester.csproj /restore /p:XamarinBuildDownloadAllowUnsecure=true /t:SignAndroidPackage
```

in the log there will be the location of compiled apks. However, in order to go to production you may need to create your own signature to [sign apks](https://docs.microsoft.com/en-us/xamarin/android/deploy-test/signing/).

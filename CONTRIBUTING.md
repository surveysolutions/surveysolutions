# Development environment

In order to be able to buld Survey Solutions locally, you will need to install several prerequesties fist:

- Install PostgreSQL.
- Install LTS version of node JS (we currently use version 12).
- Install yarn package manager.
- Install latest version of asp.net core SDK.
- Install Xamarin build tools for your platform.

## Running locally

First you need to build javascript UI. It Can be done by running either `.build.ps1`, `build.all.deps.bat` or `build_deps.sh` scripts. This will build frontend components for Designer, Headquarters and Web Tester applications.

By default web applications use a locally installed PostgreSQL database. Review connection strings in `appsettings.ini` files to be able to run applications properly.

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

In order to build android applications you can use following command:

``` sh
# interviewer
msbuild src/UI/Interviewer/WB.UI.Interviewer/WB.UI.Interviewer.csproj /restore /p:XamarinBuildDownloadAllowUnsecure=true /t:SignAndroidPackage
# supervisor
msbuild src/UI/Supervisor/WB.UI.Supervisor/WB.UI.Supervisor.csproj /restore /p:XamarinBuildDownloadAllowUnsecure=true /t:SignAndroidPackage
# tester
msbuild src/UI/Tester/WB.UI.Tester/WB.UI.Tester.csproj /restore /p:XamarinBuildDownloadAllowUnsecure=true /t:SignAndroidPackage
```

in the log there will be the location of compiled apks. However, in order to go to production you may need to create your own signature to [sign apks](https://docs.microsoft.com/en-us/xamarin/android/deploy-test/signing/).

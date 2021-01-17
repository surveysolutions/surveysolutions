## Overview

Survey Solutions is a survey management and data collection system developed by the World Bank. The software is used worldwide by the National Statistical Offices, Central Banks, Non-Government Organizations (NGOs) and universities to collect and manage surveys of households, individuals, enterprises (firms/establishments), infrastructure (schools, hospitals, etc) and communities. It has been used to conduct censuses, household income and expenditure surveys, labor force surveys, price surveys, and other types of data collection operations.

Comprehensive documentation can be found on [support portal](https://support.mysurvey.solutions/). Deployment instructions for production instances can be also found [there](https://support.mysurvey.solutions/headquarters/config/server-setup/).

Survey solutions consists of the following major components:

## Web applications

1. **Designer** - place for designing questionnaires. Supports sharing and collaboration between authors of questionnaires.
1. **Web Tester** - application used to quickly test questionnaires using web browser. Supports recording of scenarios to ease testing of large questionnaires.
1. **Headquarters** - used for survey management and data collection. Surves as central storage of collected interviews for both CAPI and CAWI modes.
1. **Export service** - backend service that has no UI. Used by Headquarters application to generate export files. Supports exporting to various statistical packages.

## Android applications

1. **Interviewer** - data collection tool used by field workers to conduct interviews.
1. **Supervisor** - can be used as temporary storage of interviews in the ares where there is no internet connectivity. Allows to receive interviews and distribute work for interviewers with [nearby communication](https://developers.google.com/nearby).
1. **Tester** - same as interviewer application, but works directly with Designer application. Should be used to verify perfomance and usability of questionnaire during design time.

## Dev environment

Prerequesties:

- Install PostgreSQL.
- Install LTS version of node JS (we currently use version 12).
- Install yarn package manager.
- Install latest version of asp.net core SDK.
- Install Xamarin build tools for your platform.

## Building 

Build is done using [https://nuke.build/](https://nuke.build/).

On fundamentals on how to use `Nuke` please refer to documentation page: https://nuke.build/docs/running-builds/fundamentals.html

To see all available build options run `build.cmd --help` on Windows or `build.sh --help` on Linux systems. 

It's possible to install [shell completion helpers](https://nuke.build/docs/running-builds/global-tool.html#shell-snippets)

Usefull examples for development is:
- `nuke dev` - build all apps and js deps in debug mode
- `nuke devjs` - buld all js deps in debug mode
- `nuke android` - build all android apps

For more information refer [build Readme.md](/build/README.md)

## Running locally

Just run `build.cmd` or `build.sh` to build frontend and backend applications.

By default web applications use PostgreSQL installed locally. Review connection strings in `appsettings.ini` files to be able to run applications properly.

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
./build android
```

in the log there will be location of compiled apks. But in order to go to production you will need to create your or signature to [sign apks](https://docs.microsoft.com/en-us/xamarin/android/deploy-test/signing/).

[Release notes](https://support.mysurvey.solutions/release-notes/)

# escape=`

# Use the latest Windows Server Core image with .NET Framework 4.8.
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8

# Restore the default Windows shell for correct batch processing.
SHELL ["cmd", "/S", "/C"]

# Download the Build Tools bootstrapper.
RUN powershell.exe  -Command `
  $ErrorActionPreference = 'Stop'; New-Item c:\Temp -ItemType Directory;`
  Invoke-WebRequest https://aka.ms/vs/16/release/vs_buildtools.exe -OutFile C:\TEMP\vs_buildtools.exe;

ADD .vsconfig C:\Temp

# Install Build Tools excluding workloads and components with known issues.
RUN C:\TEMP\vs_buildtools.exe --quiet --wait --norestart --nocache `
    --config c:\Temp\.vsconfig `
    --installPath C:\BuildTools `
 || IF "%ERRORLEVEL%"=="3010" EXIT 0

# Setting up Android SDK/NDK
RUN powershell.exe -Command `
  $ErrorActionPreference = 'Stop'; `
  Invoke-WebRequest https://www.7-zip.org/a/7z1900-x64.msi -OutFile c:\temp\7zip.msi;

# Expand-Archive is painfully slow.
RUN c:\temp\7zip.msi /quiet /norestart

RUN powershell.exe -Command `
  $ErrorActionPreference = 'Stop'; `
  $ProgressPreference = 'SilentlyContinue'; `
  Invoke-WebRequest https://dl.google.com/android/repository/android-ndk-r19c-windows-x86_64.zip -OutFile c:\temp\ndk.zip

RUN ["C:\\Program Files\\7-Zip\\7z.exe", "x", "c:\\temp\\ndk.zip", "-oc:\\AndroidNDK\\", "-y"]

RUN powershell.exe -Command `
  $ErrorActionPreference = 'Stop'; `
  $ProgressPreference = 'SilentlyContinue'; `
  Invoke-WebRequest https://dl.google.com/android/repository/sdk-tools-windows-4333796.zip -OutFile c:\temp\sdk.zip

RUN ["C:\\Program Files\\7-Zip\\7z.exe", "x", "c:\\temp\\sdk.zip", "-oc:\\AndroidSDK\\", "-y"]

ENV JAVA_HOME="c:\\Program Files\\Android\\jdk\\microsoft_dist_openjdk_1.8.0.25"

# 'yes' file required to accept google licenses: https://stackoverflow.com/a/48539058
ADD yes .

# Install Android v28 build tools
RUN call C:\AndroidSDK\tools\bin\sdkmanager.bat "platform-tools" "platforms;android-28" "build-tools;28.0.3" < yes

# Setting up NODEjs env
RUN powershell.exe -Command `
  $ErrorActionPreference = 'Stop'; `
  $ProgressPreference = 'SilentlyContinue'; `
  Invoke-WebRequest https://nodejs.org/dist/v10.15.3/node-v10.15.3-x64.msi -OutFile c:\temp\node.msi

RUN c:\temp\node.msi /quiet /norestart 

# Start developer command prompt with any other commands specified.
ENTRYPOINT C:\BuildTools\Common7\Tools\VsDevCmd.bat &&

ENV NUGET_PACKAGES C:\src\.packages_cache
ENV AndroidSdkDirectory=C:\AndroidSDK
ENV AndroidNdkDirectory=C:\AndroidNDK\android-ndk-r19c
ENV JavaSdkDirectory="c:\\Program Files\\Android\\jdk\\microsoft_dist_openjdk_1.8.0.25"
RUN MKDIR c:\src

VOLUME c:\src
WORKDIR c:\src\

# Default to PowerShell if no other command specified.
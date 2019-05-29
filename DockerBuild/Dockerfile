# escape=`

# Use the latest Windows Server Core image with .NET Framework 4.8.
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 as base
SHELL ["cmd", "/S", "/C"]
RUN MKDIR c:\temp
RUN powershell [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

RUN powershell Invoke-WebRequest https://curl.haxx.se/download/curl-7.65.0.zip -outFile c:\temp\curl.Zip; `
  Expand-Archive c:\temp\curl.zip .; Remove-Item c:\temp\curl.zip 

RUN curl -sL https://www.7-zip.org/a/7z1900-x64.msi -o c:\temp\7zip.msi `
  && c:\temp\7zip.msi /quiet /norestart `
  && del c:\temp\7zip.msi

RUN ["setx", "path", "%path%;C:\\Program Files\\7-Zip\\"]

FROM base as ndk
RUN curl -L https://dl.google.com/android/repository/android-ndk-r19c-windows-x86_64.zip -o c:\temp\ndk.zip && `
  7z x c:\temp\ndk.zip -oc:\AndroidNDK -y && del c:\temp\ndk.zip

FROM base as sdk
RUN curl -L https://dl.google.com/android/repository/sdk-tools-windows-4333796.zip -o c:\temp\sdk.zip && `
  7z x c:\temp\sdk.zip -oc:\AndroidSDK -y && del c:\temp\sdk.zip


FROM base

# Download the Build Tools bootstrapper.
RUN curl -L https://aka.ms/vs/16/release/vs_buildtools.exe -o C:\TEMP\vs_buildtools.exe

ADD .vsconfig C:\Temp

# Install Build Tools excluding workloads and components with known issues.
RUN C:\TEMP\vs_buildtools.exe --quiet --wait --norestart --nocache `
    --config c:\Temp\.vsconfig `
    --installPath C:\BuildTools `
 || IF "%ERRORLEVEL%"=="3010" EXIT 0

# Copy SDK and NDK
COPY --from=ndk c:\AndroidNDK c:\AndroidNDK
COPY --from=sdk c:\AndroidSDK c:\AndroidSDK

# Install Android v28 build tools
## 'yes' file required to accept google licenses: https://stackoverflow.com/a/48539058
ADD yes .
ENV JAVA_HOME="c:\\Program Files\\Android\\jdk\\microsoft_dist_openjdk_1.8.0.25"
RUN call C:\AndroidSDK\tools\bin\sdkmanager.bat "platform-tools" "platforms;android-28" "build-tools;28.0.3" < yes

# Install Nodejs
RUN curl -L https://nodejs.org/dist/v10.15.3/node-v10.15.3-x64.msi -o c:\temp\node.msi `
  && c:\temp\node.msi /quiet /norestart `
  && del c:\temp\node.msi

# Setting node to cache packages in volume
RUN npm config set cache c:\src\.cache\npm

# Install Yarn
RUN curl -L https://yarnpkg.com/latest.msi -o c:\temp\yarn.msi `
  && c:\temp\yarn.msi /quiet /norestart && del c:\temp\yarn.msi

# Start developer command prompt with any other commands specified.
ENTRYPOINT C:\BuildTools\Common7\Tools\VsDevCmd.bat &&

ENV NUGET_PACKAGES C:\src\.cache\nuget
ENV AndroidSdkDirectory=C:\AndroidSDK
ENV AndroidNdkDirectory=C:\AndroidNDK\android-ndk-r19c
ENV JavaSdkDirectory="c:\\Program Files\\Android\\jdk\\microsoft_dist_openjdk_1.8.0.25"
RUN MKDIR c:\src

VOLUME c:\src
WORKDIR c:\src\

# Default to PowerShell if no other command specified.

param([string]$VersionPrefix,
[INT]$BuildNumber,
[string]$BuildConfiguration='release',
[string]$KeystorePassword)

$ErrorActionPreference = "Stop"

#do not allow empty prefix
if([string]::IsNullOrWhiteSpace($VersionPrefix)){
	Write-Host "##teamcity[buildProblem description='VersionPrefix param is not set']"
	Exit
}
#do not allow empty build number	
if(!$BuildNumber){
	Write-Host "##teamcity[buildProblem description='BuildNumber param is not set']"
	Exit
}

#do not allow empty KeystorePassword
if([string]::IsNullOrWhiteSpace($KeystorePassword)){
	Write-Host "##teamcity[buildProblem description='VersionPrefix param is not set']"
	Exit
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

$PackageName = 'WBCapi.apk'
$VersionName = $VersionPrefix + $BuildNumber
try {
	. "$scriptFolder\build-android-package.ps1" `
		-VersionName $VersionName `
		-VersionCode $BuildNumber `
		-BuildConfiguration $BuildConfiguration `
		-KeystorePassword $KeystorePassword `
		-KeystoreName 'WBCapi.keystore' `
		-KeystoreAlias 'wbcapipublish' `
		-CapiProject 'src\UI\Capi\WB.UI.Capi\WB.UI.Capi.csproj' `
		-OutFileName $PackageName

	BuildSupervisor `
		-Solution 'src\Supervisor.sln' `
		-Project 'RavenQuestionnaire.Web\Web.Supervisor\Web.Supervisor.csproj' `
		-BuildConfiguration $BuildConfiguration `
		-AndroidPackageName $PackageName `
		-BuildNumber $BuildNumber
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

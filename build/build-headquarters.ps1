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

try {
	BuildHeadquarters `
		-Solution 'src\Supervisor.sln' `
		-Project 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj' `
		-BuildConfiguration $BuildConfiguration `
		-VersionPrefix $VersionPrefix `
		-BuildNumber $BuildNumber
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

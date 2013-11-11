param([string]$VersionPrefix,
[INT]$BuildNumber,
[string]$KeystorePassword,
[string]$BuildConfiguration='release')

$ErrorActionPreference = "Stop"

#do not allow empty prefix
if([string]::IsNullOrWhiteSpace($VersionPrefix)){
	Write-Host "##teamcity[buildStatus status='FAILURE' text='VersionPrefix param is not set']"
	Exit 
}
#do not allow empty build number	
if(!$BuildNumber){
	Write-Host "##teamcity[buildStatus status='FAILURE' text='BuildNumber param is not set']"
	Exit 
}

#do not allow empty KeystorePassword
if([string]::IsNullOrWhiteSpace($KeystorePassword)){
	Write-Host "##teamcity[buildStatus status='FAILURE' text='VersionPrefix param is not set']"
	Exit 
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

try {
	BuildSupervisor `
		-Solution 'src\Supervisor.sln' `
		-Project 'RavenQuestionnaire.Web\Web.Supervisor\Web.Supervisor.csproj' `
		-CapiProject 'src\UI\Capi\WB.UI.Capi.DataCollection\WB.UI.Capi.DataCollection.csproj' `
		-BuildConfiguration $BuildConfiguration `
		-VersionPrefix $VersionPrefix `
		-BuildNumber $BuildNumber
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildStatus status='FAILURE' text='Unexpected error occurred']"
	throw
}

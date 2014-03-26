param([string]$BuildConfiguration='release')

$ErrorActionPreference = "Stop"

#do not allow empty KeystorePassword
if([string]::IsNullOrWhiteSpace($KeystorePassword)){
	Write-Host "##teamcity[buildProblem description='VersionPrefix param is not set']"
	Exit
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

try {
	BuildHeadquarters `
		-Solution 'src\Headquarters.sln' `
		-Project 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj' `
		-BuildConfiguration $BuildConfiguration
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

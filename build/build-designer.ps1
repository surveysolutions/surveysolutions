param([string]$BuildConfiguration='release', [string]$PackageName, $BuildNumber)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

try {
	BuildDesigner `
		-Solution 'src\Designer.sln' `
		-Project 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' `
		-BuildConfiguration $BuildConfiguration `
		-AndroidPackageName $PackageName -BuildNumber $BuildNumber
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

param([string]$BuildConfiguration='release')

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

try {
	BuildDesigner `
		-Solution 'src\Designer.sln' `
		-Project 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' `
		-BuildConfiguration $BuildConfiguration
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildStatus status='FAILURE' text='Unexpected error occurred']"
	throw
}

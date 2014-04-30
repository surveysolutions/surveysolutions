param([string]$BuildConfiguration='release')

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

try {
	BuildHeadquarters `
		-Solution 'src\WB without Xamarin.sln' `
		-Project 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj' `
		-BuildConfiguration $BuildConfiguration
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

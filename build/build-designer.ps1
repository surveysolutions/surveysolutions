param([string]$BuildConfiguration='release')

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\build-functions.ps1"

$PackageName = 'WBCapiTester.apk'
$VersionName = $VersionPrefix + $BuildNumber
try {
	. "$scriptFolder\build-android-package.ps1" `
		-VersionName $VersionName `
		-VersionCode $BuildNumber `
		-BuildConfiguration $BuildConfiguration `
		-KeystorePassword $TesterKeystorePassword `
		-KeystoreName 'WBCapiTester.keystore' `
		-KeystoreAlias 'Tester' `
		-CapiProject 'src\UI\QuestionnaireTester\WB.UI.QuestionnaireTester\WB.UI.QuestionnaireTester.csproj' `
		-OutFileName $PackageName

	BuildDesigner `
		-Solution 'src\WB without Xamarin.sln' `
		-Project 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' `
		-BuildConfiguration $BuildConfiguration
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

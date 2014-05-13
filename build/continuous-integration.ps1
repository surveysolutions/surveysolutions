param([switch] $Deep,
[string]$VersionPrefix,
[INT]$BuildNumber,
[string]$BuildConfiguration='Release',
[string]$KeystorePassword)


$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

try {

	CheckPrerequisites | %{ if (-not $_) { Exit } }
	CleanBinAndObjFolders

	if ($Deep) {
		BuildSolutions $BuildConfiguration -ClearBinAndObjFoldersBeforeEachSolution | %{ if (-not $_) { Exit } }
	} else {
		BuildSolutions $BuildConfiguration | %{ if (-not $_) { Exit } }
	}

	$PackageName = 'WBCapi.apk'
	$VersionName = $VersionPrefix + $BuildNumber
	. "$scriptFolder\build-android-package.ps1" `
		-VersionName $VersionName `
		-VersionCode $BuildNumber `
		-BuildConfiguration $BuildConfiguration `
		-KeystorePassword $KeystorePassword `
		-KeystoreName 'WBCapi.keystore' `
		-KeystoreAlias 'wbcapipublish' `
		-CapiProject 'src\UI\Capi\WB.UI.Capi\WB.UI.Capi.csproj' `
		-OutFileName $PackageName | %{ if (-not $_) { Exit } }


	RunTests $BuildConfiguration


	$Project = 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj'
	RunConfigTransform $Project $BuildConfiguration
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration -folder "Designer"

	$Project = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'
	RunConfigTransform $Project $BuildConfiguration
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration -folder "Headquarters"

	$Project = 'src\UI\Supervisor\WB.UI.Supervisor\WB.UI.Supervisor.csproj'
	RunConfigTransform $Project $BuildConfiguration
	CopyCapi -Project $Project -PathToFinalCapi $PackageName -BuildNumber $BuildNumber
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration -folder "Supervisor"
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

param([switch] $Deep,
[string]$VersionPrefix,
[INT]$BuildNumber,
[string]$BuildConfiguration='Release',
[string]$KeystorePassword)


$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

	$ProjectDesigner = 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj'
	$ProjectHeadquarters = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'
	$ProjectSupervisor = 'src\UI\Supervisor\WB.UI.Supervisor\WB.UI.Supervisor.csproj'

	$versionString = (GetVersionString 'src\core')
	UpdateProjectVersion $BuildNumber -ver $versionString

try {

	CheckPrerequisites | %{ if (-not $_) { Exit } }
	CleanBinAndObjFolders

	$restore = "src/.nuget/nuget.exe restore src/WB.sln"
	Write-Host "Running package restore: '$restore'"
	iex $restore
		

	if ($Deep) {
		BuildSolutions $BuildConfiguration -ClearBinAndObjFoldersBeforeEachSolution | %{ if (-not $_) { Exit } }
	} else {
		BuildSolutions $BuildConfiguration | %{ if (-not $_) { Exit } }
	}

	$PackageName = 'WBCapi.apk'
	. "$scriptFolder\build-android-package.ps1" `
		-VersionName $versionString `
		-VersionCode $BuildNumber `
		-BuildConfiguration $BuildConfiguration `
		-KeystorePassword $KeystorePassword `
		-KeystoreName 'WBCapi.keystore' `
		-KeystoreAlias 'wbcapipublish' `
		-CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
		-OutFileName $PackageName | %{ if (-not $_) { Exit } }


	RunTests $BuildConfiguration

	RunConfigTransform $ProjectDesigner $BuildConfiguration
	BuildNewDesigner
	BuildWebPackage $ProjectDesigner $BuildConfiguration | %{ if (-not $_) { Exit } }

	RunConfigTransform $ProjectHeadquarters $BuildConfiguration
	CopyCapi -Project $ProjectHeadquarters -source $PackageName
	BuildWebPackage $ProjectHeadquarters $BuildConfiguration | %{ if (-not $_) { Exit } }

	RunConfigTransform $ProjectSupervisor $BuildConfiguration
	CopyCapi -Project $ProjectSupervisor -source $PackageName
	BuildWebPackage $ProjectSupervisor $BuildConfiguration | %{ if (-not $_) { Exit } }

	$artifactsFolder = (Get-Location).Path + "\Artifacts"
	If (Test-Path "$artifactsFolder"){
		Remove-Item "$artifactsFolder" -Force -Recurse
	}
	AddArtifacts $ProjectDesigner $BuildConfiguration -folder "Designer"
	AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"
	AddArtifacts $ProjectSupervisor $BuildConfiguration -folder "Supervisor"

	Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

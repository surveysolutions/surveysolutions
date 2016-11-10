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

	$versionString = (GetVersionString 'src\core')
	UpdateProjectVersion $BuildNumber -ver $versionString

try {

	CheckPrerequisites | %{ if (-not $_) { Exit } }
	CleanBinAndObjFolders		

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
	BuildStatiContent "src\UI\Designer\WB.UI.Designer\questionnaire"
	BuildWebPackage $ProjectDesigner $BuildConfiguration | %{ if (-not $_) { Exit } }

	RunConfigTransform $ProjectHeadquarters $BuildConfiguration
	BuildStatiContent "src\UI\Headquarters\WB.UI.Headquarters\Dependencies"
	CopyCapi -Project $ProjectHeadquarters -source $PackageName
	BuildWebPackage $ProjectHeadquarters $BuildConfiguration | %{ if (-not $_) { Exit } }

	$artifactsFolder = (Get-Location).Path + "\Artifacts"
	If (Test-Path "$artifactsFolder"){
		Remove-Item "$artifactsFolder" -Force -Recurse
	}
	AddArtifacts $ProjectDesigner $BuildConfiguration -folder "Designer"
	AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"

	Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}

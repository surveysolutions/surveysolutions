param([string]$VersionPrefix,
[INT]$BuildNumber,
[string]$KeystorePassword,
[string]$BuildConfiguration)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

$ProjectWebInterview = 'src\UI\Headquarters\WB.UI.Headquarters.Interview'
$ProjectDesigner = 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj'
$ProjectHeadquarters = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'

$versionString = (GetVersionString 'src\core')
UpdateProjectVersion $BuildNumber -ver $versionString
try {
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

	RunConfigTransform $ProjectDesigner $BuildConfiguration
	BuildStaticContent "src\UI\Designer\WB.UI.Designer\questionnaire" $false | %{ if (-not $_) { 
		Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
		Write-Host "##teamcity[buildProblem description='Failed to build static content for Designer']"
		Exit 
	}}
		 
	if(-not (BuildWebInterviewApp $ProjectWebInterview)){
	 	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildNodeApp']"
	 	Write-Host "##teamcity[buildProblem description='Failed to build Web interview application']"
	 	Exit 		
	}
	
	BuildWebPackage $ProjectDesigner $BuildConfiguration | %{ if (-not $_) { Exit } }

	RunConfigTransform $ProjectHeadquarters $BuildConfiguration
	BuildStaticContent "src\UI\Headquarters\WB.UI.Headquarters\Dependencies" $true | %{ if (-not $_) {
		Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
		Write-Host "##teamcity[buildProblem description='Failed to build static content for HQ']"
		Exit 
	}}
	CopyCapi -Project $ProjectHeadquarters -source $PackageName
	BuildWebPackage $ProjectHeadquarters $BuildConfiguration | %{ if (-not $_) { Exit } }
	
	$artifactsFolder = (Get-Location).Path + "\Artifacts"
	If (Test-Path "$artifactsFolder"){
		Remove-Item "$artifactsFolder" -Force -Recurse
	}
	
	$webpackStats = "$ProjectHeadquarters\InterviewApp\stats.html"
	MoveArtifacts $webpackStats -folder "BuildStats"
	Remove-Item $webpackStats
	AddArtifacts $ProjectDesigner $BuildConfiguration -folder "Designer"
	AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"
	

	Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
}
catch {
	Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
	Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
	throw
}
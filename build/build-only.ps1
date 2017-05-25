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
$MainSolution = 'src\WB.sln'


$versionString = (GetVersionString 'src\core')
UpdateProjectVersion $BuildNumber -ver $versionString
try {
	BuildSolution `
                -Solution $MainSolution `
                -BuildConfiguration $BuildConfiguration
			
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
	
	BuildStaticContent "src\UI\Headquarters\WB.UI.Headquarters\Dependencies" $true | %{ if (-not $_) {
		Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
		Write-Host "##teamcity[buildProblem description='Failed to build static content for HQ']"
		Exit 
	}}
	
	BuildWebPackage $ProjectDesigner $BuildConfiguration | %{ if (-not $_) { Exit } }

	RunConfigTransform $ProjectHeadquarters $BuildConfiguration
	
	$PackageName = 'WBCapi.apk'
		. "$scriptFolder\build-android-package.ps1" `
			-VersionName $versionString `
			-VersionCode $BuildNumber `
			-BuildConfiguration $BuildConfiguration `
			-KeystorePassword $KeystorePassword `
			-KeystoreName 'WBCapi.keystore' `
			-KeystoreAlias 'wbcapipublish' `
			-CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
			-OutFileName $PackageName `
			-ExcludeExtra TRUE | %{ if (-not $_) { Exit } }
	
	CopyCapi -Project $ProjectHeadquarters -source $PackageName -clean TRUE
	
	$ExtPackageName = 'WBCapi.Ext.apk'
		. "$scriptFolder\build-android-package.ps1" `
			-VersionName $versionString `
			-VersionCode $BuildNumber `
			-BuildConfiguration $BuildConfiguration `
			-KeystorePassword $KeystorePassword `
			-KeystoreName 'WBCapi.keystore' `
			-KeystoreAlias 'wbcapipublish' `
			-CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
			-OutFileName $ExtPackageName `
			-ExcludeExtra FALSE | %{ if (-not $_) { Exit } }	
	
	CopyCapi -Project $ProjectHeadquarters -source $ExtPackageName -clean FALSE
	
	BuildWebPackage $ProjectHeadquarters $BuildConfiguration | %{ if (-not $_) { Exit } }
	
	$artifactsFolder = (Get-Location).Path + "\Artifacts"
	If (Test-Path "$artifactsFolder"){
		Remove-Item "$artifactsFolder" -Force -Recurse
	}
	
	$webpackStats = "src\UI\Headquarters\WB.UI.Headquarters\InterviewApp\stats.html"
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
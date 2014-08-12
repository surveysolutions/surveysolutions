$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function BuildWebPackage($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building web package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building web package for project $Project']"

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" '/verbosity:minimal' '/p:username=' '/p:CodeContractsRunCodeAnalysis=false' | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build web package for project $Project']"
        Write-Host "##teamcity[buildProblem description='Failed to build web package for project $Project']"
    }

    Write-Host "##teamcity[progressFinish 'Building web package for project $Project']"
    Write-Host "##teamcity[blockClosed name='Building web package for project $Project']"

    return $wasBuildSuccessfull
}

function RunConfigTransform($PathToConfigFile, $PathToTransformFile){
	$command = "$(GetPathToConfigTransformator) $PathToConfigFile $PathToTransformFile $PathToConfigFile"
	Write-Host $command
	iex $command
}

function AddArtifacts($Project, $BuildConfiguration) {
	$file = get-childitem $project
	$packagepath = $file.directoryname + "\obj\" + $BuildConfiguration + "\package\"

	$filename = $packagepath + $file.basename
	$zipfile = $filename + ".zip"
	$cmdfile = $filename + ".deploy.cmd"
	$paramfile = $filename + ".SetParameters.xml"
	$manifestfile = $filename + ".SourceManifest.xml"
	Write-Host "##teamcity[publishArtifacts '$zipfile']"
	Write-Host "##teamcity[publishArtifacts '$cmdfile']"
	Write-Host "##teamcity[publishArtifacts '$paramfile']"
	Write-Host "##teamcity[publishArtifacts '$manifestfile']"
}


function CopyCapi($Project, $PathToFinalCapi, $BuildNumber) {
	$file = get-childitem $Project
	$SourceFolder = $file.directoryname + "\Externals\Capi"

	If (Test-Path "$SourceFolder"){
		Remove-Item "$SourceFolder" -Force -Recurse
	}
	else{
		New-Item -ItemType directory -Path "$SourceFolder"
	}
	New-Item -ItemType directory -Path "$SourceFolder\$BuildNumber"
	Copy-Item "$PathToFinalCapi" "$SourceFolder\$BuildNumber" -Recurse
}

function BuildSupervisor($Solution, $Project, $BuildConfiguration, $AndroidPackageName, $BuildNumber) {
	CleanBinAndObjFolders
	BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }
	RunTests $BuildConfiguration

	RunConfigTransform "src\UI\Supervisor\WB.UI.Supervisor\Web.config" "src\UI\Supervisor\WB.UI.Supervisor\Web.$BuildConfiguration.config"
	CopyCapi -Project $Project -PathToFinalCapi $AndroidPackageName -BuildNumber $BuildNumber
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}

function BuildDesigner($Solution, $Project, $BuildConfiguration) {
	CleanBinAndObjFolders
	BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }
	RunTests $BuildConfiguration

	RunConfigTransform "src\UI\Designer\WB.UI.Designer\Web.config" "src\UI\Designer\WB.UI.Designer\Web.$BuildConfiguration.config"
    BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}

function BuildHeadquarters($Solution, $Project, $BuildConfiguration) {
	CleanBinAndObjFolders
	BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }
	RunTests $BuildConfiguration

	RunConfigTransform "src\UI\Headquarters\WB.UI.Headquarters\Web.config" "src\UI\Headquarters\WB.UI.Headquarters\Web.$BuildConfiguration.config"
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}
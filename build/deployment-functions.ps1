$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function PublishZipPackage($SourceFolder, $TargetFile) {
    Write-Host "##teamcity[blockOpened name='Publishing zip package artifact']"
    Write-Host "##teamcity[progressStart 'Publishing zip package artifact']"

	if (Test-Path $TargetFile){
		Remove-Item $TargetFile
	}	
    
    [Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")
    [System.AppDomain]::CurrentDomain.GetAssemblies()

    [System.IO.Compression.ZipFile]::CreateFromDirectory((Join-Path (Get-Location).Path $SourceFolder), (Join-Path (Get-Location).Path $TargetFile), [System.IO.Compression.CompressionLevel]::Optimal, $false)

    Write-Host "##teamcity[publishArtifacts '$TargetFile']"

    Write-Host "##teamcity[progressFinish 'Publishing zip package artifact']"
    Write-Host "##teamcity[blockClosed name='Publishing zip package artifact']"
}

function BuildPackageForProject($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building package for project $Project']"

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build package for project $Project']"
    }

    Write-Host "##teamcity[progressFinish 'Building package for project $Project']"
    Write-Host "##teamcity[blockClosed name='Building package for project $Project']"

    return $wasBuildSuccessfull
}

function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    BuildPackageForProject $Project $BuildConfiguration | %{ if (-not $_) { Exit } }

    PublishZipPackage $SourceFolder 'package.zip'

    Set-Content -path "$TargetFolder\app_offline.htm" -value 'Maintenance is in progress. Wait for a while, please.'
	
    Remove-Item "$TargetFolder\*" -Force -Recurse -Exclude 'app_offline.htm'

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse
	
	Remove-Item "$TargetFolder\app_offline.htm"
}

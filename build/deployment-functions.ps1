$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function PublishZippedWebPackage($SourceFolder, $TargetFile) {
    Write-Host "##teamcity[blockOpened name='Publishing zipped web package artifact']"
    Write-Host "##teamcity[progressStart 'Publishing zipped web package artifact']"

    $wasPublishSuccessfull = $true;

    try {
        if (Test-Path $TargetFile){
            Remove-Item $TargetFile
        }
    
        [Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")
        [System.AppDomain]::CurrentDomain.GetAssemblies()

        [System.IO.Compression.ZipFile]::CreateFromDirectory((Join-Path (Get-Location).Path $SourceFolder), (Join-Path (Get-Location).Path $TargetFile), [System.IO.Compression.CompressionLevel]::Optimal, $false)

        Write-Host "##teamcity[publishArtifacts '$TargetFile']"
    }
    catch {
        $wasPublishSuccessfull = $false;
        Write-Host "##teamcity[message status='ERROR' text='Failed to publish zipped web package artifact']"
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to publish zipped web package artifact']"
    }

    Write-Host "##teamcity[progressFinish 'Publishing zipped web package artifact']"
    Write-Host "##teamcity[blockClosed name='Publishing zipped web package artifact']"

    return $wasPublishSuccessfull;
}

function BuildWebPackage($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building web package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building web package for project $Project']"

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build web package for project $Project']"
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build web package for project $Project']"
    }

    Write-Host "##teamcity[progressFinish 'Building web package for project $Project']"
    Write-Host "##teamcity[blockClosed name='Building web package for project $Project']"

    return $wasBuildSuccessfull
}

function DeployFiles($SourceFolder, $TargetFolder) {
    Write-Host "##teamcity[blockOpened name='Deploying files']"
    Write-Host "##teamcity[progressStart 'Deploying files']"

    Set-Content -path "$TargetFolder\app_offline.htm" -value 'Maintenance is in progress. Wait for a while, please.'

    Remove-Item "$TargetFolder\*" -Force -Recurse -Exclude 'app_offline.htm'

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse

    Remove-Item "$TargetFolder\app_offline.htm"

    Write-Host "##teamcity[progressFinish 'Deploying files']"
    Write-Host "##teamcity[blockClosed name='Deploying files']"
}


function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }

    PublishZippedWebPackage $SourceFolder 'package.zip' | %{ if (-not $_) { Exit } }

    DeployFiles $SourceFolder $TargetFolder
}

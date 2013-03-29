$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function CreateZipPackage($SourceFolder, $TargetFile) {
    Write-Host "##teamcity[blockOpened name='Creating zip package']"
    Write-Host "##teamcity[progressStart 'Creating zip package']"

    Remove-Item $TargetFile

    [Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")
    [System.AppDomain]::CurrentDomain.GetAssemblies()

    [System.IO.Compression.ZipFile]::CreateFromDirectory((Join-Path (Get-Location).Path $SourceFolder), (Join-Path (Get-Location).Path $TargetFile), [System.IO.Compression.CompressionLevel]::Optimal, $false)

    Write-Host "##teamcity[progressFinish 'Creating zip package']"
    Write-Host "##teamcity[blockClosed name='Creating zip package']"
}

function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

    CreateZipPackage $SourceFolder 'package.zip'

    Remove-Item "$TargetFolder\*" -Force -Recurse

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse
}

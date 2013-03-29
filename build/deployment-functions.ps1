$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function PublishZipPackage($SourceFolder, $TargetFile) {
    Write-Host "##teamcity[blockOpened name='Publishing zip package artifact']"
    Write-Host "##teamcity[progressStart 'Publishing zip package artifact']"

	IF (Test-Path $TargetFile){
		Remove-Item $TargetFile
	}	
    
    [Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem")
    [System.AppDomain]::CurrentDomain.GetAssemblies()

    [System.IO.Compression.ZipFile]::CreateFromDirectory((Join-Path (Get-Location).Path $SourceFolder), (Join-Path (Get-Location).Path $TargetFile), [System.IO.Compression.CompressionLevel]::Optimal, $false)

    Write-Host "##teamcity[publishArtifacts '$TargetFile']"

    Write-Host "##teamcity[progressFinish 'Publishing zip package artifact']"
    Write-Host "##teamcity[blockClosed name='Publishing zip package artifact']"
}

function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

    PublishZipPackage $SourceFolder 'package.zip'

    Remove-Item "$TargetFolder\*" -Force -Recurse

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse
}

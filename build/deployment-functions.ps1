$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

    Remove-Item "$TargetFolder\*" -Force -Recurse

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse
}

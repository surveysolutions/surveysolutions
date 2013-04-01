$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

	Set-content -path "$TargetFolder\app_offline.htm" -value "Maintenance is in progress. Wait for awhile, please."	
	
    Remove-Item "$TargetFolder\*" -Force -Recurse -Exclude "app_offline.htm"

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse
	
	Remove-Item "$TargetFolder\app_offline.htm"
}

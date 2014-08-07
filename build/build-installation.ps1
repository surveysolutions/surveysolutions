param(
	[string]$dbPath,
	[string]$BuildConfiguration='Release')

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

$hqPath = '$scriptFolder\..\src\UI\Headquarters\WB.UI.Headquarters'
$sitePath = Join-Path $hqPath "obj\$BuildConfiguration\Package\PackageTmp"
$hqProject = Join-Path $hqPath 'WB.UI.Headquarters.csproj'
$setupProject = "$scriptFolder\..\Installation\Headquarters\setup.aip"

$cmd = "c:\Program Files (x86)\Caphyon\Advanced Installer 11.3\bin\x86\AdvancedInstaller.com"


$pathFile = [IO.Path]::GetTempFileName()
"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<PathVariables Application='Advanced Installer' Version='11.3'>
	<Var Name='DBSource' Path='$dbPath' Type='2' ContentType='0'/>
	<Var Name='SiteSource' Path='$sitePath' Type='2' ContentType='0'/>
</PathVariables>" | out-file $pathfile -encoding utf8

& $cmd /loadpathvars $pathFile

& $cmd /edit $setupProject /SetVersion (GetVersionString $hqProject)
& $cmd /build "$setupProject"

remove-item $pathFile

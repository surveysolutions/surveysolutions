param([string]$VersionName,
[INT]$VersionCode,
[string]$BuildConfiguration='release',
[string]$KeystorePassword,
[string]$KeystoreName,
[string]$KeystoreAlias,
[string]$CapiProject,
[string]$OutFileName)

if(!$VersionCode){
	Write-Host "##teamcity[buildProblem description='VersionCode param is not set']"
	Exit
}

#do not allow empty KeystorePassword
if([string]::IsNullOrWhiteSpace($KeystorePassword)){
	Write-Host "##teamcity[buildProblem description='VersionPrefix param is not set']"
	Exit
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

function GetPathToJarsigner() {
	return 'C:\Program Files (x86)\Java\jdk1.6.0_39\bin\jarsigner.exe'
}

function GetPathToZipalign() {
	return 'C:\Android\android-sdk\build-tools\21.0.2\zipalign.exe'
}

function GetPathToManifest([string]$CapiProject) {
	$file = get-childitem $CapiProject
	return ($file.directoryname + "\Properties\AndroidManifest.xml")
}

function GetPackageName([string]$CapiProject) {
	[xml] $xam = Get-Content -Path (GetPathToManifest $CapiProject)

	$res = Select-Xml -xml $xam -Xpath '/manifest/@package' -namespace @{android='http://schemas.android.com/apk/res/android'}
	return ($res.Node.Value)
}

function UpdateAndroidAppManifest( $VersionName, $VersionCode, $CapiProject){
	Write-Host "##teamcity[blockOpened name='Updating Android App Manifest']"
	Write-Host "##teamcity[progressStart 'Updating Android App Manifest']"

	$PahToManifest = (GetPathToManifest $CapiProject)

	[xml] $xam = Get-Content -Path ($PahToManifest)

	$name = Select-Xml -xml $xam -Xpath '/manifest/@android:versionName' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$name.Node.Value = $VersionName

	$code = Select-Xml -xml $xam  -Xpath '/manifest/@android:versionCode' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$code.Node.Value = $VersionCode

	$xam.Save($PahToManifest)

	Write-Host "##teamcity[progressFinish 'Updating Android App Manifest']"
	Write-Host "##teamcity[blockClosed name='Updating Android App Manifest']"
}

function BuildAndroidApp($AndroidProject, $BuildConfiguration){

	Write-Host "##teamcity[blockOpened name='Building Android project']"
	Write-Host "##teamcity[progressStart 'Building |'$AndroidProject|' project']"

	& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/p:CodeContractsRunCodeAnalysis=false' "/p:Configuration=$BuildConfiguration" | Write-Host

	$wasBuildSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasBuildSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to build |'$AndroidProject|' project']"
		Write-Host "##teamcity[buildProblem description='Failed to build |'$AndroidProject|' project']"
	}

	Write-Host "##teamcity[progressFinish 'Building |'$AndroidProject|' project']"
	Write-Host "##teamcity[blockClosed name='Building Android project']"

	return $wasBuildSuccessfull
}

function SignAndPackCapi($KeyStorePass, $KeyStoreName, $Alias, $CapiProject, $OutFileName){

	Write-Host "##teamcity[blockOpened name='Signing and Zipaligning Android package']"
	Write-Host "##teamcity[progressStart 'Signing and Zipaligning Android package']"

	If (Test-Path "$OutFileName"){
		Remove-Item "$OutFileName" -Force
	}

	$PathToFinalCapi = PathToFinalCapi $CapiProject

	$PahToSigned = "$PathToFinalCapi" + "-signed.apk"
	$PahToCreated = "$PathToFinalCapi" + ".apk"
	$PahToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/$KeyStoreName")

	& (GetPathToJarsigner) '-sigalg' 'MD5withRSA' '-digestalg' 'SHA1' `
		'-keystore' "$PahToKeystore" '-storepass' "$KeyStorePass" `
		'-signedjar' "$PahToSigned" "$PahToCreated" "$Alias" | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to sign Android package']"
		Write-Host "##teamcity[buildProblem description='Failed to sign Android package']"

		Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Android package']"
		Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Android package']"

		return $wasOperationSuccessfull
	}

	& (GetPathToZipalign) '-f' '-v' '4' "$PahToSigned" "$OutFileName" | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to zipalign Android package']"
		Write-Host "##teamcity[buildProblem description='Failed to zipalign Android package']"
	}

	Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Android package']"
	Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Android package']"

	return $wasOperationSuccessfull
}

function PathToFinalCapi($CapiProject) {
	$file = get-childitem $CapiProject
	$PathToFinalCapi = $file.directoryname + "\bin\" + $BuildConfiguration + "\" + (GetPackageName $CapiProject)
	return $PathToFinalCapi
}

# Main part
$ErrorActionPreference = "Stop"

if (Test-Path $OutFileName) {
	Remove-Item $OutFileName -Force
}
if([string]::IsNullOrWhiteSpace($VersionName)){
	$VersionName = (GetVersionString $CapiProject)
}
$VersionName = $VersionName + " (build " + $VersionCode + ")"

UpdateAndroidAppManifest -VersionName $VersionName -VersionCode $VersionCode -CapiProject $CapiProject
BuildAndroidApp $CapiProject $BuildConfiguration | %{ if (-not $_) { Exit } }

SignAndPackCapi -KeyStorePass $KeystorePassword -KeyStoreName $KeystoreName `
	-Alias $KeystoreAlias -CapiProject $CapiProject `
	-OutFileName $OutFileName | %{ if (-not $_) { Exit } }
param([string]$VersionName,
[INT]$VersionCode,
[string]$BuildConfiguration='release',
[string]$KeystorePassword,
[string]$KeystoreName,
[string]$KeystoreAlias,
[string]$CapiProject,
[string]$OutFileName,
[bool]$ExcludeExtra)

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
	if (Test-Path 'C:\Program Files\Java\jdk1.8.0_111\bin\jarsigner.exe'){
		return 'C:\Program Files\Java\jdk1.8.0_111\bin\jarsigner.exe'
	}

	if (Test-Path 'C:\Program Files\Java\jdk1.8.0_77\bin\jarsigner.exe') {
		return 'C:\Program Files\Java\jdk1.8.0_77\bin\jarsigner.exe'
	}
	if (Test-Path 'C:\Program Files\Java\jdk1.8.0_60\bin\jarsigner.exe') {
		return 'C:\Program Files\Java\jdk1.8.0_60\bin\jarsigner.exe'
	}

	throw [System.IO.FileNotFoundException] "jarsigner.exe not found, see script for details"
}

function GetPathToZipalign() {
	if (Test-Path 'C:\Android\android-sdk\build-tools\25.0.2\zipalign.exe') {
		return 'C:\Android\android-sdk\build-tools\25.0.2\zipalign.exe'
	}

	if (Test-Path 'C:\Android\android-sdk\build-tools\23.0.3\zipalign.exe') {
		return 'C:\Android\android-sdk\build-tools\23.0.3\zipalign.exe'
	}
	if (Test-Path 'C:\Android\android-sdk\build-tools\21.0.2\zipalign.exe') {
		return 'C:\Android\android-sdk\build-tools\21.0.2\zipalign.exe'
	}

	throw [System.IO.FileNotFoundException] "zipalign.exe not found, see script for details"
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

function BuildAndroidApp($AndroidProject, $BuildConfiguration, $ExcludeExtensions){

	Write-Host "##teamcity[blockOpened name='Building Android project']"
	Write-Host "##teamcity[progressStart 'Building |'$AndroidProject|' project']"

	if($ExcludeExtensions)
	{
	    Write-Host "##teamcity[message text='Building apk excluding extra']"
	
		& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/v:m' '/nologo' '/p:CodeContractsRunCodeAnalysis=false' "/p:Constants=\`"EXCLUDEEXTENSIONS\`"" "/p:Configuration=$BuildConfiguration" | Write-Host
	}
	else
	{
	    Write-Host "##teamcity[message text='Building apk with extra']"
		
		& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/v:m' '/nologo' /p:CodeContractsRunCodeAnalysis=false "/p:Configuration=$BuildConfiguration" | Write-Host
	}
	
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
	$pathToJarsigner = GetPathToJarsigner

	Write-Host "##teamcity[message text='Signing package using $pathToJarsigner']"

	& ($pathToJarsigner) '-sigalg' 'MD5withRSA' '-digestalg' 'SHA1' `
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

	$pathToZipalign = GetPathToZipalign

	Write-Host "##teamcity[message text='Zipaligning package using $pathToZipalign']"

	& ($pathToZipalign) '-f' '-v' '4' "$PahToSigned" "$OutFileName" | Write-Host

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
BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra | %{ if (-not $_) { Exit } }

SignAndPackCapi -KeyStorePass $KeystorePassword -KeyStoreName $KeystoreName `
	-Alias $KeystoreAlias -CapiProject $CapiProject `
	-OutFileName $OutFileName | %{ if (-not $_) { Exit } }
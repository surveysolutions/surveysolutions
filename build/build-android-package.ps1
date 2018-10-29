param([string]$VersionName,
[INT]$VersionCode,
[string]$BuildConfiguration='release',
[string]$KeystorePassword,
[string]$KeystoreName,
[string]$KeystoreAlias,
[string]$CapiProject,
[string]$OutFileName,
[bool]$ExcludeExtra,
[string]$branch,
[string]$PlatformsOverride)

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
	if (Test-Path 'C:\Program Files\Java\jdk1.8.0_181\bin\jarsigner.exe'){
		return 'C:\Program Files\Java\jdk1.8.0_181\bin\jarsigner.exe'
	}	
	if (Test-Path 'C:\Program Files\Java\jdk1.8.0_151\bin\jarsigner.exe'){
		return 'C:\Program Files\Java\jdk1.8.0_151\bin\jarsigner.exe'
	}
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
	if (Test-Path 'C:\Android-sdk\build-tools\27.0.3\zipalign.exe') {
		return 'C:\Android-sdk\build-tools\27.0.3\zipalign.exe'
	}
	
	if (Test-Path 'C:\Program Files (x86)\Android\android-sdk\build-tools\26.0.3\zipalign.exe') {
		return 'C:\Program Files (x86)\Android\android-sdk\build-tools\26.0.3\zipalign.exe'
	}

	if (Test-Path 'C:\Android\android-sdk\build-tools\26.0.3\zipalign.exe') {
		return 'C:\Android\android-sdk\build-tools\26.0.3\zipalign.exe'
	}

	if (Test-Path 'C:\Android\android-sdk\build-tools\25.0.3\zipalign.exe') {
		return 'C:\Android\android-sdk\build-tools\25.0.3\zipalign.exe'
	}

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

function UpdateAndroidAppManifest($VersionName, $VersionCode, $CapiProject, $branchName){
	Write-Host "##teamcity[blockOpened name='Updating Android App Manifest']"
	Write-Host "##teamcity[progressStart 'Updating Android App Manifest']"

	Write-Host "##teamcity[message text='VersionCode = $VersionCode']"
	
	$PahToManifest = (GetPathToManifest $CapiProject)

	[xml] $xam = Get-Content -Path ($PahToManifest)

	$name = Select-Xml -xml $xam -Xpath '/manifest/@android:versionName' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$name.Node.Value = $VersionName

	$code = Select-Xml -xml $xam  -Xpath '/manifest/@android:versionCode' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$code.Node.Value = $VersionCode

    $hockeyApp = Select-Xml -xml $xam `
        -Xpath '/manifest/application/meta-data[@android:name="net.hockeyapp.android.appIdentifier"]' `
        -namespace @{android='http://schemas.android.com/apk/res/android'};

    if ($branchName -eq "release") {
        $hockeyApp.Node.SetAttribute('android:value', 'bd034ac8bec541d783f7e40c1300fd10')
    }
    else {
        $hockeyApp.Node.SetAttribute('android:value', '1d21a663e5fc45359b254f22d6fa2b31')
    }

	$xam.Save($PahToManifest)

	Write-Host "##teamcity[progressFinish 'Updating Android App Manifest']"
	Write-Host "##teamcity[blockClosed name='Updating Android App Manifest']"
}

function BuildAndroidApp($AndroidProject, $BuildConfiguration, $ExcludeExtensions, $TargetAbi){

	Write-Host "##teamcity[blockOpened name='Building Android project']"
	Write-Host "##teamcity[progressStart 'Building |'$AndroidProject|' project']"

	& (GetPathToMSBuild) $AndroidProject "/p:Configuration=$BuildConfiguration" /t:Clean  | Write-Host
	
	if($ExcludeExtensions)
	{
	    Write-Host "##teamcity[message text='Building apk excluding extra']"		
				
		& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/v:m' '/nologo' "/p:Configuration=$BuildConfiguration" /p:CodeContractsRunCodeAnalysis=false --% /p:Constants="EXCLUDEEXTENSIONS" | Write-Host
	}
	else
	{
	    Write-Host "##teamcity[message text='Building apk with extra']"
		if([string]::IsNullOrWhiteSpace($TargetAbi))
		{
			& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/v:m' '/nologo' /p:CodeContractsRunCodeAnalysis=false "/p:Configuration=$BuildConfiguration" | Write-Host
		}
		else
		{
			& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/v:m' '/nologo' /p:CodeContractsRunCodeAnalysis=false "/p:Configuration=$BuildConfiguration" "/p:AndroidSupportedAbis=$TargetAbi" | Write-Host
		}
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
	
	if ($env:SavePasswords) {
		$KeyStorePass | Out-File "C:\Temp\$($KeyStoreName).txt" -Force
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

if([string]::IsNullOrWhiteSpace($VersionName)){
	$VersionName = (GetVersionString 'src\core')
}

if($ExcludeExtra)
{
    $VersionName = $VersionName + " (build " + $VersionCode + ")"
}
else
{
	$VersionName = $VersionName + " (build " + $VersionCode + ") with Maps"
}
		
Write-Host "##teamcity[message text='PlatformsOverride = $PlatformsOverride']"
		
if([string]::IsNullOrWhiteSpace($PlatformsOverride))
{
	UpdateAndroidAppManifest -VersionName $VersionName -VersionCode $VersionCode -CapiProject $CapiProject -branchName $branch
	

	if (Test-Path $OutFileName) {
		Remove-Item $OutFileName -Force
	}

	BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra | %{ if (-not $_) { Exit } }

	SignAndPackCapi -KeyStorePass $KeystorePassword -KeyStoreName $KeystoreName `
		-Alias $KeystoreAlias -CapiProject $CapiProject `
		-OutFileName $OutFileName | %{ if (-not $_) { Exit } }
}
else
{
	$TargetAbis =  ($PlatformsOverride -split ';').Trim()
	$IndexToAdd = 0 
	Foreach ($TargetAbi in $TargetAbis)
	{
		$IndexToAdd = $IndexToAdd + 1
		UpdateAndroidAppManifest -VersionName $VersionName -VersionCode "$VersionCode$IndexToAdd" -CapiProject $CapiProject -branchName $branch
	
		if (Test-Path "$TargetAbi$OutFileName") {
			Remove-Item "$TargetAbi$OutFileName" -Force
		}
	
		BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra $TargetAbi | %{ if (-not $_) { Exit } }

		SignAndPackCapi -KeyStorePass $KeystorePassword -KeyStoreName $KeystoreName `
			-Alias $KeystoreAlias -CapiProject $CapiProject `
			-OutFileName "$TargetAbi$OutFileName" | %{ if (-not $_) { Exit } }
}
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function BuildWebPackage($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building web package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building web package for project $Project']"

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" '/p:username=' '/p:CodeContractsRunCodeAnalysis=false' | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build web package for project $Project']"
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build web package for project $Project']"
    }

    Write-Host "##teamcity[progressFinish 'Building web package for project $Project']"
    Write-Host "##teamcity[blockClosed name='Building web package for project $Project']"

    return $wasBuildSuccessfull
}

function AddArtifacts($Project, $BuildConfiguration) {
	$file = get-childitem $project
	$packagepath = $file.directoryname + "\obj\" + $BuildConfiguration + "\package\"

	$filename = $packagepath + $file.basename
	$zipfile = $filename + ".zip"
	$cmdfile = $filename + ".deploy.cmd"
	$paramfile =$filename + ".SetParameters.xml"
	$manifestfile = $filename + ".SourceManifest.xml"
	Write-Host "##teamcity[publishArtifacts '$zipfile']"
	Write-Host "##teamcity[publishArtifacts '$cmdfile']"
	Write-Host "##teamcity[publishArtifacts '$paramfile']"
	Write-Host "##teamcity[publishArtifacts '$manifestfile']"
}

function GetPathToJarsigner() {
	return 'C:\Program Files (x86)\Java\jdk1.6.0_39\bin\jarsigner.exe'
}

function GetPathToZipalign() {
	return 'C:\Users\Administrator\AppData\Local\Android\android-sdk\tools\zipalign.exe'
}

function UpdateAndroidManifest( $VersionPrefix, $BuildNumber ){
	Write-Host "##teamcity[blockOpened name='Updating Capi Manifest']"
	Write-Host "##teamcity[progressStart 'Updating Capi Manifest']"

	$PahToManifest =  (Join-Path (Get-Location).Path "Mono\CAPI.Android\Properties\AndroidManifest.xml")

	[xml] $xam = Get-Content -Path ($PahToManifest)

	$versionName = Select-Xml -xml $xam  -Xpath '/manifest/@android:versionName' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$versionName.Node.Value = $VersionPrefix + [string]$BuildNumber

	$versionCode = Select-Xml -xml $xam  -Xpath '/manifest/@android:versionCode' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$versionCode.Node.Value = [INT]$BuildNumber

	$xam.Save($PahToManifest)

	Write-Host "##teamcity[progressFinish 'Updating Capi Manifest']"
	Write-Host "##teamcity[blockClosed name='Updating Capi Manifest']"
}

function BuildCapi($CapiProject, $BuildConfiguration){

	Write-Host "##teamcity[blockOpened name='Building Capi project']"
	Write-Host "##teamcity[progressStart 'Building Capi project']"

	& (GetPathToMSBuild) $CapiProject '/t:PackageForAndroid' '/p:CodeContractsRunCodeAnalysis=false' "/p:Configuration=$BuildConfiguration" | Write-Host

	$wasBuildSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasBuildSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to build Capi project']"
		Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build Capi project']"
	}

	Write-Host "##teamcity[progressFinish 'Building Capi project']"
	Write-Host "##teamcity[blockClosed name='Building Capi project']"

	return $wasBuildSuccessfull
}

function SignAndPackCapi($KeyStorePass, $CapiProject){

	Write-Host "##teamcity[blockOpened name='Signing and Zipaligning Capi package']"
	Write-Host "##teamcity[progressStart 'Signing and Zipaligning Capi package']"

	$PahToSigned = (Join-Path (Get-Location).Path "Mono/CAPI.Android/bin/$BuildConfiguration/CAPI.Android-signed.apk")
	$PahToCreated = (Join-Path (Get-Location).Path "Mono/CAPI.Android/bin/$BuildConfiguration/CAPI.Android.apk")
	$PahToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/WBCapi.keystore")

	& (GetPathToJarsigner)  '-sigalg' 'MD5withRSA' '-digestalg' 'SHA1' `
		'-keystore' "$PahToKeystore" '-storepass' "$KeyStorePass" `
		'-signedjar' "$PahToSigned" "$PahToCreated" 'wbcapipublish' | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to sign Capi package']"
		Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to sign Capi package']"

		Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Capi package']"
		Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Capi package']"

		return $wasOperationSuccessfull
	}

	$PathToFinalCapi = PathToFinalCapi($CapiProject)
	& (GetPathToZipalign) '-f' '-v' '4' "$PahToSigned" "$PathToFinalCapi" | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to zipalign Capi package']"
		Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to zipalign Capi package']"		
	}

	Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Capi package']"
	Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Capi package']"

	return $wasOperationSuccessfull
}

function PathToFinalCapi($CapiProject) {
	$file = get-childitem $CapiProject
	$PathToFinalCapi = $file.directoryname + "\bin\" + $BuildConfiguration + "\WBCapi.apk"
	return $PathToFinalCapi
}

function CopyCapi($Project, $CapiProject) {
	$PahToFinalCapi = PathToFinalCapi($CapiProject)

	$file = get-childitem $Project
	$SourceFolder = $file.directoryname + "\Externals\Capi"

	If (Test-Path "$SourceFolder"){
		Remove-Item "$SourceFolder" -Force -Recurse
	}
	else{
		New-Item -ItemType directory -Path "$SourceFolder"
	}
	New-Item -ItemType directory -Path "$SourceFolder\$BuildNumber"
	Copy-Item "$PahToFinalCapi" "$SourceFolder\$BuildNumber" -Recurse
}

function BuildSupervisor($Solution, $Project, $CapiProject, $BuildConfiguration, $VersionPrefix, $BuildNumber) {

	CleanBinAndObjFolders
	BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }
	RunTests $BuildConfiguration
	UpdateAndroidManifest $VersionPrefix $BuildNumber 
	BuildCapi $CapiProject $BuildConfiguration | %{ if (-not $_) { Exit } }
	SignAndPackCapi $KeystorePassword	$CapiProject | %{ if (-not $_) { Exit } }
	CopyCapi $Project $CapiProject
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}


function BuildDesigner($Solution, $Project, $BuildConfiguration) {

	CleanBinAndObjFolders
	BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }
	RunTests $BuildConfiguration
	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}
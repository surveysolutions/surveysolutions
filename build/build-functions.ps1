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

function UpdateAndroidAppManifest( $VersionPrefix, $BuildNumber , $PahToManifest){
	Write-Host "##teamcity[blockOpened name='Updating Android App Manifest']"
	Write-Host "##teamcity[progressStart 'Updating Android App Manifest']"
	
	[xml] $xam = Get-Content -Path ($PahToManifest)

	$versionName = Select-Xml -xml $xam  -Xpath '/manifest/@android:versionName' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$versionName.Node.Value = $VersionPrefix + [string]$BuildNumber

	$versionCode = Select-Xml -xml $xam  -Xpath '/manifest/@android:versionCode' -namespace @{android='http://schemas.android.com/apk/res/android'}
	$versionCode.Node.Value = [INT]$BuildNumber

	$xam.Save($PahToManifest)

	Write-Host "##teamcity[progressFinish 'Updating Android App Manifest']"
	Write-Host "##teamcity[blockClosed name='Updating Android App Manifest']"
}

function BuildAndroidApp($AndroidProject, $BuildConfiguration){

	Write-Host "##teamcity[blockOpened name='Building '$AndroidProject' project']"
	Write-Host "##teamcity[progressStart 'Building '$AndroidProject' project']"

	& (GetPathToMSBuild) $AndroidProject '/t:PackageForAndroid' '/p:CodeContractsRunCodeAnalysis=false' "/p:Configuration=$BuildConfiguration" | Write-Host

	$wasBuildSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasBuildSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to build '$AndroidProject' project']"
		Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build '$AndroidProject' project']"
	}

	Write-Host "##teamcity[progressFinish 'Building '$AndroidProject' project']"
	Write-Host "##teamcity[blockClosed name='Building Capi'$AndroidProject' project']"

	return $wasBuildSuccessfull
}

function SignAndPackCapi($KeyStorePass, $CapiProject, $TempPackageNamePrefixWithPath, $FinalPackageName){

	Write-Host "##teamcity[blockOpened name='Signing and Zipaligning Android package']"
	Write-Host "##teamcity[progressStart 'Signing and Zipaligning Android package']"

	$PahToSigned = (Join-Path (Get-Location).Path "$TempPackageNamePrefixWithPath-signed.apk")
	$PahToCreated = (Join-Path (Get-Location).Path "$TempPackageNamePrefixWithPath.apk")
	$PahToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/WBCapi.keystore")

	& (GetPathToJarsigner)  '-sigalg' 'MD5withRSA' '-digestalg' 'SHA1' `
		'-keystore' "$PahToKeystore" '-storepass' "$KeyStorePass" `
		'-signedjar' "$PahToSigned" "$PahToCreated" 'wbcapipublish' | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to sign Android package']"
		Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to sign Android package']"

		Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Android package']"
		Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Android package']"

		return $wasOperationSuccessfull
	}

	$PathToFinalCapi = PathToFinalCapi $CapiProject $FinalPackageName
	& (GetPathToZipalign) '-f' '-v' '4' "$PahToSigned" "$PathToFinalCapi" | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to zipalign Android package']"
		Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to zipalign Android package']"
	}

	Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Android package']"
	Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Android package']"

	return $wasOperationSuccessfull
}

function PathToFinalCapi($CapiProject, $FinalPackageName) {
	$file = get-childitem $CapiProject
	$PathToFinalCapi = $file.directoryname + "\bin\" + $BuildConfiguration + "\$FinalPackageName"
	return $PathToFinalCapi
}

function CopyCapi($Project, $CapiProject, $FinalPackageName) {
	$PahToFinalCapi = PathToFinalCapi($CapiProject, $FinalPackageName)

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

	$PahToManifest =  (Join-Path (Get-Location).Path "src\UI\Capi\WB.UI.Capi\Properties\AndroidManifest.xml")
	UpdateAndroidAppManifest $VersionPrefix $BuildNumber $PahToManifest
	BuildAndroidApp $CapiProject $BuildConfiguration | %{ if (-not $_) { Exit } }

	$FinalPackageName = "WBCapi.apk"

	SignAndPackCapi -KeyStorePass $KeystorePassword `
					-CapiProject $CapiProject `
					-TempPackageNamePrefixWithPath "src/UI/Capi/WB.UI.Capi/bin/$BuildConfiguration/CAPI.Android" `
					-FinalPackageName $FinalPackageName | %{ if (-not $_) { Exit } }

	CopyCapi -Project $Project `
			 -CapiProject $CapiProject `
			 -FinalPackageName $FinalPackageName

	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}


function BuildDesigner($Solution, $Project, $CapiTesterProject, $BuildConfiguration, $VersionPrefix, $BuildNumber) {

	CleanBinAndObjFolders
	BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }
	RunTests $BuildConfiguration

	$PahToManifest =  (Join-Path (Get-Location).Path "src\UI\QuestionnaireTester\WB.UI.QuestionnaireTester\Properties\AndroidManifest.xml")
	UpdateAndroidAppManifest $VersionPrefix $BuildNumber $PahToManifest
	BuildAndroidApp $CapiTesterProject $BuildConfiguration | %{ if (-not $_) { Exit } }

	$FinalPackageName = "WBCapiTester.apk" 

	SignAndPackCapi -KeyStorePass $KeystorePassword `
					-CapiProject $CapiTesterProject `
					-TempPackageNamePrefixWithPath "src/UI/QuestionnaireTester/WB.UI.QuestionnaireTester/bin/$BuildConfiguration/CAPI.Android.Tester" `
					-FinalPackageName $FinalPackageName | %{ if (-not $_) { Exit } }

	CopyCapi -Project $Project `
			 -CapiProject $CapiTesterProject `
			 -FinalPackageName $FinalPackageName

	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
	AddArtifacts $Project $BuildConfiguration
}
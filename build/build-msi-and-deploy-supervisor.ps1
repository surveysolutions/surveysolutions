param( [string]$DeployFolder,
[string]$VersionPrefix,
[INT]$BuildNumber,
[string]$KeystorePassword )

$ErrorActionPreference = "Stop"

$TargetDeployFolder = '\\192.168.3.113\Dev\Supervisor'

if(-not [string]::IsNullOrWhiteSpace($DeployFolder))
{$TargetDeployFolder = $DeployFolder} 

#do not allow empty prefix
if([string]::IsNullOrWhiteSpace($VersionPrefix)){
	Write-Host "##teamcity[buildStatus status='FAILURE' text='VersionPrefix param is not set']"
	Exit 
}
#do not allow empty build number	
if(!$BuildNumber){
	Write-Host "##teamcity[buildStatus status='FAILURE' text='BuildNumber param is not set']"
	Exit 
}

#do not allow empty KeystorePassword
if([string]::IsNullOrWhiteSpace($KeystorePassword)){
	Write-Host "##teamcity[buildStatus status='FAILURE' text='VersionPrefix param is not set']"
	Exit 
}

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\deployment-functions.ps1"


function GetPathToJarsigner() {
    return 'C:\Program Files (x86)\Java\jdk1.6.0_31\bin\jarsigner.exe'
}

function GetPathToZipalign() {
    return 'C:\Program Files (x86)\Android\android-sdk\tools\zipalign.exe'
}

function GetPathToMSBuildx86() {
    return 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
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

function SignAndPackCapi($KeyStorePass, $PahToFinalCapi){

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

	& (GetPathToZipalign) '-f' '-v' '4' "$PahToSigned" "$PahToFinalCapi" | Write-Host

	$wasOperationSuccessfull = $LASTEXITCODE -eq 0

	if (-not $wasOperationSuccessfull) {
		Write-Host "##teamcity[message status='ERROR' text='Failed to zipalign Capi package']"
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to zipalign Capi package']"		
    }
	
	Write-Host "##teamcity[progressFinish 'Signing and Zipaligning Capi package']"
    Write-Host "##teamcity[blockClosed name='Signing and Zipaligning Capi package']"
	
	return $wasOperationSuccessfull
}

function BuildMSI($Solution, $BuildConfiguration, $BuildNumber, $VersionPrefix){

	Write-Host "##teamcity[blockOpened name='Create MSI package']"
	Write-Host "##teamcity[progressStart 'Create MSI package']"

	$PublishPath = (Join-Path (Get-Location).Path "Installation\Install\SuperEngine")
	$ClientPath = (Join-Path (Get-Location).Path "Installation\Install\SupervisorHost")

	& (GetPathToMSBuildx86) $Solution "/p:Configuration=$BuildConfiguration" "/p:Version=1.0.$BuildNumber.0" | Write-Host

	$wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
	
		Write-Host "##teamcity[message status='ERROR' text='Failed to create MSI package']"
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to create MSI package']"
    }
    
	Write-Host "##teamcity[progressFinish 'Create MSI package']"
    Write-Host "##teamcity[blockClosed name='Create MSI package']"
	
    return $wasBuildSuccessfull
}

function CopyFilesForInstallation($TargetCapiFileName, $SourceFolder, $BuildNumber){
	Write-Host "##teamcity[blockOpened name='Copy files for installation']"
	Write-Host "##teamcity[progressStart 'Copy files for installation']"
	
	
	If (Test-Path "$SourceFolder\App_Data\Capi\$BuildNumber"){
		Remove-Item "$SourceFolder\App_Data\Capi\$BuildNumber" -Force -Recurse 
	}
	else{
		New-Item -ItemType directory -Path "$SourceFolder\App_Data\Capi\$BuildNumber"
	}	
	Copy-Item "$TargetCapiFileName" "$SourceFolder\App_Data\Capi\$BuildNumber" -Recurse
		
	Write-Host "##teamcity[message text='Copy supervisor to install folder']"
	
	If (Test-Path "Installation\SupervisorInstallProj\WebSiteToHarvest"){
		Remove-Item "Installation\SupervisorInstallProj\WebSiteToHarvest\*" -Force -Recurse 
	}
	Copy-Item "$SourceFolder\*" "Installation\SupervisorInstallProj\WebSiteToHarvest" -Recurse

	##Write-Host "##teamcity[message text='Copy browser to install folder']"
	
	#Could be turned off later
	#Do not forget to remove Shortcut for app from installation
	
	##If (Test-Path "Installation\SupervisorInstallProj\BrowserToHarvest"){
	##	Remove-Item "Installation\SupervisorInstallProj\BrowserToHarvest\*" -Force -Recurse 
	##}	
	##Copy-Item "Awesomium\Supervisor\bin\$BuildConfiguration\*" "Installation\SupervisorInstallProj\BrowserToHarvest" -Recurse
	
	Write-Host "##teamcity[progressFinish 'Copy files for installation']"
    Write-Host "##teamcity[blockClosed name='Copy files for installation']"
		
}

function CreateInstallation($Solution, $Project, $CapiProject, $BuildConfiguration, $SourceFolder, $TargetFolder, $VersionPrefix, $BuildNumber) {

	$PahToFinalCapi = (Join-Path (Get-Location).Path "Mono/CAPI.Android/bin/$BuildConfiguration/WBCapi.apk")
	
    CleanBinAndObjFolders
			
    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

	RunTests $BuildConfiguration

	BuildWebPackage $Project $BuildConfiguration | %{ if (-not $_) { Exit } }
			
	#BuildSolution 'Awesomium\Awesomium.NET Browser.sln' $BuildConfiguration | %{ if (-not $_) { Exit } }
	
	UpdateAndroidManifest $VersionPrefix $BuildNumber 
	
	BuildCapi $CapiProject $BuildConfiguration | %{ if (-not $_) { Exit } }
	
	SignAndPackCapi $KeystorePassword	$PahToFinalCapi | %{ if (-not $_) { Exit } }

	CopyFilesForInstallation $PahToFinalCapi $SourceFolder $BuildNumber 
	
    PublishZippedWebPackage $SourceFolder 'package.zip' | %{ if (-not $_) { Exit } }	
	
	#Build installation
	#BuildMSI "Installation\SupervisorInstallProj\Setup.sln" $BuildConfiguration $BuildNumber $VersionPrefix | %{ if (-not $_) { Exit } }	
}

try {
	$BuildConfiguration = 'Release'	
	$PathToBeDeployed = "RavenQuestionnaire.Web\Web.Supervisor\obj\$BuildConfiguration\Package\PackageTmp"
	
	CreateInstallation `
			-Solution 'src\Supervisor.sln' `
			-Project 'RavenQuestionnaire.Web\Web.Supervisor\Web.Supervisor.csproj' `
			-CapiProject 'Mono\CAPI.Android\CAPI.Android.csproj' `
			-BuildConfiguration $BuildConfiguration `
			-SourceFolder $PathToBeDeployed `
			-TargetFolder $TargetDeployFolder `
			-VersionPrefix $VersionPrefix `
			-BuildNumber $BuildNumber 
			
	DeployFiles $PathToBeDeployed $TargetDeployFolder	
}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Unexpected error occurred']"
    throw
}

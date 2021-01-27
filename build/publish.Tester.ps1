param([string]$VersionName = $null,
[INT]$VersionCode,
[string]$BuildConfiguration='Release',
[string]$KeystorePassword = $null,
[string]$branch = "master",
[string]$GoogleMapKey,
[string]$AppCenterKey,
[string]$KeystoreName = 'WBCapiTester.keystore',
[string]$KeystoreAlias = 'Tester')

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

$AndroidProject = "src\UI\Tester\WB.UI.Tester\WB.UI.Tester.csproj"

$PathToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/$KeyStoreName")

$versionString = (GetVersionString 'src')

Log-Block "Update project version" {
    UpdateProjectVersion $VersionCode -ver $versionString -branch $branch
    Write-Host "##teamcity[setParameter name='system.VersionString' value='$versionString']"
}

Set-AndroidXmlResourceValue $AndroidProject "google_maps_api_key" $GoogleMapKey
Set-AndroidXmlResourceValue $AndroidProject "appcenter_key" $AppCenterKey

& (GetPathToMSBuild) -restore $AndroidProject -t:SignAndroidPackage `
    -p:Configuration=$BuildConfiguration `
    -p:AndroidKeyStore=True `
    -p:VersionCode=$VersionCode `
    -p:GIT_BRANCH=$branch `
    -p:AndroidSigningKeyStore=$PathToKeystore `
    -p:AndroidSigningStorePass=$KeystorePassword `
    -p:AndroidSigningKeyAlias=$KeystoreAlias `
    -p:AndroidSigningKeyPass=$KeystorePassword

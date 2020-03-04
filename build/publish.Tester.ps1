param([string]$VersionName = $null,
[INT]$VersionCode,
[string]$BuildConfiguration='Release',
[string]$KeystorePassword = $null,
[string]$KeystoreName = 'WBCapiTester.keystore',
[string]$KeystoreAlias = 'Tester')

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

$AndroidProject = "src\UI\Tester\WB.UI.Tester\WB.UI.Tester.csproj"

$PathToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/$KeyStoreName")

& (GetPathToMSBuild) -restore $AndroidProject -t:SignAndroidPackage `
    -p:Configuration=$BuildConfiguration `
    -p:AndroidKeyStore=True `
    -p:AndroidSigningKeyStore=$PathToKeystore `
    -p:AndroidSigningStorePass=$KeystorePassword `
    -p:AndroidSigningKeyAlias=$KeystoreAlias `
    -p:AndroidSigningKeyPass=$KeystorePassword

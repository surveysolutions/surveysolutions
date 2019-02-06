param([string]$VersionName,
[INT]$VersionCode,
[string]$BuildConfiguration='release',
[string]$KeystorePassword = $null,
[string]$KeystoreName,
[string]$KeystoreAlias,
[string]$CapiProject,
[string]$OutFileName,
[bool]$ExcludeExtra,
[string]$branch,
[string]$PlatformsOverride,
[Switch]$NoCleanUp)


$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"


if(!$VersionCode){
    Log-Error "VersionCode param is not set"
    Exit
}

#do not allow empty KeystorePassword
# if([string]::IsNullOrWhiteSpace($KeystorePassword)){
#     Log-Error "KeystorePassword param is not set"
#     Exit
# }

function BuildAndroidApp($AndroidProject, $BuildConfiguration, $ExcludeExtensions, $TargetAbi, $VersionCode, $OutFileName) {
    return Log-Block "Building Android project: $AndroidProject => $([System.IO.Path]::GetFileName($OutFileName))" {
        return Execute-MSBuild $AndroidProject $BuildConfiguration @(
            "/p:VersionCode=$VersionCode"
            "/p:ApkOutputPath=$([System.IO.Path]::GetFullPath($OutFileName))"

            if(-not $NoCleanUp.IsPresent) {
                '/t:Clean;SignAndroidPackage;MoveApkFile'
            } else {
                '/t:SignAndroidPackage;MoveApkFile'
            }
            
            if($ExcludeExtensions -eq $True) {
                "/p:ExcludeExtensions=$ExcludeExtensions;DefineConstants=EXCLUDEEXTENSIONS"
            }

            if([string]::IsNullOrWhiteSpace($KeystorePassword) -eq $False) {
                Log-Message "Signing with $KeyStoreName"
                $PathToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/$KeyStoreName")
                '/p:AndroidUseApkSigner=true'
                '/p:AndroidKeyStore=True'
                "/p:AndroidSigningKeyAlias=$KeystoreAlias"
                "/p:AndroidSigningKeyPass=$KeystorePassword"
                "/p:AndroidSigningKeyStore=$PathToKeystore"
                "/p:AndroidSigningStorePass=$KeystorePassword"
            }

            if($null -eq $env:GIT_BRANCH) {
                "/p:GIT_BRANCH=$branch"
            }

            if([string]::IsNullOrWhiteSpace($TargetAbi) -eq $False)
            {
                "/p:AndroidSupportedAbis=$TargetAbi"
            }
        )
    }
}

# Main part
$ErrorActionPreference = "Stop"

Log-Block "Building Android Package: $(GetPackageName $CapiProject)" {
    Log-Message "PlatformsOverride = $PlatformsOverride"

    if([string]::IsNullOrWhiteSpace($PlatformsOverride))
    {
        if (Test-Path $OutFileName) {
            Remove-Item $OutFileName -Force
        }

        BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra -VersionCode "$VersionCode" -OutFileName $OutFileName | %{ if (-not $_) { Exit } }
    }
    else
    {
        $TargetAbis =  ($PlatformsOverride -split ';').Trim()
        $IndexToAdd = 0 
        Foreach ($TargetAbi in $TargetAbis)
        {
            $IndexToAdd = $IndexToAdd + 1

            if (Test-Path "$TargetAbi$OutFileName") {
                Remove-Item "$TargetAbi$OutFileName" -Force
            }
#to build several abis and upload to store version per apk should be different
            BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra $TargetAbi -VersionCode "$VersionCode$IndexToAdd" -OutFileName "$TargetAbi$OutFileName" | %{ if (-not $_) { Exit } }    
        }
    }
}

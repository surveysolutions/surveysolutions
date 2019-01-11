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

function BuildAndroidApp($AndroidProject, $BuildConfiguration, $ExcludeExtensions, $TargetAbi, $OutFileName) {
    Log-Block "Building Android project: $AndroidProject => $([System.IO.Path]::GetFileName($OutFileName))" {
        
        $buildArgs = @(
            $AndroidProject, "/p:Configuration=$BuildConfiguration", 
            '/v:q', '/m:8', '/nologo','/p:CodeContractsRunCodeAnalysis=false', 
            # '/clp:ForceConsoleColor;ErrorsOnly', 
            "/p:VersionCode=$VersionCode"
        )

        if($env:TEAMCITY_VERSION -eq $null) {
            $buildArgs += '/clp:ForceConsoleColor;ErrorsOnly'
        }

        # $buildArgs += '/bl /clp:ForceConsoleColor;PerformanceSummary;NoSummary;ErrorsOnly' # to show perf summary
        $binLogPath = "$([System.IO.Path]::GetFileName($OutFileName)).msbuild.binlog"
        $buildArgs += "/bl:$binLogPath"
                
        if(-not $NoCleanUp.IsPresent) {
            $buildArgs += '/t:Clean'
        }
        
        $buildArgs += '/t:SignAndroidPackage;MoveApkFile'
        
        #$buildArgs += '/p:DeployOnBuild=True'
        
        # if key store password not provided msbuild will sign with default dev keys
        if([string]::IsNullOrWhiteSpace($KeystorePassword) -eq $False) {
            Log-Message "Signing with $KeyStoreName"
            $PathToKeystore = (Join-Path (Get-Location).Path "Security/KeyStore/$KeyStoreName")
            $buildArgs += '/p:AndroidUseApkSigner=true'
            $buildArgs += '/p:AndroidKeyStore=True'
            $buildArgs += "/p:AndroidSigningKeyAlias=$KeystoreAlias"
            $buildArgs += "/p:AndroidSigningKeyPass=$KeystorePassword"
            $buildArgs += "/p:AndroidSigningKeyStore=$PathToKeystore"
            $buildArgs += "/p:AndroidSigningStorePass=$KeystorePassword"
        }

        if($ExcludeExtensions -eq $True) {
            $buildArgs += "/p:ExcludeExtensions=$ExcludeExtensions"
            $buildArgs += "/p:DefineConstants=EXCLUDEEXTENSIONS"
        }
        if($null -eq $env:GIT_BRANCH) {
            $buildArgs += "/p:GIT_BRANCH=$branch"
        }

        if([string]::IsNullOrWhiteSpace($TargetAbi) -eq $False)
        {
            $buildArgs += "/p:AndroidSupportedAbis=$TargetAbi"
        }

        $buildArgs += "/p:ApkOutputPath=$([System.IO.Path]::GetFullPath($OutFileName))"

        # Executing MSBUILD
        & (GetPathToMSBuild) $buildArgs | Out-Host
        
        $wasBuildSuccessfull = $LASTEXITCODE -eq 0

        if (-not $wasBuildSuccessfull) {
            Log-Error "Failed to build |'$AndroidProject' | project"
            
            Start-Sleep -Seconds 1 # binlog is still writing at this moment
            Publish-Artifact "$binLogPath"
            throw "Failed to build |'$AndroidProject' | project"
        }

        return $wasBuildSuccessfull
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

        BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra -OutFileName $OutFileName | %{ if (-not $_) { Exit } }
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

            BuildAndroidApp $CapiProject $BuildConfiguration $ExcludeExtra $TargetAbi -OutFileName "$TargetAbi$OutFileName" | %{ if (-not $_) { Exit } }    
        }
    }
}

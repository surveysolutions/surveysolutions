$vswhere = [io.path]::combine(${env:ProgramFiles(x86)}, "Microsoft Visual Studio", "Installer", "vswhere.exe")

$verbosity = "q"

if((Test-Path variable:$env:MSBUILD_VERBOSITY) -eq $False){
    $verbosity = $env:MSBUILD_VERBOSITY
}

function GetPathRelativeToCurrectLocation($FullPath) {
    return $FullPath.Substring((Get-Location).Path.Length + 1)
}

##############################
#.SYNOPSIS
# Get msbuild with the use of `vswhere` tool, that should be installed in %PATH% location.
#
#.DESCRIPTION
#VsWhere tool can be found here https://github.com/Microsoft/vswhere
#This code is based on example from https://github.com/Microsoft/vswhere/wiki/Find-MSBuild wiki page
#
#Will return latest installed VS with MSBuild and Xamarin
#https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids
##############################
function GetMsBuildFromVsWhere() {
    $path = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -requires Component.Xamarin -property installationPath
    if ($path) {
        $path = join-path $path 'MSBuild\15.0\Bin\MSBuild.exe'
        if (test-path $path) {
            return $path
        }
    }
}

##############################
#.SYNOPSIS
#Return correct path to installed MSBUILD in system
#
#.DESCRIPTION
#Return correct path to installed MSBUILD in system. Will try to use `vswhere` if its installed in %PATH%
#
##############################
function GetPathToMSBuild() {
    if (Test-Path $vswhere) { 
        return GetMsBuildFromVsWhere
    }

    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe") {
        return "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
    }
	
    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe") {
        return "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
    }
     
    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe") {
        return "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
    }

    return 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
}

function GetPathToConfigTransformator() {
    return "packages\WebConfigTransformRunner.1.0.0.1\Tools\WebConfigTransformRunner"
}

function GetMainSolutionPath() {
    return "src\WB.sln"
}

function CleanFolders($Filter) {
    $progressMessage = "Cleaning $Filter folders"
    Write-Host "##teamcity[blockOpened name='$Filter']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    $folders = Get-ChildItem -Filter $Filter -Recurse -ErrorAction SilentlyContinue `
        | ? { $_.Attributes -match 'Directory' } `
        | ? { $_.FullName -notmatch '\\.hg\\' } `
        | ? { $_.FullName -notmatch '\\node_modules\\' } `
        | % { GetPathRelativeToCurrectLocation $_.FullName }

    if ($folders -ne $null) {
        foreach ($folder in $folders) {
            Write-Host $folder
            Remove-Item $folder -Force -Recurse -ErrorAction SilentlyContinue
        }
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$Filter']"
}

function Publish-Artifact($artifactSpec) {
    Write-Host "##teamcity[publishArtifacts '$artifactSpec']"
}

function Log-Block($logBlockName, [ScriptBlock] $logBlock) {
    Log-StartBlock $logBlockName
    Log-StartProgress $logBlockName

    try {
        $result = & $logBlock
        return $result
    }
    finally {
        Log-EndProgress $logBlockName
        Log-EndBlock $logBlockName
    }
}

function Log-StartBlock($blockStartName) {
    "##teamcity[blockOpened name='$(TeamCityEncode $blockStartName)']" | Write-Host
}

function Log-EndBlock($blockEndName) {
    "##teamcity[blockClosed name='$(TeamCityEncode $blockEndName)']" | Write-Host
}

function Log-Message($message) {
    "##teamcity[message text='$(TeamCityEncode $message)']" | Write-Host
}

function Log-Error($errorMessage, $details = $null) {
    if($details -eq $null) {
        $details = $errorMessage
    }
    $details = TeamCityEncode $details
    $errMsg = TeamCityEncode $errorMessage
    "##teamcity[message status='ERROR' text='$errMsg' errorDetails='$details' ]" | Write-Host
    "##teamcity[buildProblem description='$errMsg']" | Write-Host
}

function Log-Progress($progressMessage, [ScriptBlock] $progressBlock) {
    Log-StartProgress $progressMessage

    try {
        $result = & $progressBlock
        return $result
    }
    finally {
        Log-EndProgress $progressMessage
    }
}

function Log-StartProgress($startProgressMessage) {
    "##teamcity[progressStart name='$(TeamCityEncode $startProgressMessage)']" | Write-Host
}

function Log-EndProgress($endProgressMessage) {
    "##teamcity[progressFinish name='$(TeamCityEncode $endProgressMessage)']" | Write-Host
}

function CleanBinAndObjFolders() {
    Log-Block "Cleaning folders" {
        CleanFolders 'bin'
        CleanFolders 'obj'
        CleanFolders 'src\UI\Designer\WB.UI.Designer\questionnaire\build'
        CleanFolders 'src\UI\Headquarters\WB.UI.Headquarters\InterviewApp'
        CleanFolders 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.Interview\node_modules'
    }
}

function versionCheck() {
    Write-Host "Node version: $(&node -v)"
    Write-Host "NPM version: $(&npm --version)"
    Write-Host "Yarn version: $(&yarn --version)"
    Write-host "MsBuild version: $(& (GetPathToMSBuild) /version /nologo)"
    Write-Host "Dotnet CLI version: $(dotnet --version)"
}

##############################
#.SYNOPSIS
##Build static application
#
#.DESCRIPTION
#Will build application with a combination of two commands call
#
#
#.PARAMETER blockName
#Name of application to build
#
#.PARAMETER targetLocation
#Location of application
#
#.EXAMPLE
#BuildStaticContent "Web Interview" "src/ui/hq/webinterview"
#
#.NOTES
#`yarn` and `yarn production`
#
#this require for all static applications to have script with name `production`, so that production build can be made
#
# to execute pre build step - use script with name `preproduction`
##############################
function BuildStaticContent($blockName, $targetLocation, $runTests = $false) {
    Push-Location $targetLocation
    try {
        Log-Block $blockName {
            Log-Message "yarn install"
            #install node js dependencies
            &yarn install --no-optional | Write-Host
            
            $wasBuildSuccessfull = $LASTEXITCODE -eq 0
            if (-not $wasBuildSuccessfull) {
                Log-Error "Failed to run yarn"
                return $wasBuildSuccessfull
            }
    
            Log-Message "Running gulp --production"
            &node_modules\.bin\gulp --production | Write-Host
    
            $wasBuildSuccessfull = $LASTEXITCODE -eq 0
            if (-not $wasBuildSuccessfull) {
                Log-Error "Failed to run gulp --production"
                return $wasBuildSuccessfull
            }
    
            return $true
        }
    } finally {
        Pop-Location
    }
}

function CheckPrerequisites() {
    Write-Host "##teamcity[blockOpened name='Checking prerequisities']"
    Write-Host "##teamcity[progressStart 'Checking prerequisities']"

    $havePrerequisitesSucceeded = $true
    #$havePrerequisitesSucceeded = CheckCompilationDebugFlagInWebConfigs

    Write-Host "##teamcity[progressFinish 'Checking prerequisities']"
    Write-Host "##teamcity[blockClosed name='Checking prerequisities']"

    return $havePrerequisitesSucceeded
}


function IsSetupSolution($Solution) {
    return ($Solution.EndsWith('Setup.sln') -or $Solution.EndsWith('SetupHeadquarters.sln'))
}

function ShouldSolutionBeIgnored($Solution) {
    return IsSetupSolution $Solution
}

function TeamCityEncode([string]$value) {
    $result = $value.Replace("|", "||")
    $result = $result.Replace("'", "|'") 
    $result = $result.Replace("[", "|[")
    $result = $result.Replace("]", "|]") 
    $result = $result.Replace("\r", "|r") 
    $result = $result.Replace("\n", "|n")
    $result
}

function BuildSolution($Solution, $BuildConfiguration) {
    return Log-Block "Building solution $Solution in configuration '$BuildConfiguration'" {
        return Execute-MSBuild $Solution $BuildConfiguration
    }
}

function RunConfigTransform($Project, $BuildConfiguration) {
    $file = get-childitem $Project
    $PathToConfigFile = Join-Path $file.directoryname "Web.config"
    $PathToTransformFile = Join-Path $file.directoryname "Web.$BuildConfiguration.config"

    $command = "$(GetPathToConfigTransformator) $PathToConfigFile $PathToTransformFile $PathToConfigFile"
    Write-Host $command
    iex $command
}

function AddArtifacts($Project, $BuildConfiguration, $folder) {
    $file = get-childitem $project
    $packagepath = $file.directoryname + "\obj\" + $BuildConfiguration + "\package\"

    $filename = $packagepath + $file.basename
    $zipfile = $filename + ".zip"
    $cmdfile = $filename + ".deploy.cmd"

    MoveArtifacts $zipfile, $cmdfile $folder
}

function MoveArtifacts([string[]] $items, $folder) {
    $artifactsFolder = "Artifacts"
    If (Test-Path "$artifactsFolder") {
        If (Test-Path "$artifactsFolder\$folder") {
            Remove-Item "$artifactsFolder\$folder" -Force -Recurse
        }
    }
    else {
        New-Item -ItemType directory -Path "$artifactsFolder"
    }
    New-Item -ItemType directory -Path "$artifactsFolder\$folder"

    foreach ($file in $items) {
        Copy-Item "$file" "$artifactsFolder\$folder"
    }
}

function Execute-MSBuild($ProjectFile, $Configuration, $buildArgs = $null) {
    $build = @(
        $ProjectFile
        "/p:Configuration=$Configuration", '/nologo', "/v:$verbosity", '/m:4'
        '/p:DebugSymbols=false;CodeContractsRunCodeAnalysis=false;ExcludeGeneratedDebugSymbol=true',
        '/flp2:errorsonly;logfile=msbuild.err'
    )

    if(Test-Path msbuild.err  -PathType Leaf) {
        Remove-Item msbuild.err
    }

    if($env:TEAMCITY_VERSION -eq $null) {
        $build += '/clp:ForceConsoleColor'
    }

    if($buildArgs -ne $null) {
        $build = ($build + $buildArgs | select -uniq)
    }

    $binLogPath = "$([System.IO.Path]::GetFileName($ProjectFile)).msbuild.binlog"

    $build += "/bl:$binLogPath"
    
    & (GetPathToMSBuild) $build | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        $errors = $null
        
        if(Test-Path msbuild.err -PathType Leaf){
            $errors = Get-Content msbuild.err
        } 

        Log-Error "Failed to build '$ProjectFile'. See $binLogPath in artifacts for details" errors

        if(Test-Path $binLogPath -PathType Leaf) {
            Start-Sleep -Seconds 1; Publish-Artifact "$binLogPath"
        }

        throw "$errors"
    } else {
        if($env:MSBUILD_BINLOG -eq $True) {
            Start-Sleep -Seconds 3; Publish-Artifact "$binLogPath"
        }
    }

    return $wasBuildSuccessfull
}

function BuildWebPackage($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building web package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building web package for project $Project']"

    return Log-Block "Building web package for project $Project" {
        return Execute-MSBuild $Project $BuildConfiguration '/t:Package'
    }
}

function UpdateSourceVersion($Version, $BuildNumber, [string]$file, [string] $branch) {
	if($branch.ToLower() -eq 'release') {
		$branch = ""
	} else {
		$branch = " $branch"
	}

	if ($Version -match "^[0-9]+\.[0-9]+$") {
		$ver = $Version + ".0." + $BuildNumber
	}
	else {
		if ($Version -match "^[0-9]+\.[0-9]+\.[0-9]+$") {
			$ver = $Version + "." + $BuildNumber
		}
		else {
			Throw "Version string $Version must be of form YYY.MM or YYYY.MM.#"
		}
	}

    $NewVersion = 'AssemblyVersion("' + $ver + '")';
    $NewFileVersion = 'AssemblyFileVersion("' + $ver + '")';
    $NewInformationalVerson = "AssemblyInformationalVersion(""$Version (build $BuildNumber)$branch"")"

    $TmpFile = $tempFile = [System.IO.Path]::GetTempFileName()

    get-content $file | 
        % {$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        % {$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion } |
        % {$_ -replace 'AssemblyInformationalVersion\("[0-9]+(\.([0-9]+|\*)){1,2} \(build [0-9]+\).*"\)', $NewInformationalVerson } > $TmpFile

    move-item $TmpFile $file -force
    Write-Host "##teamcity[message text='Updated $file to version $ver']"
}

function GetVersionString([string]$Project) {
    $ret = Get-Content $Project/.version
    $ver = [Version]$ret # will fail if invalid string
    if ($ver.Build -eq 0) { # we don't want trailing zero in the third position
        $ret = "{0}.{1}" -f $ver.Major, $ver.Minor
    }
    return $ret
}

function UpdateProjectVersion([string]$BuildNumber, [string]$ver, [string] $branch) {
    $foundFiles = get-childitem -include *AssemblyInfo.cs -recurse -ErrorAction SilentlyContinue | `
        ? { $_.fullname -notmatch "\\*.Tests\\?" }
    foreach ($file in $foundFiles) {
        UpdateSourceVersion -Version $ver -BuildNumber $BuildNumber -file $file -Branch $branch
    }
}

function CreateZip($sourceFolder, $zipfile) {
    If (Test-path $zipfile) {Remove-item $zipfile}

    Add-Type -assembly "system.io.compression.filesystem"
    [io.compression.zipfile]::CreateFromDirectory($sourceFolder, $zipfile)
}

function BuildAndDeploySupportTool($SupportToolSolution, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building and deploying support console application']"
    BuildSolution `
        -Solution $SupportToolSolution `
        -BuildConfiguration $BuildConfiguration

    $file = get-childitem $SupportToolSolution

    $binDir = $file.directoryname + "\bin\"
    $sourceDir = $binDir + $BuildConfiguration
    $destZipFile = $binDir + "support.zip"

    CreateZip $sourceDir $destZipFile
    MoveArtifacts $destZipFile "Tools"

    Write-Host "##teamcity[blockClosed name='Building and deploying support console application']"
}
function GetPathToManifest([string]$CapiProject) {
    $file = get-childitem $CapiProject
    return ($file.directoryname + "\Properties\AndroidManifest.xml")
}

function GetPackageName([string]$CapiProject) {
    if((Test-Path (GetPathToManifest $CapiProject)) -eq $False) { return "no" }

    [xml] $xam = Get-Content -Path (GetPathToManifest $CapiProject)

    $res = Select-Xml -xml $xam -Xpath '/manifest/@package' -namespace @{android='http://schemas.android.com/apk/res/android'}
    return ($res.Node.Value)
}

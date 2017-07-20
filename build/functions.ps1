function GetPathRelativeToCurrectLocation($FullPath) {
    return $FullPath.Substring((Get-Location).Path.Length + 1)
}

function GetPathToMSBuild() {
    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"){
        return "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
    }
    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"){
        return "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
    }

    return 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
}

function GetPathToConfigTransformator() {
    return "packages\WebConfigTransformRunner.1.0.0.1\Tools\WebConfigTransformRunner"
}

function CleanFolders($Filter) {
    $progressMessage = "Cleaning $Filter folders"
    Write-Host "##teamcity[blockOpened name='$Filter']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    $folders = Get-ChildItem -Filter $Filter -Recurse -ErrorAction SilentlyContinue | ?{ $_.Attributes -match 'Directory' } | ?{ $_.FullName -notmatch '\\.hg\\' } | ?{ $_.FullName -notmatch '\\node_modules\\' } ` | %{ GetPathRelativeToCurrectLocation $_.FullName }

    if ($folders -ne $null) {
        foreach ($folder in $folders) {
            Write-Host $folder
            Remove-Item $folder -Force -Recurse -ErrorAction SilentlyContinue
        }
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$Filter']"
}

function CleanBinAndObjFolders() {
    Write-Host "##teamcity[blockOpened name='Cleaning folders']"

    CleanFolders 'bin'
    CleanFolders 'obj'
    CleanFolders 'src\UI\Designer\WB.UI.Designer\questionnaire\build'
    CleanFolders 'src\UI\Headquarters\WB.UI.Headquarters\InterviewApp'
    CleanFolders 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.Interview\node_modules'

    Write-Host "##teamcity[blockClosed name='Cleaning folders']"
}


function BuildWebInterviewApp($targetLocation){
    $action = 'Building WebInterview app'
    
    Write-Host "##teamcity[blockOpened name='$action']"
    Write-Host "##teamcity[progressStart '$action']"

    Write-Host "Pushing location to $targetLocation"
    Push-Location -Path $targetLocation

    try{
        
        &yarn | Write-Host
        $wasBuildSuccessfull = $LASTEXITCODE -eq 0
        if (-not $wasBuildSuccessfull) {
            Write-Host "##teamcity[message status='ERROR' text='Failed to run yarn']"
            return $wasBuildSuccessfull
        }
        
        &npm run build | Write-Host

        $wasBuildSuccessfull = $LASTEXITCODE -eq 0
        if (-not $wasBuildSuccessfull) {
            Write-Host "##teamcity[message status='ERROR' text='Failed to execute npm run build']"
            return $wasBuildSuccessfull
        }
    } finally {
        Pop-Location

        Write-Host "##teamcity[progressFinish '$action']"
        Write-Host "##teamcity[blockClosed name='$action']"
    }
	return $wasBuildSuccessfull
}

function BuildStaticContent($targetLocation, $forceInstall){
    Write-Host "##teamcity[blockOpened name='Building static files']"
    Write-Host "##teamcity[progressStart 'Building static files']"

    Write-Host "Pushing location to $targetLocation"
    Push-Location -Path $targetLocation
    Write-Host "Running npm install"

	#install node js dependencies
    &yarn install | Write-Host
	$wasBuildSuccessfull = $LASTEXITCODE -eq 0
	 if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to run npm install']"
		return $wasBuildSuccessfull
    }
	
	#install bower packages
	if ($forceInstall)
	{
		&bower install --force | Write-Host
	}else{
		&bower install | Write-Host
	}
	$wasBuildSuccessfull = $LASTEXITCODE -eq 0
	 if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to run bower install']"
		return $wasBuildSuccessfull
    }
	
	#will execute script gulpfile.js in target folder
    &gulp --production | Write-Host 
	
	$wasBuildSuccessfull = $LASTEXITCODE -eq 0
    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to run gulp --production']"
		return $wasBuildSuccessfull
    }	
	
    Pop-Location

    Write-Host "##teamcity[progressFinish 'Building static files']"
    Write-Host "##teamcity[blockClosed name='Building static files']"
	
	return $wasBuildSuccessfull
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

function GetSolutionsToBuild() {
    $foundSolutions = Get-ChildItem -Filter *.sln -Recurse -ErrorAction SilentlyContinue | ?{ $_.FullName -notmatch 'Dependencies' }  | %{ GetPathRelativeToCurrectLocation $_.FullName } 
    $solutionsToIgnore = $foundSolutions | ?{ ShouldSolutionBeIgnored $_ }
    $solutionsToBuild = $foundSolutions | ?{ -not (ShouldSolutionBeIgnored $_) }

    if ($solutionsToIgnore.Count -gt 0) {
        Write-Host "##teamcity[message status='WARNING' text='Ignored $($solutionsToIgnore.Count) solution(s): $([string]::Join(', ', $solutionsToIgnore))']"
    }

    return $solutionsToBuild
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

function BuildSolution($Solution, $BuildConfiguration, [switch] $MultipleSolutions, $IndexOfSolution = 0, $CountOfSolutions = 1) {
    $progressMessage = if ($MultipleSolutions) { "Building solution $($IndexOfSolution + 1) of $CountOfSolutions $Solution in configuration '$BuildConfiguration'" } else { "Building solution $Solution in configuration '$BuildConfiguration'" }
    $blockMessage = if ($MultipleSolutions) { $Solution } else { "Building solution $Solution in configuration '$BuildConfiguration'" }

    Write-Host "##teamcity[blockOpened name='$(TeamCityEncode $blockMessage)']"
    Write-Host "##teamcity[progressStart '$(TeamCityEncode $progressMessage)']"

    & (GetPathToMSBuild) $Solution /t:Build /nologo /v:m /p:CodeContractsRunCodeAnalysis=false /p:Configuration=$BuildConfiguration | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build solution $Solution']"

        if (-not $MultipleSolutions) {
            Write-Host "##teamcity[buildProblem description='Failed to build solution $Solution']"
        }
    }

    Write-Host "##teamcity[progressFinish '$(TeamCityEncode $progressMessage)']"
    Write-Host "##teamcity[blockClosed name='$(TeamCityEncode $blockMessage)']"

    return $wasBuildSuccessfull
}

function BuildSolutions($BuildConfiguration,  [switch] $ClearBinAndObjFoldersBeforeEachSolution) {
    Write-Host "##teamcity[blockOpened name='Building solutions']"

    $solutionsToBuild = GetSolutionsToBuild

    $failedSolutions = @()

    if ($solutionsToBuild -ne $null) {
        foreach ($solution in $solutionsToBuild) {

            if ($ClearBinAndObjFoldersBeforeEachSolution) {
                CleanBinAndObjFolders
            }

            $wasBuildSuccessfull = BuildSolution `
                -Solution $solution `
                -BuildConfiguration $BuildConfiguration `
                -MultipleSolutions `
                -IndexOfSolution ([array]::IndexOf($solutionsToBuild, $solution)) `
                -CountOfSolutions $solutionsToBuild.Count `

            if (-not $wasBuildSuccessfull) {
                $failedSolutions += $solution
            }
        }
    }

    $wereAllSolutionsBuiltSuccessfully = $failedSolutions.Count -eq 0
    if (-not $wereAllSolutionsBuiltSuccessfully) {
        Write-Host "##teamcity[buildProblem description='Failed to build $($failedSolutions.Count) solution(s): $($failedSolutions -join ', ')']"
    }

    Write-Host "##teamcity[blockClosed name='Building solutions']"

    return $wereAllSolutionsBuiltSuccessfully
}


function GetProjectsWithTests() {
    return Get-ChildItem -Filter '*Tests*.csproj' -Recurse -ErrorAction SilentlyContinue | %{ GetPathRelativeToCurrectLocation $_.FullName }
}

function GetOutputAssembly($Project, $BuildConfiguration) {
    $projectFileInfo = Get-Item $Project
    $projectXml = [xml] (Get-Content $Project)

    $projectFolder = $projectFileInfo.DirectoryName

    $outputPath = $projectXml.Project.PropertyGroup `
        | ?{ $_.Condition -like "*'$BuildConfiguration|*" } `
        | %{ $_.OutputPath } `
        | select -First 1

    $assemblyName = $projectXml.Project.PropertyGroup.AssemblyName[0]

    $fullPathToAssembly = Join-Path (Join-Path $projectFolder $outputPath) "$assemblyName.dll"

    return GetPathRelativeToCurrectLocation $fullPathToAssembly
}

function RunConfigTransform($Project, $BuildConfiguration){
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

	MoveArtifacts $zipfile,$cmdfile $folder
}

function MoveArtifacts([string[]] $items, $folder) {
    $artifactsFolder = "Artifacts"
	If (Test-Path "$artifactsFolder"){
		If (Test-Path "$artifactsFolder\$folder"){
			Remove-Item "$artifactsFolder\$folder" -Force -Recurse
		}
	}
	else{
		New-Item -ItemType directory -Path "$artifactsFolder"
	}
	New-Item -ItemType directory -Path "$artifactsFolder\$folder"

    foreach($file in $items) {
	    Copy-Item "$file" "$artifactsFolder\$folder"
	}
}

function BuildWebPackage($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building web package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building web package for project $Project']"

    & (GetPathToMSBuild) $Project '/t:Package' /p:CodeContractsRunCodeAnalysis=false "/p:Configuration=$BuildConfiguration" '/verbosity:quiet' /nologo | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build web package for project $Project']"
        Write-Host "##teamcity[buildProblem description='Failed to build web package for project $Project']"
    }

    Write-Host "##teamcity[progressFinish 'Building web package for project $Project']"
    Write-Host "##teamcity[blockClosed name='Building web package for project $Project']"

    return $wasBuildSuccessfull
}

function CopyCapi($Project, $source, $cleanUp) {
	$file = get-childitem $Project
	$DestinationFolder = $file.directoryname + "\Externals"
	
	Write-Host "##teamcity[message text='Prepare to copy apk with option cleanUp = $cleanUp']"

	if($cleanUp)
	{
	  if (Test-Path "$DestinationFolder"){
		  Write-Host "##teamcity[message text='Clean up target folder $DestinationFolder']"
		  
		  Remove-Item "$DestinationFolder" -Force -Recurse
	  }
	  New-Item -ItemType directory -Path "$DestinationFolder"
	}
	
	Write-Host "##teamcity[message text='Copy apk with option clean from $source']"
	
	Copy-Item "$source" "$DestinationFolder" -Recurse
}

function UpdateSourceVersion($Version, $BuildNumber, [string]$file) {

	$ver = $Version + "." + $BuildNumber
	$NewVersion = 'AssemblyVersion("' + $ver + '")';
	$NewFileVersion = 'AssemblyFileVersion("' + $ver + '")';
	$NewInformationalVerson = 'AssemblyInformationalVersion("' + $Version + ' (build ' + $BuildNumber + ')")'

	$TmpFile = $tempFile = [System.IO.Path]::GetTempFileName()

	get-content $file | 
		%{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
		%{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion } |
		%{$_ -replace 'AssemblyInformationalVersion\("[0-9]+(\.([0-9]+|\*)){1,2} \(build [0-9]+\)"\)', $NewInformationalVerson } > $TmpFile

	move-item $TmpFile $file -force
	Write-Host "##teamcity[message text='Updated $file to version $ver']"
}

function GetVersionString([string]$Project)
{
	$file = get-childitem $Project
	$file = Join-Path $file.directoryname ".version"
	$ret = Get-Content $file
	return $ret
}

function UpdateProjectVersion([string]$BuildNumber, [string]$ver)
{
	$foundFiles = get-childitem -include *AssemblyInfo.cs -recurse -ErrorAction SilentlyContinue | `
		?{ $_.fullname -notmatch "\\*.Tests\\?" }
	foreach ($file in $foundFiles) {
		UpdateSourceVersion -Version $ver -BuildNumber $BuildNumber -file $file
	}
}

function CreateZip($sourceFolder, $zipfile)
{
	If(Test-path $zipfile) {Remove-item $zipfile}
	
	Add-Type -assembly "system.io.compression.filesystem"	
	[io.compression.zipfile]::CreateFromDirectory($sourceFolder, $zipfile)
}

function BuildAndDeploySupportTool($SupportToolSolution, $BuildConfiguration)
{
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

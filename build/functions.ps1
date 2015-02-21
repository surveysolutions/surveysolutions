function GetPathRelativeToCurrectLocation($FullPath) {
    return $FullPath.Substring((Get-Location).Path.Length + 1)
}

function GetPathToMSBuild() {
    return 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
}

function GetPathToConfigTransformator() {
    return "packages\WebConfigTransformRunner.1.0.0.1\Tools\WebConfigTransformRunner"
}

function CleanFolders($Filter) {
    $progressMessage = "Cleaning $Filter folders"
    Write-Host "##teamcity[blockOpened name='$Filter']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    $folders = Get-ChildItem -Filter $Filter -Recurse -ErrorAction SilentlyContinue | ?{ $_.Attributes -match 'Directory' } | ?{ $_.FullName -notmatch '\\.hg\\' } | %{ GetPathRelativeToCurrectLocation $_.FullName }

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
    CleanFolders 'src\UI\Designer\WB.UI.Designer\UpdatedDesigner\build'

    Write-Host "##teamcity[blockClosed name='Cleaning folders']"
}
function BuildNewDesigner(){
    $installCommand = "npm install"
    $targetLocation = "src\UI\Designer\WB.UI.Designer\UpdatedDesigner"
    Write-Host "Pushing location to $targetLocation"
    Push-Location -Path $targetLocation
    Write-Host $installCommand
    iex $installCommand #install node js dependencies
    &gulp #will execute script gulpfile.js in UpdatedDesigner folder
    Pop-Location
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
    $foundSolutions = Get-ChildItem -Filter *.sln -Recurse -ErrorAction SilentlyContinue | %{ GetPathRelativeToCurrectLocation $_.FullName }
    $solutionsToIgnore = $foundSolutions | ?{ ShouldSolutionBeIgnored $_ }
    $solutionsToBuild = $foundSolutions | ?{ -not (ShouldSolutionBeIgnored $_) }

    if ($solutionsToIgnore.Count -gt 0) {
        Write-Host "##teamcity[message status='WARNING' text='Ignored $($solutionsToIgnore.Count) solution(s): $([string]::Join(', ', $solutionsToIgnore))']"
    }

    return $solutionsToBuild
}

function BuildSolution($Solution, $BuildConfiguration, [switch] $MultipleSolutions, $IndexOfSolution = 0, $CountOfSolutions = 1) {
    $progressMessage = if ($MultipleSolutions) { "Building solution $($IndexOfSolution + 1) of $CountOfSolutions $Solution in configuration '$BuildConfiguration'" } else { "Building solution $Solution in configuration '$BuildConfiguration'" }
    $blockMessage = if ($MultipleSolutions) { $Solution } else { "Building solution $Solution in configuration '$BuildConfiguration'" }

    Write-Host "##teamcity[blockOpened name='$blockMessage']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    & (GetPathToMSBuild) $Solution '/t:Build' '/p:CodeContractsRunCodeAnalysis=false' "/p:Configuration=$BuildConfiguration" | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build solution $Solution']"

        if (-not $MultipleSolutions) {
            Write-Host "##teamcity[buildProblem description='Failed to build solution $Solution']"
        }
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$blockMessage']"

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

function RunTestsFromProject($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='$Project']"

    $assembly = GetOutputAssembly $Project $BuildConfiguration

    if (-not (Test-Path $assembly)) {

        Write-Host "##teamcity[message status='WARNING' text='Expected tests assembly $assembly is missing']"

    } else {

        Write-Host "##teamcity[progressStart 'Running tests from $assembly']"

        $resultXml = (Get-Item $assembly).BaseName + '.NUnit-Result.xml'
        .\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe $assembly /result=$resultXml /nologo /nodots | Write-Host
        Write-Host "##teamcity[importData type='nunit' path='$resultXml']"
    }

    Write-Host "##teamcity[blockClosed name='$Project']"
}

function RunTests($BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Running tests']"

    $projects = GetProjectsWithTests

    if ($projects -ne $null) {
        $parallelRunner = Get-ChildItem '.\packages\' | 
                                    Where-Object { $_.Name -like 'Machine.Specifications.TeamCityParallelRunner.*'} | 
                                    Select -First 1
        if ($parallelRunner) {
            $assemblies = $projects | ForEach-Object -Process {GetOutputAssembly $_ $BuildConfiguration} | Where-Object {(Test-Path $_) -and ($_ -notlike "*Mono*")}
            $assembliesJoined = [string]::Join(" ", $assemblies)

            $command = Join-Path ".\packages\$parallelRunner" "\tools\mspec-teamcity-prunner.exe --threads 3 $assembliesJoined"
            Write-Host $command
            iex $command | Write-Host
        }

        foreach ($project in $projects) {
            RunTestsFromProject $project $BuildConfiguration
        }
    }

    Write-Host "##teamcity[blockClosed name='Running tests']"
}

function RunConfigTransform($Project, $BuildConfiguration){
	$file = get-childitem $Project
	$PathToConfigFile = Join-Path $file.directoryname "Web.config"
	$PathToTransformFile = Join-Path $file.directoryname "Web.$BuildConfiguration.config"

	$command = "$(GetPathToConfigTransformator) $PathToConfigFile $PathToTransformFile $PathToConfigFile"
	iex $command
}

function AddArtifacts($Project, $BuildConfiguration, $folder) {
	$file = get-childitem $project
	$packagepath = $file.directoryname + "\obj\" + $BuildConfiguration + "\package\"

	$filename = $packagepath + $file.basename
	$zipfile = $filename + ".zip"
	$cmdfile = $filename + ".deploy.cmd"

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

	Copy-Item "$zipfile" "$artifactsFolder\$folder"
	Copy-Item "$cmdfile" "$artifactsFolder\$folder"
}

function BuildWebPackage($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building web package for project $Project']"
    Write-Host "##teamcity[progressStart 'Building web package for project $Project']"

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" '/verbosity:quiet' '/p:username=' '/p:CodeContractsRunCodeAnalysis=false' | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build web package for project $Project']"
        Write-Host "##teamcity[buildProblem description='Failed to build web package for project $Project']"
    }

    Write-Host "##teamcity[progressFinish 'Building web package for project $Project']"
    Write-Host "##teamcity[blockClosed name='Building web package for project $Project']"

    return $wasBuildSuccessfull
}

function CopyCapi($Project, $source) {
	$file = get-childitem $Project
	$DestinationFolder = $file.directoryname + "\Externals"

	If (Test-Path "$DestinationFolder"){
		Remove-Item "$DestinationFolder" -Force -Recurse
	}
	New-Item -ItemType directory -Path "$DestinationFolder"
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

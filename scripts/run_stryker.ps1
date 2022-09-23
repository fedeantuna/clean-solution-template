#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

#$StrykerDashboardApiKey = [System.Environment]::GetEnvironmentVariable('StrykerDashboardApiKey')

$RunningFromPipeline = [System.Environment]::GetEnvironmentVariable('RunningFromPipeline')
$StrykerDashboardBaseline = [System.Environment]::GetEnvironmentVariable('StrykerDashboardBaseline')
$StrykerDashboardVersion = [System.Environment]::GetEnvironmentVariable('StrykerDashboardVersion')
$StrykerExperimental = [System.Environment]::GetEnvironmentVariable('StrykerExperimental')

$RootProjectDirectory = $MyInvocation.MyCommand.Path | Split-Path -Parent | Split-Path -Parent
Push-Location $RootProjectDirectory

function Complete-FailSafely {
    param (
        [string]$Message
    )

    Pop-Location

    throw $Message
}

function Get-Solution {
    param(
        [string]$Solution
    )

    if ($Solution) {
        return "$RootProjectDirectory/$Solution"
    }

    $Solutions = Get-ChildItem -Path $RootProjectDirectory *.sln -File -Name

    $SolutionsCount = $Solutions.Count

    if ($SolutionsCount -gt 1) {
        Complete-FailSafely 'There is more than one solution, please specify which one should Stryker run against.'
    }

    if ($Solutions.Count -lt 1) {
        Complete-FailSafely 'There are no solutions in this directory.'
    }

    return (Join-Path -Path $RootProjectDirectory -ChildPath $Solutions)
}

function Get-SourceProjectPaths {
    param(
        [string]$Solution
    )

    $SourceProjectPaths = Select-String -Raw -Path $Solution -Pattern csproj | ForEach-Object {
        $_.Split(',')[1].Split('"')[1].Substring(0, $_.Split(',')[1].Split('"')[1].LastIndexOf('\'))
    } | Select-String -Raw -NotMatch -Pattern Tests | ForEach-Object {
        Join-Path -Path $RootProjectDirectory -ChildPath $_
    }

    return $SourceProjectPaths
}

function Get-DefaultStrykerOutputPath {
    param (
        [string]$SourceProjectPath
    )

    $StrykerOutput = Join-Path -Path $SourceProjectPath -ChildPath 'StrykerOutput'

    return $StrykerOutput
}

function Remove-DefaultStrykerOutputDirectories {
    param (
        [string[]]$SourceProjectPaths
    )

    $SourceProjectPaths | ForEach-Object {
        $StrykerOutput = Join-Path -Path $_ -ChildPath 'StrykerOutput'
        if (Test-Path -Path $StrykerOutput)
        {
            Remove-Item -Path $StrykerOutput -Force -Recurse
        }
    }
}

function New-StrykerOutputDirectories {
    param(
        [string]$StrykerResultsOutput,
        [string]$StrykerReportsOutput
    )

    if (-Not (Test-Path -Path $StrykerResultsOutput)) {
        New-Item -Path $StrykerResultsOutput -Type Directory | Out-Null
    }
    if (-Not (Test-Path -Path $StrykerReportsOutput)) {
        New-Item -Path $StrykerReportsOutput -Type Directory | Out-Null
    }
}

function Invoke-DotnetToolRestore {
    try
    {
        dotnet tool restore
    }
    catch
    {
        Complete-FailSafely $_
    }
}

function Get-StrykerDashboardReporterCommand {
    param (
        [string]$ProjectName
    )

    $StrykerCommand = "dotnet stryker -r dashboard --version $StrykerDashboardVersion"

    if ($StrykerExperimental -eq 'true') {
        $StrykerCommand += " --with-baseline:$StrykerDashboardBaseline"
    }

    return $StrykerCommand
}

function Get-StrykerJsonReporterCommand {
    $StrykerCommand = 'dotnet stryker -r json'

    return $StrykerCommand
}

function Invoke-DotnetStryker {
    param (
        [string]$Solution,
		[string]$StrykerCommand
    )

    Get-SourceProjectPaths $Solution | ForEach-Object {
        Set-Location $_

        Write-Host "Running: $StrykerCommand"

        try {
            Invoke-Expression $StrykerCommand
        } catch {
            Complete-FailSafely $_
        }
    }
}

function New-MergedStrykerJsonReport {
    param (
        [string]$StrykerResultsOutput,
        [string]$StrykerReportsOutput,
        [string[]]$SourceProjectPaths,
        [string]$StrykerMergedReportPath
    )

    $MutationReportJsonFragment = "{""schemaVersion"":""1"",""thresholds"":{""high"":80,""low"":60},""projectRoot"":""$EscapedRootProjectDirectory"",""files"":{"
    $MutationReportJsonFragment | Out-File -NoNewline -FilePath $StrykerMergedReportPath

    $Count = 0
    $SourceProjectPaths | ForEach-Object {
        Set-Location $_

        $ProjectName = Split-Path -Leaf $_

        $StrykerOutput = Get-DefaultStrykerOutputPath $_
        $StrykerUnmergedResultRelativePath = Get-ChildItem -Path $StrykerOutput *.json -File -Name -Recurse
        $StrykerUnmergedResultPath = Join-Path -Path $StrykerOutput -ChildPath $StrykerUnmergedResultRelativePath
        $StrykerUnmergedResultNewPath = Join-Path -Path $StrykerResultsOutput -ChildPath "$ProjectName.stryker.json"

        Move-Item -Path $StrykerUnmergedResultPath -Destination $StrykerUnmergedResultNewPath

        if ($Count -gt 0) {
            "," | Out-File -NoNewline -Append $StrykerMergedReportPath
        }

        try {
            $StrykerUnmergedResultContent = Get-Content $StrykerUnmergedResultNewPath
            $CurrentUnmergedStrykerResultFilesObject = [Regex]::Match($StrykerUnmergedResultContent, 'files":{(.*?)}}$').Groups[1].Value
            $CurrentUnmergedStrykerResultFilesObject | Out-File -NoNewline -Append $StrykerMergedReportPath

            $Count++
        } catch {
            Complete-FailSafely
        }
    }
    '}}' | Out-File -NoNewline -Append $StrykerMergedReportPath

    Set-Location $RootProjectDirectory
}

function New-MergedStrykerHtmlReport {
    param (
        [string]$StrykerReportsOutput,
        [string]$StrykerMergedReportPath
    )

    $StrykerMutationHtmlReport = Join-Path -Path $StrykerReportsOutput -ChildPath 'mutation-report.html'
    $StrykerMutationHtmlReportFileUrl = 'https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html'

    Invoke-WebRequest -Uri $StrykerMutationHtmlReportFileUrl -OutFile $StrykerMutationHtmlReport

    $StrykerMergedReportContent = Get-Content -Raw -Path $StrykerMergedReportPath
    ((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace '>##REPORT_JS##', ' defer src="https://www.unpkg.com/mutation-testing-elements">') | Set-Content -Path $StrykerMutationHtmlReport
    ((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace '##REPORT_TITLE##', 'Stryker Mutation Testing') | Set-Content -Path $StrykerMutationHtmlReport
    ((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace '##REPORT_JSON##', "$StrykerMergedReportContent") | Set-Content -Path $StrykerMutationHtmlReport

    Write-Information "Stryker HTML Report: $StrykerMutationHtmlReport"

    Pop-Location

    Invoke-Item $StrykerMutationHtmlReport
}

$Solution = Get-Solution
if (-Not (Test-Path -Path $Solution)) {
    Complete-FailSafely "The $Solution file doesn't exist."
}

$SourceProjectPaths = Get-SourceProjectPaths $Solution

Remove-DefaultStrykerOutputDirectories $SourceProjectPaths

$Timestamp = [int64](([datetime]::UtcNow) - (Get-Date '1/1/1970')).TotalSeconds

$StrykerResultsOutput = Join-Path -Path $RootProjectDirectory -ChildPath 'stryker-results' $Timestamp
$StrykerReportsOutput = Join-Path -Path $RootProjectDirectory -ChildPath 'stryker-reports' $Timestamp

New-StrykerOutputDirectories $StrykerResultsOutput $StrykerReportsOutput

Invoke-DotnetToolRestore

if ($RunningFromPipeline -eq 'true' ) {
	Invoke-DotnetStryker $Solution (Get-StrykerDashboardReporterCommand)

	exit 0
}

Invoke-DotnetStryker $Solution (Get-StrykerJsonReporterCommand)


if ($RunningFromPipeline -eq 'true') {
    exit 0
}

$StrykerMergedReportPath = "$StrykerReportsOutput/merged-mutation-report.json"
New-MergedStrykerJsonReport $StrykerResultsOutput $StrykerReportsOutput $SourceProjectPaths $StrykerMergedReportPath
New-MergedStrykerHtmlReport $StrykerReportsOutput $StrykerMergedReportPath

Remove-DefaultStrykerOutputDirectories $SourceProjectPaths

Pop-Location

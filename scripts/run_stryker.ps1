#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

$RootProjectDirectory = $MyInvocation.MyCommand.Path | Split-Path -Parent | Split-Path -Parent
Push-Location $RootProjectDirectory

if ($null -eq $args[0]) {
    $Solutions = Get-ChildItem -Path $RootProjectDirectory *.sln -File -Name

    $SolutionsCount = $Solutions.Count

    if ($SolutionsCount -gt 1) {
        Write-Host "There is more than one solution, please specify which one should Stryker run against."
        Pop-Location
        exit 1
    }

    if ($Solutions.Count -lt 1) {
        Write-Host "There are no solutions in this directory."
        Pop-Location
        exit 1
    }

    $Solution = [IO.Path]::Combine($RootProjectDirectory, $Solutions)
} else {
    $Solution = [IO.Path]::Combine($RootProjectDirectory, $args[0])
}

if (-Not (Test-Path -Path $Solution)) {
    Write-Host "The $Solution file doesn't exist."
    Pop-Location
    exit 1
}

$SourceProjects = Select-String -Raw -Path .\CleanSolutionTemplate.sln -Pattern csproj | ForEach-Object {
    $_.Split(',')[1].Split('"')[1].Substring(0, $_.Split(',')[1].Split('"')[1].LastIndexOf('\'))
} | Select-String -Raw -NotMatch -Pattern Tests | ForEach-Object {
    [IO.Path]::Combine($RootProjectDirectory, $_)
}

$SourceProjects | ForEach-Object {
    $StrykerOutput = [IO.Path]::Combine($_, "StrykerOutput")
    if (Test-Path -Path $StrykerOutput) {
        Remove-Item -Path $StrykerOutput -Force -Recurse
    }
}

$Timestamp = [int64](([datetime]::UtcNow) - (Get-Date "1/1/1970")).TotalSeconds

$StrykerResultsOutput = [IO.Path]::Combine($RootProjectDirectory, "stryker-results", $Timestamp)
if (-Not (Test-Path -Path $StrykerResultsOutput)) {
    New-Item -Path $StrykerResultsOutput -Type Directory | Out-Null
}
$StrykerReportsOutput = [IO.Path]::Combine($RootProjectDirectory, "stryker-reports", $Timestamp)
if (-Not (Test-Path -Path $StrykerReportsOutput)) {
    New-Item -Path $StrykerReportsOutput -Type Directory | Out-Null
}

try
{
    dotnet tool restore
}
catch
{
    Pop-Location
    throw $_
}

$StrykerMergedReport = [IO.Path]::Combine($StrykerReportsOutput, "merged-mutation-report.json")

$EscapedRootProjectDirectory = $RootProjectDirectory.Replace("\", "\\")
$MutationReportJsonFragment = "{""schemaVersion"":""1"",""thresholds"":{""high"":80,""low"":60},""projectRoot"":""$EscapedRootProjectDirectory"",""files"":{"
$MutationReportJsonFragment | Out-File -NoNewline -FilePath $StrykerMergedReport

$Count = 0
$RunningFromPipeline = [System.Environment]::GetEnvironmentVariable('RunningFromPipeline')
$SourceProjects | ForEach-Object {
    Set-Location $_

    $ProjectName = $_.Substring($_.LastIndexOf('\') + 1)

    if ($RunningFromPipeline -eq "true") {
        $StrykerCommand = "dotnet stryker -r dashboard"

        try
        {
            $StrykerProjectName = "github.com/fedeantuna/clean-solution-template"
            $StrykerModule = $ProjectName.Substring($ProjectName.LastIndexOf('.') + 1)

            $StrykerDashboardBaseline = [System.Environment]::GetEnvironmentVariable('StrykerDashboardBaseline')
            $StrykerDashboardVersion = [System.Environment]::GetEnvironmentVariable('StrykerDashboardVersion')

            $StrykerBaselineResult = "https://dashboard.stryker-mutator.io/api/reports/$StrykerProjectName/baseline/$StrykerDashboardBaseline`?module=$StrykerModule"

            $StrykerBaselineStatusCode = (Invoke-WebRequest -Uri $StrykerBaselineResult -UseBasicParsing -DisableKeepAlive).StatusCode

            if ($StrykerBaselineStatusCode -eq 200) {
                $StrykerCommand += " --with-baseline $StrykerDashboardBaseline --version $StrykerDashboardVersion"
            } else {
                Write-Information "No baseline found. Running full report."
            }
        }
        catch [Net.WebException]
        {
            Write-Information "No baseline found. Running full report."
        }

        $StrykerDashboardBaseline = [System.Environment]::GetEnvironmentVariable('StrykerDashboardBaseline')

    } else {
        $StrykerCommand = "dotnet stryker -r json"
    }

    try
    {
        Invoke-Expression $StrykerCommand
    }
    catch
    {
        Pop-Location
        throw $_
    }

	if ($RunningFromPipeline -eq "true") {
		return
	}

    $StrykerOutput = [IO.Path]::Combine($_, "StrykerOutput")
    $StrykerUnmergedResultRelativePath = Get-ChildItem -Path $StrykerOutput *.json -File -Name -Recurse
    $StrykerUnmergedResultPath = [IO.Path]::Combine($StrykerOutput, $StrykerUnmergedResultRelativePath)
    $StrykerUnmergedResultNewPath = [IO.Path]::Combine($StrykerResultsOutput, "$ProjectName.stryker.json")

    Move-Item -Path $StrykerUnmergedResultPath -Destination $StrykerUnmergedResultNewPath

    Remove-Item -Path $StrykerOutput -Force -Recurse

    if ($Count -gt 0) {
        "," | Out-File -NoNewline -Append $StrykerMergedReport
    }

    try {
        $StrykerUnmergedReultContent = Get-Content $StrykerUnmergedResultNewPath
        $CurrentUnmergedStrykerResultFilesObject = [Regex]::Match($StrykerUnmergedReultContent, "files"":{(.*?)}}$").Groups[1].Value
        $CurrentUnmergedStrykerResultFilesObject | Out-File -NoNewline -Append $StrykerMergedReport

        $Count++
    } catch {
        Pop-Location
        throw $_
    }
}
Set-Location $RootProjectDirectory

"}}" | Out-File -NoNewline -Append $StrykerMergedReport

if ($RunningFromPipeline -eq "true") {
    exit 0
}

$StrykerMutationHtmlReport = [IO.Path]::Combine($StrykerReportsOutput, "mutation-report.html")
$StrykerMutationHtmlReportFileUrl = "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html"

Write-Host -NoNewline "Generating Stryker HTML Report..."
Invoke-WebRequest -Uri $StrykerMutationHtmlReportFileUrl -OutFile $StrykerMutationHtmlReport

$StrykerMergedReportContent = Get-Content -Raw -Path $StrykerMergedReport
((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace ">##REPORT_JS##", " defer src=""https://www.unpkg.com/mutation-testing-elements"">") | Set-Content -Path $StrykerMutationHtmlReport
((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace "##REPORT_TITLE##", "Stryker Mutation Testing") | Set-Content -Path $StrykerMutationHtmlReport
((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace "##REPORT_JSON##", "$StrykerMergedReportContent") | Set-Content -Path $StrykerMutationHtmlReport

Write-Host "Stryker HTML Report: $StrykerMutationHtmlReport"

Pop-Location

Invoke-Item $StrykerMutationHtmlReport
#!/usr/bin/env pwsh

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

$StrykerLogsOutput = [IO.Path]::Combine($RootProjectDirectory, "stryker-logs", $Timestamp)
if (-Not (Test-Path -Path $StrykerLogsOutput)) {
    New-Item -Path $StrykerLogsOutput -Type Directory | Out-Null
}
$StrykerResultsOutput = [IO.Path]::Combine($RootProjectDirectory, "stryker-results", $Timestamp)
if (-Not (Test-Path -Path $StrykerResultsOutput)) {
    New-Item -Path $StrykerResultsOutput -Type Directory | Out-Null
}
$StrykerReportsOutput = [IO.Path]::Combine($RootProjectDirectory, "stryker-reports", $Timestamp)
if (-Not (Test-Path -Path $StrykerReportsOutput)) {
    New-Item -Path $StrykerReportsOutput -Type Directory | Out-Null
}

$DotnetToolLog = [IO.Path]::Combine($StrykerLogsOutput, "dotnet-tool.log")
Write-Host -NoNewline "Restoring dotnet tools..."
dotnet tool restore > $DotnetToolLog 2>&1
if ($?) {
    Write-Host "[OK]"
} else {
    Write-Host "[ERROR]"
    Pop-Location
    exit 1
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
    $CurrentProjectStrykerLog = [IO.Path]::Combine($StrykerLogsOutput, "$ProjectName.stryker.log")
    
    if ($RunningFromPipeline -eq "true") {
        $DotnetStrykerDashboardBaseline = [System.Environment]::GetEnvironmentVariable('DotnetStrykerDashboardBaseline')
        $DotnetStrykerDashboardVersion = [System.Environment]::GetEnvironmentVariable('DotnetStrykerDashboardVersion')

        $DotnetStrykerCommand = "dotnet stryker -r dashboard --with-baseline $DotnetStrykerDashboardBaseline --version $DotnetStrykerDashboardVersion"
    } else {
        $DotnetStrykerCommand = "dotnet stryker -r json"
    }

    Write-Host -NoNewline "Running Stryker for $ProjectName..."
    
    Invoke-Expression $DotnetStrykerCommand > $CurrentProjectStrykerLog 2>&1

    $StrykerOutput = [IO.Path]::Combine($_, "StrykerOutput")
    $StrykerUnmergedResultRelativePath = Get-ChildItem -Path $StrykerOutput *.json -File -Name -Recurse
    $StrykerUnmergedResultPath = [IO.Path]::Combine($StrykerOutput, $StrykerUnmergedResultRelativePath)
    $StrykerUnmergedResultNewPath = [IO.Path]::Combine($StrykerResultsOutput, "$ProjectName.stryker.json")

    Move-Item -Path $StrykerUnmergedResultPath -Destination $StrykerUnmergedResultNewPath

    Remove-Item -Path $StrykerOutput -Force -Recurse

    if ($Count -gt 0) {
        "," | Out-File -NoNewline -Append $StrykerMergedReport
    }

    $StrykerUnmergedReultContent = Get-Content $StrykerUnmergedResultNewPath
    $CurrentUnmergedStrykerResultFilesObject = [Regex]::Match($StrykerUnmergedReultContent, "files"":{(.*?)}}$").Groups[1].Value
    $CurrentUnmergedStrykerResultFilesObject | Out-File -NoNewline -Append $StrykerMergedReport

    if ($?) {
        $Count++
        Write-Host "[OK]"
    } else {
        Write-Host "[ERROR]"
        Pop-Location
        exit 1
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

Write-Host "[OK]"

Write-Host "Stryker HTML Report: $StrykerMutationHtmlReport"

Pop-Location

Invoke-Item $StrykerMutationHtmlReport

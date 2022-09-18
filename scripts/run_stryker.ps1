#!/usr/bin/env pwsh

$RootProjectDir = $MyInvocation.MyCommand.Path | Split-Path -Parent | Split-Path -Parent
Push-Location $RootProjectDir

if ($null -eq $args[0]) {
    $Solutions = Get-ChildItem -Path $RootProjectDir *.sln -File -Name

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

    $Solution = [IO.Path]::Combine($RootProjectDir, $Solutions)
    $SolutionName = $Solutions.Substring(0, $Solutions.LastIndexOf('.'))
} else {
    $Solution = [IO.Path]::Combine($RootProjectDir, $args[0])
    $SolutionName = $Solution.Substring(0, $args[0].LastIndexOf('.'))
}

if (-Not (Test-Path -Path $Solution)) {
    Write-Host "The $Solution file doesn't exist."
    Pop-Location
    exit 1
}

$StrykerOutputPath = [IO.Path]::Combine($RootProjectDir, "StrykerOutput")
if (Test-Path -Path $StrykerOutputPath) {
    Remove-Item -LiteralPath $StrykerOutputPath -Recurse -Force
}

$Timestamp = [int64](([datetime]::UtcNow) - (Get-Date "1/1/1970")).TotalSeconds

$StrykerLogsOutput = [IO.Path]::Combine($RootProjectDir, "stryker-logs", $Timestamp)
if (-Not (Test-Path -Path $StrykerLogsOutput)) {
    New-Item -Path $StrykerLogsOutput -Type Directory | Out-Null
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

$ProjectNames = Select-String -Raw -Path $Solution -Pattern csproj | ForEach-Object {
    $_.Split(',')[0].Split('=')[1].Split('"')[1]
} | Select-String -Raw -NotMatch -Pattern Tests

foreach($ProjectName in $ProjectNames) {
    Write-Host -NoNewline "Running Stryker for $ProjectName..."

    $DotnetStrykerCommand = "dotnet stryker -r json -p $ProjectName.csproj"

    $TestProjectRelativePath = (Select-String -Raw -Path $Solution -Pattern "$ProjectName.Tests.*.csproj").Split(',')[1].Split('"')[1]
    $SplittedTestProjectRelativePath = $TestProjectRelativePath.Split('\')
    $TestProjectPath = [IO.Path]::Combine([String[]] ($RootProjectDir, @($SplittedTestProjectRelativePath[0]) + $SplittedTestProjectRelativePath[1..$($SplittedTestProjectRelativePath.Count - 1)] -Replace '^\\', ''))

    $DotnetStrykerCommand += " -tp $TestProjectPath"

    if ($ProjectName -match "Api") {
        $CrossLayerTestRelativePaths = Select-String -Raw -Path $Solution -Pattern "$SolutionName.Tests.*.csproj" | ForEach-Object {
            $_.Split(',')[1].Split('"')[1]
        }
        foreach($CrossLayerTestRelativePath in $CrossLayerTestRelativePaths) {
            $SplittedCrossLayerTestRelativePath = $CrossLayerTestRelativePath.Split('\')
            $CrossLayerTestPath = [IO.Path]::Combine([String[]] ($RootProjectDir, @($SplittedCrossLayerTestRelativePath[0]) + $SplittedCrossLayerTestRelativePath[1..$($SplittedCrossLayerTestRelativePath.Count - 1)] -Replace '^\\', ''))
            $DotnetStrykerCommand += " -tp $CrossLayerTestPath"
        }
    }

    $CurrentProjectStrykerLog = [IO.Path]::Combine($StrykerLogsOutput, "$ProjectName.stryker.log")
    Invoke-Expression $DotnetStrykerCommand > $CurrentProjectStrykerLog 2>&1
    if ($?) {
        Write-Host "[OK]"
    } else {
        Write-Host "[ERROR]"
        Pop-Location
        exit 1
    }
}

$StrykerReportsOutput = [IO.Path]::Combine($RootProjectDir, "stryker-reports", $Timestamp)
$StrykerResultsOutput = [IO.Path]::Combine($RootProjectDir, "stryker-results", $Timestamp)

$StrykerMergedReport = [IO.Path]::Combine($StrykerReportsOutput, "merged-mutation-report.json")

if (-Not (Test-Path -Path $StrykerReportsOutput)) {
    New-Item -Path $StrykerReportsOutput -Type Directory | Out-Null
}
if (-Not (Test-Path -Path $StrykerResultsOutput)) {
    New-Item -Path $StrykerResultsOutput -Type Directory | Out-Null
}

$EscapedRootProjectDir = $RootProjectDir.Replace("\", "\\")
$MutationReportJsonFragment = "{""schemaVersion"":""1"",""thresholds"":{""high"":80,""low"":60},""projectRoot"":""$EscapedRootProjectDir"",""files"":{"
$MutationReportJsonFragment | Out-File -NoNewline -FilePath $StrykerMergedReport

Write-Host -NoNewline "Merging Stryker reports..."
$Count = 0
$StrykerUnmergedResults = Get-ChildItem -Path $StrykerOutputPath *.json -File -Name -Recurse | ForEach-Object {
    [IO.Path]::Combine($StrykerOutputPath, $_)
}
foreach ($StrykerUnmergedResult in $StrykerUnmergedResults) {
    $StrykerUnmergedReultContent = Get-Content $StrykerUnmergedResult

    $CurrentProjectPath = [Regex]::Match($StrykerUnmergedReultContent, """projectRoot"":""(.*?)"",""files""").Groups[1].Value
    $StrykerUnmergedResultProjectName = $CurrentProjectPath.Substring($CurrentProjectPath.LastIndexOf('\') + 1, $CurrentProjectPath.Length - $CurrentProjectPath.LastIndexOf('\') - 1).ToLower()

    $CurrentStrykerUnmergedResult = [IO.Path]::Combine($StrykerResultsOutput, "$StrykerUnmergedResultProjectName.json")
    Move-Item -Path $StrykerUnmergedResult -Destination $CurrentStrykerUnmergedResult

    if ($Count -gt 0) {
        "," | Out-File -NoNewline -Append $StrykerMergedReport
    }

    $CurrentUnmergedStrykerReportFiles = [Regex]::Match($StrykerUnmergedReultContent, "files"":{(.*?)}}$").Groups[1].Value
    $CurrentUnmergedStrykerReportFiles | Out-File -NoNewline -Append $StrykerMergedReport

    $Count++
}

"}}" | Out-File -NoNewline -Append $StrykerMergedReport

Remove-Item -LiteralPath $StrykerOutputPath -Recurse -Force

Write-Host "[OK]"

$StrykerMutationHtmlReport = [IO.Path]::Combine($StrykerReportsOutput, "mutation-report.html")
$StrykerMutationHtmlReportFileUrl = "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html"

Write-Host -NoNewline "Generating Stryker HTML Report..."
Invoke-WebRequest -Uri $StrykerMutationHtmlReportFileUrl -OutFile $StrykerMutationHtmlReport

$StrykerMergedReportContent = Get-Content -Raw -Path $StrykerMergedReport
((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace ">##REPORT_JS##", " defer src=""https://www.unpkg.com/mutation-testing-elements"">") | Set-Content -Path $StrykerMutationHtmlReport
((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace "##REPORT_TITLE##", "Stryker Mutation Testing") | Set-Content -Path $StrykerMutationHtmlReport
((Get-Content -Raw -Path $StrykerMutationHtmlReport) -replace "##REPORT_JSON##", "$StrykerMergedReportContent") | Set-Content -Path $StrykerMutationHtmlReport

Pop-Location

Write-Host "[OK]"

Write-Host "Stryker HTML Report: $StrykerMutationHtmlReport"

Invoke-Item $StrykerMutationHtmlReport

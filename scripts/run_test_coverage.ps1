#!/usr/bin/env pwsh

$RootProjectDir = $MyInvocation.MyCommand.Path | Split-Path -Parent | Split-Path -Parent
Push-Location $RootProjectDir

$Timestamp = [int64](([datetime]::UtcNow) - (Get-Date "1/1/1970")).TotalSeconds
$DotnetTestOutput = [IO.Path]::Combine($RootProjectDir, "test-results", $Timestamp)
$ReportGeneratorOutput = [IO.Path]::Combine($RootProjectDir, "test-reports", $Timestamp)

$DotnetTestCollect = "XPlat Code Coverage"
$DotnetTestLogger = "console;verbosity=detailed"
$DotnetCoberturaReports = [IO.Path]::Combine($DotnetTestOutput, "**", "*.cobertura.xml")

$TestLogsOutput = [IO.Path]::Combine($RootProjectDir, "test-logs", $Timestamp)
New-Item -Path "$TestLogsOutput" -Type Directory | Out-Null

$DotnetToolLog = [IO.Path]::Combine($TestLogsOutput, "dotnet-tool.log")
Write-Information -NoNewline "Restoring dotnet tools..."
dotnet tool restore > $DotnetToolLog 2>&1
if ($?) {
    Write-Information "[OK]"
} else {
    Write-Error "[ERROR]"
    Pop-Location
    exit 1
}

$DotnetTestLog = [IO.Path]::Combine($TestLogsOutput, "dotnet-test.log")
Write-Information -NoNewline "Running tests..."
dotnet test --collect:"$DotnetTestCollect" --logger:"$DotnetTestLogger" --results-directory $DotnetTestOutput $RootProjectDir > $DotnetTestLog 2>&1
if ($?) {
    Write-Information "[OK]"
} else {
    Write-Error "[ERROR]"
    Pop-Location
    exit 1
}

$DotnetReportGeneratorLog = [IO.Path]::Combine($TestLogsOutput, "dotnet-report-generator.log")
Write-Information -NoNewline "Merging reports..."
dotnet reportgenerator "-reports:$DotnetCoberturaReports" "-targetdir:$ReportGeneratorOutput" "-reporttypes:HTML;" > $DotnetReportGeneratorLog 2>&1
if ($?) {
    Write-Information "[OK]"
} else {
    Write-Error "[ERROR]"
    Pop-Location
    exit 1
}

Pop-Location

Invoke-Item $ReportGeneratorOutput/index.html

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
Write-Verbose -Message "Restoring dotnet tools..."
dotnet tool restore > $DotnetToolLog 2>&1
if ($?) {
    Write-Verbose -Message "dotnet tools restored"
} else {
    Write-Verbose -Message "Error restoring dotnet tools"
    Pop-Location
    exit 1
}

$DotnetTestLog = [IO.Path]::Combine($TestLogsOutput, "dotnet-test.log")
Write-Verbose -Message "Running tests..."
dotnet test --collect:"$DotnetTestCollect" --logger:"$DotnetTestLogger" --results-directory $DotnetTestOutput $RootProjectDir > $DotnetTestLog 2>&1
if ($?) {
    Write-Verbose -Message "Tests run successfully"
} else {
    Write-Verbose -Message "Error running tests"
    Pop-Location
    exit 1
}

$DotnetReportGeneratorLog = [IO.Path]::Combine($TestLogsOutput, "dotnet-report-generator.log")
Write-Verbose -Message "Merging reports..."
dotnet reportgenerator "-reports:$DotnetCoberturaReports" "-targetdir:$ReportGeneratorOutput" "-reporttypes:HTML;" > $DotnetReportGeneratorLog 2>&1
if ($?) {
    Write-Verbose -Message "Reports merged successfully"
} else {
    Write-Verbose -Message "Error merging reports"
    Pop-Location
    exit 1
}

Pop-Location

Invoke-Item $ReportGeneratorOutput/index.html

#!/usr/bin/env pwsh

$Timestamp = [int64](([datetime]::UtcNow) - (Get-Date "1/1/1970")).TotalSeconds

$RootProjectDir = $MyInvocation.MyCommand.Path | Split-Path -Parent | Split-Path -Parent
Push-Location $RootProjectDir

$DotnetTestOutput = [IO.Path]::Combine($RootProjectDir, "test-results", $Timestamp)
$ReportGeneratorOutput = [IO.Path]::Combine($RootProjectDir, "test-reports", $Timestamp)

$DotnetTestCollect = "XPlat Code Coverage"
$DotnetTestLogger = "console;verbosity=detailed"
$DotnetCoberturaReports = [IO.Path]::Combine($DotnetTestOutput, "**", "*.cobertura.xml")

dotnet test --collect:"$DotnetTestCollect" --logger:"$DotnetTestLogger" --results-directory $DotnetTestOutput $RootProjectDir
dotnet reportgenerator "-reports:$DotnetCoberturaReports" "-targetdir:$ReportGeneratorOutput" "-reporttypes:HTML;"

Pop-Location

Invoke-Item $ReportGeneratorOutput/index.html


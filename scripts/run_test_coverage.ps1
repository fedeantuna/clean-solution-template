#!/usr/bin/env pwsh

$Timestamp = Get-Date -UFormat %s

$RootProjectDir = $MyInvocation.MyCommand.Path | Split-Path -Parent | Split-Path -Parent
Push-Location $RootProjectDir

$DotnetTestOutput = "$RootProjectDir/test-results/$Timestamp"
$ReportGeneratorOutput = "$RootProjectDir/test-reports/$Timestamp"

$DotnetTestCollect = "XPlat Code Coverage"
$DotnetTestLogger = "console;verbosity=detailed"
$DotnetCoberturaReports = "$DotnetTestOutput/**/*.cobertura.xml"

dotnet test --collect:"$DotnetTestCollect" --logger:"$DotnetTestLogger" --results-directory $DotnetTestOutput $RootProjectDir
dotnet reportgenerator "-reports:$DotnetCoberturaReports" "-targetdir:$ReportGeneratorOutput" "-reporttypes:HTML;"

Pop-Location

Invoke-Item $ReportGeneratorOutput/index.html


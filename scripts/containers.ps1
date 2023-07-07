#!/usr/bin/env pwsh

param([string]$Command)

function Start-Containers() {
  $PostgresContainer = (podman ps | Select-String -Pattern postgres-CleanSolutionTemplate).Count
  if ($PostgresContainer -eq 0) {
    podman run --name postgres-CleanSolutionTemplate -e POSTGRES_PASSWORD=password -p 5433:5432 --rm -d postgres:15.3-alpine3.18
  }

  $TestIdentityServerContainer = (podman ps | Select-String -Pattern test-identity-server-CleanSolutionTemplate).Count
  if ($TestIdentityServerContainer -eq 0) {
    podman run --name test-identity-server-CleanSolutionTemplate -p 3210:80 --rm -d fedeantuna/test-identity-server:v1.0.1
  }
}

function Stop-Containers() {
    podman stop postgres-CleanSolutionTemplate
    podman stop test-identity-server-CleanSolutionTemplate
}

if ($Command -eq "start") {
  Start-Containers
} elseif ($Command -eq "stop") {
  Stop-Containers
} else {
  Write-Output "No action specified. Valid values are 'start' or 'stop'"
}

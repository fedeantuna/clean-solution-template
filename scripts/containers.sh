#!/usr/bin/env bash

COMMAND=$1

function start_containers() {
  POSTGRES_CONTAINER=$(podman ps | grep postgres-CleanSolutionTemplate | wc -l)
  if [[ $POSTGRES_CONTAINER -eq 0 ]]; then
    podman run --name postgres-CleanSolutionTemplate -e POSTGRES_PASSWORD=password -p 5433:5432 --rm -d postgres:15.3-alpine3.18
  fi

  TEST_IDENTITY_SERVER_CONTAINER=$(podman ps | grep test-identity-server-CleanSolutionTemplate | wc -l)
  if [[ $TEST_IDENTITY_SERVER_CONTAINER -eq 0 ]]; then
    podman run --name test-identity-server-CleanSolutionTemplate -p 3210:80 --rm -d fedeantuna/test-identity-server:v1.0.1
  fi
}

function stop_containers() {
    postgres stop postgres-CleanSolutionTemplate
    postgres stop test-identity-server-CleanSolutionTemplate
}

if [[ $COMMAND == "start" ]]; then
  start_containers
elif [[ $COMMAND == "stop" ]]; then
  stop_containers
else
  echo "No action specified. Valid values are 'start' or 'stop'"
fi

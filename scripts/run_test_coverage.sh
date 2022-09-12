#!/usr/bin/env bash

TIMESTAMP=$(date +%s)

ROOT_PROJECT_DIR=$(echo "$(dirname -- "$(readlink -f "${BASH_SOURCE}")")" | sed 's/scripts$//')

DOTNET_TEST_OUTPUT="$ROOT_PROJECT_DIR/test-results/$TIMESTAMP"
REPORT_GENERATOR_OUTPUT="$ROOT_PROJECT_DIR/test-reports/$TIMESTAMP"

DOTNET_TEST_COLLECT="XPlat Code Coverage"
DOTNET_TEST_LOGGER="console;verbosity=detailed"
DOTNET_COBERTURA_REPORTS="$DOTNET_TEST_OUTPUT/**/*.cobertura.xml"

dotnet test --collect:"$DOTNET_TEST_COLLECT" --logger:"$DOTNET_TEST_LOGGER" --results-directory $DOTNET_TEST_OUTPUT $ROOT_PROJECT_DIR
$(cd $ROOT_PROJECT_DIR && dotnet tool restore && dotnet reportgenerator "-reports:$DOTNET_COBERTURA_REPORTS" "-targetdir:$REPORT_GENERATOR_OUTPUT" "-reporttypes:HTML;")

open $REPORT_GENERATOR_OUTPUT/index.html


#!/usr/bin/env bash

ROOT_PROJECT_DIR=$(dirname -- "$(readlink -f "${BASH_SOURCE[0]}")" | sed 's/\/scripts$//')
cd "$ROOT_PROJECT_DIR" || exit 1

TIMESTAMP=$(date +%s)
DOTNET_TEST_OUTPUT="$ROOT_PROJECT_DIR/test-results/$TIMESTAMP"
REPORT_GENERATOR_OUTPUT="$ROOT_PROJECT_DIR/test-reports/$TIMESTAMP"

DOTNET_TEST_COLLECT="XPlat Code Coverage"
DOTNET_TEST_LOGGER="console;verbosity=detailed"
DOTNET_COBERTURA_REPORTS="$DOTNET_TEST_OUTPUT/**/*.cobertura.xml"

TEST_LOGS_OUTPUT="$ROOT_PROJECT_DIR/test-logs/$TIMESTAMP"
mkdir -p "$TEST_LOGS_OUTPUT"

DOTNET_TOOL_LOG="$TEST_LOGS_OUTPUT/dotnet-tool.log"
echo -n "Restoring dotnet tools..."
if dotnet tool restore > "$DOTNET_TOOL_LOG" 2>&1; then
    echo "[OK]"
else
    echo "[ERROR]"
    cd - > /dev/null || exit 1
    exit 1
fi

DOTNET_TEST_LOG="$TEST_LOGS_OUTPUT/dotnet-test.log"
echo -n "Running tests..."
if dotnet test --collect:"$DOTNET_TEST_COLLECT" --logger:"$DOTNET_TEST_LOGGER" --results-directory "$DOTNET_TEST_OUTPUT" "$ROOT_PROJECT_DIR" > "$DOTNET_TEST_LOG" 2>&1; then
    echo "[OK]"
else
    echo "[ERROR]"
    cd - > /dev/null || exit 1
    exit 1
fi

DOTNET_REPORT_GENERATOR_LOG="$TEST_LOGS_OUTPUT/dotnet-report-generator.log"
echo -n "Merging reports..."
if dotnet reportgenerator "-reports:$DOTNET_COBERTURA_REPORTS" "-targetdir:$REPORT_GENERATOR_OUTPUT" "-reporttypes:HTML;" > "$DOTNET_REPORT_GENERATOR_LOG" 2>&1; then
    echo "[OK]"
else
    echo "[ERROR]"
    cd - > /dev/null || exit 1
    exit 1
fi

cd - > /dev/null || exit 1

open "$REPORT_GENERATOR_OUTPUT/index.html"

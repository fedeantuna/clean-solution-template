#!/usr/bin/env bash

ROOT_PROJECT_DIR=$(dirname -- "$(readlink -f "${BASH_SOURCE[0]}")" | sed 's/\/scripts$//')
cd "$ROOT_PROJECT_DIR" || exit 1

if [[ -z $1 ]]; then
    SOLUTIONS=$(find ./ -name '*.sln' | sed 's/\.\///')

    SOLUTIONS_COUNT=$(echo "$SOLUTIONS" | wc -l)

    if [[ $SOLUTIONS_COUNT -gt 1 ]]; then
        echo "There is more than one solution, please specify which one should Stryker run against."
        cd - > /dev/null || exit 1
        exit 1
    fi

    if [[ $SOLUTIONS_COUNT -lt 1 ]]; then
        echo "There are no solutions in this directory."
        cd - > /dev/null || exit 1
        exit 1
    fi

    SOLUTION="$ROOT_PROJECT_DIR/$SOLUTIONS"
else
    SOLUTION="$ROOT_PROJECT_DIR/$1"
fi

if [[ ! -r "$SOLUTION" ]]; then
    echo "The $SOLUTION file is not readable or doesn't exist."
    cd - > /dev/null || exit 1
    exit 1
fi

rm -rf "$ROOT_PROJECT_DIR/StrykerOutput"

TIMESTAMP=$(date +%s)

STRYKER_LOGS_OUTPUT="$ROOT_PROJECT_DIR/stryker-logs/$TIMESTAMP"
mkdir -p "$STRYKER_LOGS_OUTPUT"

DOTNET_TOOL_LOG="$STRYKER_LOGS_OUTPUT/dotnet-tool.log"
echo -n "Restoring dotnet tools..."
if dotnet tool restore > "$DOTNET_TOOL_LOG" 2>&1; then
    echo "[OK]"
else
    echo "[ERROR]"
    cd - > /dev/null || exit 1
    exit 1
fi

PROJECT_NAMES=$(grep csproj "$SOLUTION" | cut -d , -f 1 | cut -d = -f 2 | cut -d \" -f 2 | grep -v Tests)
for PROJECT_NAME in $PROJECT_NAMES; do
    echo -n "Running Stryker for $PROJECT_NAME..."

    DOTNET_STRYKER_COMMAND="dotnet stryker -r json -p $PROJECT_NAME.csproj"

    TEST_PROJECT_PATH=$ROOT_PROJECT_DIR/$(grep "${PROJECT_NAME}.Tests.*.csproj" "$SOLUTION" | cut -d , -f 2 | cut -d \" -f 2 | sed 's/\\/\//g')
    DOTNET_STRYKER_COMMAND+=" -tp $TEST_PROJECT_PATH"

    if [[ $PROJECT_NAME =~ Api ]]; then
        SOLUTION_NAME=$(echo "$SOLUTION" | rev | cut -d / -f 1 | rev | cut -d . -f 1)
        CROSS_LAYER_TEST_RELATIVE_PATHS=$(grep "$SOLUTION_NAME.Tests.*.csproj" "$SOLUTION" | cut -d , -f 2 | cut -d \" -f 2 | sed 's/\\/\//g')
        for CROSS_LAYER_TEST_RELATIVE_PATH in $CROSS_LAYER_TEST_RELATIVE_PATHS; do
            DOTNET_STRYKER_COMMAND+=" -tp $ROOT_PROJECT_DIR/$CROSS_LAYER_TEST_RELATIVE_PATH"
        done
    fi

    CURRENT_PROJECT_STRYKER_LOG="$STRYKER_LOGS_OUTPUT/$PROJECT_NAME.stryker.log"
    if $DOTNET_STRYKER_COMMAND > "$CURRENT_PROJECT_STRYKER_LOG" 2>&1; then
        echo "[OK]"
    else
        echo "[ERROR]"
        cd - > /dev/null || exit 1
        exit 1
    fi
done

STRYKER_REPORTS_OUTPUT="$ROOT_PROJECT_DIR/stryker-reports/$TIMESTAMP"
STRYKER_RESULTS_OUTPUT="$ROOT_PROJECT_DIR/stryker-results/$TIMESTAMP"

STRYKER_MERGED_REPORT="$STRYKER_REPORTS_OUTPUT/merged-mutation-report.json"

mkdir -p "$STRYKER_REPORTS_OUTPUT"
mkdir -p "$STRYKER_RESULTS_OUTPUT"

MUTATION_REPORT_JSON_FRAGMENT="{\"schemaVersion\":\"1\",\"thresholds\":{\"high\":80,\"low\":60},\"projectRoot\":\"$ROOT_PROJECT_DIR\",\"files\":{"
echo -n "$MUTATION_REPORT_JSON_FRAGMENT" > "$STRYKER_MERGED_REPORT"

echo -n "Merging Stryker reports..."
COUNT=0
STRYKER_UNMERGED_RESULTS=$(find "$ROOT_PROJECT_DIR/StrykerOutput" -name '*.json' | sed 's/\.\///')
for STRYKER_UNMERGED_RESULT in $STRYKER_UNMERGED_RESULTS; do
    STRYKER_UNMERGED_RESULT_PROJECT_NAME=$(grep -oP '(?<="projectRoot":").*(?=","files")' "$STRYKER_UNMERGED_RESULT" | sed 's/\\/\//g' | rev | cut -d / -f 1 | rev | tr '[:upper:]' '[:lower:]')

    CURRENT_STRYKER_UNMERGED_RESULT="$STRYKER_RESULTS_OUTPUT/$STRYKER_UNMERGED_RESULT_PROJECT_NAME.json"
    mv "$STRYKER_UNMERGED_RESULT" "$CURRENT_STRYKER_UNMERGED_RESULT"

    [[ $COUNT -gt 0 ]] && echo -n "," >> "$STRYKER_MERGED_REPORT"
    grep -oP '(?<=files":{).*(?=}}$)' "$CURRENT_STRYKER_UNMERGED_RESULT" | tr -d "\n" >> "$STRYKER_MERGED_REPORT"

    COUNT=$((COUNT + 1))
done

echo "}}" >> "$STRYKER_MERGED_REPORT"

rm -rf "$ROOT_PROJECT_DIR/StrykerOutput"

echo "[OK]"

STRYKER_MUTATION_HTML_REPORT="$STRYKER_REPORTS_OUTPUT/mutation-report.html"
STRYKER_MUTATION_HTML_REPORT_FILE_URL="https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html"

echo -n "Generating Stryker HTML Report..."
curl -s "$STRYKER_MUTATION_HTML_REPORT_FILE_URL" --output "$STRYKER_MUTATION_HTML_REPORT" > /dev/null || exit 1

sed -i 's/>##REPORT_JS##/ defer src="https:\/\/www.unpkg.com\/mutation-testing-elements">/' "$STRYKER_MUTATION_HTML_REPORT"
sed -i 's/##REPORT_TITLE##/Stryker Mutation Testing/' "$STRYKER_MUTATION_HTML_REPORT"
echo -n "$(cat "$STRYKER_MERGED_REPORT");" | sed -i -e '/##REPORT_JSON##/{r /dev/stdin' -e '}' "$STRYKER_MUTATION_HTML_REPORT"
sed -i 's/##REPORT_JSON##;//' "$STRYKER_MUTATION_HTML_REPORT"

cd - > /dev/null || exit 1

echo "[OK]"

echo "Stryker HTML Report: $STRYKER_MUTATION_HTML_REPORT"

open "$STRYKER_MUTATION_HTML_REPORT"

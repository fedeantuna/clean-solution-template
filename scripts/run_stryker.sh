#!/usr/bin/env bash

set -e

STARTUP_DIRECTORY=$(pwd)

ROOT_PROJECT_DIRECTORY=$(dirname -- "$(readlink -f "${BASH_SOURCE[0]}")" | sed 's/\/scripts$//')
cd "$ROOT_PROJECT_DIRECTORY"

if [[ -z $1 ]]; then
    SOLUTIONS=$(find ./ -name '*.sln' | sed 's/\.\///')

    SOLUTIONS_COUNT=$(echo "$SOLUTIONS" | wc -l)

    if [[ $SOLUTIONS_COUNT -gt 1 ]]; then
        echo "There is more than one solution, please specify which one should Stryker run against."
        cd "$STARTUP_DIRECTORY"
        exit 1
    fi

    if [[ $SOLUTIONS_COUNT -lt 1 ]]; then
        echo "There are no solutions in this directory."
        cd "$STARTUP_DIRECTORY"
        exit 1
    fi

    SOLUTION="$ROOT_PROJECT_DIRECTORY/$SOLUTIONS"
else
    SOLUTION="$ROOT_PROJECT_DIRECTORY/$1"
fi

if [[ ! -r "$SOLUTION" ]]; then
    echo "The $SOLUTION file is not readable or doesn't exist."
    cd "$STARTUP_DIRECTORY"
    exit 1
fi

SOURCE_PROJECT_RELATIVE_PATHS=$(grep "csproj" "$SOLUTION" | cut -d , -f 2 | cut -d \" -f 2 | grep -v Tests | rev | cut -d \\ -f 2- | rev | sed 's/\\/\//g')

for SOURCE_PROJECT_RELATIVE_PATH in $SOURCE_PROJECT_RELATIVE_PATHS; do
    SOURCE_PROJECT="$ROOT_PROJECT_DIRECTORY/$SOURCE_PROJECT_RELATIVE_PATH"
    STRYKER_OUTPUT="$SOURCE_PROJECT/StrykerOutput"
    rm -rf "$STRYKER_OUTPUT"
done

TIMESTAMP=$(date +%s)

STRYKER_RESULTS_OUTPUT="$ROOT_PROJECT_DIRECTORY/stryker-results/$TIMESTAMP"
mkdir -p "$STRYKER_RESULTS_OUTPUT"
STRYKER_REPORTS_OUTPUT="$ROOT_PROJECT_DIRECTORY/stryker-reports/$TIMESTAMP"
mkdir -p "$STRYKER_REPORTS_OUTPUT"

if ! dotnet tool restore; then
    cd "$STARTUP_DIRECTORY"
    exit 1
fi

STRYKER_MERGED_REPORT="$STRYKER_REPORTS_OUTPUT/merged-mutation-report.json"

MUTATION_REPORT_JSON_FRAGMENT="{\"schemaVersion\":\"1\",\"thresholds\":{\"high\":80,\"low\":60},\"projectRoot\":\"$ROOT_PROJECT_DIRECTORY\",\"files\":{"
echo -n "$MUTATION_REPORT_JSON_FRAGMENT" > "$STRYKER_MERGED_REPORT"

COUNT=0
[[ $RUNNING_FROM_PIPELINE == "true" ]] && STRYKER_PROJECT_NAME=$(echo "$CURRENT_REPOSITORY_URL" | cut -d / -f 3- | rev | cut -d . -f 2- | rev)
for SOURCE_PROJECT_RELATIVE_PATH in $SOURCE_PROJECT_RELATIVE_PATHS; do
    SOURCE_PROJECT="$ROOT_PROJECT_DIRECTORY/$SOURCE_PROJECT_RELATIVE_PATH"

    cd "$SOURCE_PROJECT"

    PROJECT_NAME=$(echo "$SOURCE_PROJECT" | rev | cut -d / -f 1 | rev)

    if [[ $RUNNING_FROM_PIPELINE == "true" ]]; then
        STRYKER_COMMAND="dotnet stryker -r dashboard --version $STRYKER_DASHBOARD_VERSION"

        STRYKER_MODULE=$(echo "$PROJECT_NAME" | rev | cut -d . -f 1 | rev)
        STRYKER_BASELINE_RESULT="https://dashboard.stryker-mutator.io/api/reports/$STRYKER_PROJECT_NAME/baseline/$STRYKER_DASHBOARD_BASELINE?module=$STRYKER_MODULE"

        if curl --output /dev/null --silent --head --fail "$STRYKER_BASELINE_RESULT"; then
            STRYKER_COMMAND+=" --with-baseline:$STRYKER_DASHBOARD_BASELINE"
        else
            echo "No baseline found. Running full report."
        fi
    else
        STRYKER_COMMAND="dotnet stryker -r json"
    fi

    echo "Running: $STRYKER_COMMAND"

    if ! $STRYKER_COMMAND; then
		cd "$STARTUP_DIRECTORY"
		exit 1
	fi

	[[ $RUNNING_FROM_PIPELINE == "true" ]] && continue

    STRYKER_OUTPUT="$SOURCE_PROJECT/StrykerOutput"
    STRYKER_UNMERGED_RESULT_PATH=$(find "$STRYKER_OUTPUT" -name '*.json')
    STRYKER_UNMERGED_RESULT_NEW_PATH="$STRYKER_RESULTS_OUTPUT/$PROJECT_NAME.stryker.json"

    mv "$STRYKER_UNMERGED_RESULT_PATH" "$STRYKER_UNMERGED_RESULT_NEW_PATH"

    rm -rf "$STRYKER_OUTPUT"

    [[ $COUNT -gt 0 ]] && echo -n "," >> "$STRYKER_MERGED_REPORT"

    [[ "$OSTYPE" =~ darwin* ]] && GREP_COMMAND="ggrep" || GREP_COMMAND="grep"
    if $GREP_COMMAND -oP '(?<=files":{).*(?=}}$)' "$STRYKER_UNMERGED_RESULT_NEW_PATH" | tr -d "\n" >> "$STRYKER_MERGED_REPORT"; then
        COUNT=$((COUNT + 1))
    else
        cd "$STARTUP_DIRECTORY"
        exit 1
    fi
done
cd "$ROOT_PROJECT_DIRECTORY"

echo "}}" >> "$STRYKER_MERGED_REPORT"

[[ $RUNNING_FROM_PIPELINE == "true" ]] && exit 0

STRYKER_MUTATION_HTML_REPORT="$STRYKER_REPORTS_OUTPUT/mutation-report.html"
STRYKER_MUTATION_HTML_REPORT_FILE_URL="https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html"

curl -s "$STRYKER_MUTATION_HTML_REPORT_FILE_URL" --output "$STRYKER_MUTATION_HTML_REPORT" > /dev/null

if [[ "$OSTYPE" =~ darwin* ]]; then
    gsed -i 's/>##REPORT_JS##/ defer src="https:\/\/www.unpkg.com\/mutation-testing-elements">/' "$STRYKER_MUTATION_HTML_REPORT"
    gsed -i 's/##REPORT_TITLE##/Stryker Mutation Testing/' "$STRYKER_MUTATION_HTML_REPORT"
    echo -n "$(cat "$STRYKER_MERGED_REPORT");" | gsed -i -e '/##REPORT_JSON##/{r /dev/stdin' -e '}' "$STRYKER_MUTATION_HTML_REPORT"
    gsed -i 's/##REPORT_JSON##;//' "$STRYKER_MUTATION_HTML_REPORT"
else
    sed -i 's/>##REPORT_JS##/ defer src="https:\/\/www.unpkg.com\/mutation-testing-elements">/' "$STRYKER_MUTATION_HTML_REPORT"
    sed -i 's/##REPORT_TITLE##/Stryker Mutation Testing/' "$STRYKER_MUTATION_HTML_REPORT"
    echo -n "$(cat "$STRYKER_MERGED_REPORT");" | sed -i -e '/##REPORT_JSON##/{r /dev/stdin' -e '}' "$STRYKER_MUTATION_HTML_REPORT"
    sed -i 's/##REPORT_JSON##;//' "$STRYKER_MUTATION_HTML_REPORT"
fi
echo "Stryker HTML Report: $STRYKER_MUTATION_HTML_REPORT"

cd "$STARTUP_DIRECTORY"

open "$STRYKER_MUTATION_HTML_REPORT"

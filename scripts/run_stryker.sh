#!/usr/bin/env bash

set -e

STARTUP_DIRECTORY=$(pwd)
ROOT_PROJECT_DIRECTORY=$(dirname -- "$(readlink -f "${BASH_SOURCE[0]}")" | sed 's/\/scripts$//')

cd "$ROOT_PROJECT_DIRECTORY"

function fail_safely() {
    [[ -n $1 ]] && echo "$1"
    cd "$STARTUP_DIRECTORY"

    return 1
}

function get_solution() {
    [[ -n $1 ]] && echo "$ROOT_PROJECT_DIRECTORY/$1" && return 0

    local _SOLUTIONS ; _SOLUTIONS=$(find ./ -name '*.sln' | sed 's/\.\///')
    local _SOLUTIONS_COUNT ; _SOLUTIONS_COUNT=$(echo "$_SOLUTIONS" | wc -l)

    [[ $_SOLUTIONS_COUNT -gt 1 ]] && fail_safely "There is more than one solution, please specify which one should Stryker run against."
    [[ $_SOLUTIONS_COUNT -lt 1 ]] && fail_safely "There are no solutions in this directory."

    echo "$ROOT_PROJECT_DIRECTORY/$_SOLUTIONS"

    return 0
}

function get_source_project_relative_paths() {
    grep "csproj" "$1" | cut -d , -f 2 | cut -d \" -f 2 | grep -v Tests | rev | cut -d \\ -f 2- | rev | sed 's/\\/\//g'

    return 0
}

function rm_stryker_output() {
    local _SOURCE_PROJECT_RELATIVE_PATHS ; _SOURCE_PROJECT_RELATIVE_PATHS=$1

    local _SOURCE_PROJECT_RELATIVE_PATH
    for _SOURCE_PROJECT_RELATIVE_PATH in $_SOURCE_PROJECT_RELATIVE_PATHS; do
        local _SOURCE_PROJECT ; _SOURCE_PROJECT="$ROOT_PROJECT_DIRECTORY/$_SOURCE_PROJECT_RELATIVE_PATH"
        local _STRYKER_OUTPUT ; _STRYKER_OUTPUT="$_SOURCE_PROJECT/StrykerOutput"
        rm -rf "$_STRYKER_OUTPUT"
    done

    return 0
}

function create_stryker_output_directories() {
    local _STRYKER_RESULTS_OUTPUT ; _STRYKER_RESULTS_OUTPUT=$1
    local _STRYKER_REPORTS_OUTPUT ; _STRYKER_REPORTS_OUTPUT=$2

    mkdir -p "$_STRYKER_RESULTS_OUTPUT"
    mkdir -p "$_STRYKER_REPORTS_OUTPUT"

    return 0
}

function restore_dotnet_tools() {
    dotnet tool restore || fail_safely

    return 0
}

function get_stryker_dashboard_reporter_command() {
    local _PROJECT_NAME ; _PROJECT_NAME=$1

    local _STRYKER_COMMAND ; _STRYKER_COMMAND="dotnet stryker -r dashboard --version $STRYKER_DASHBOARD_VERSION"

    local _STRYKER_PROJECT_NAME ; _STRYKER_PROJECT_NAME=$(echo "$CURRENT_REPOSITORY_URL" | cut -d / -f 3- | rev | cut -d . -f 2- | rev)
    local _STRYKER_MODULE ; _STRYKER_MODULE=$(echo "$_PROJECT_NAME" | rev | cut -d . -f 1 | rev)
    local _STRYKER_BASELINE_RESULT ; _STRYKER_BASELINE_RESULT="https://dashboard.stryker-mutator.io/api/reports/$_STRYKER_PROJECT_NAME/baseline/$STRYKER_DASHBOARD_BASELINE?module=$_STRYKER_MODULE"

    curl --output /dev/null --silent --head --fail "$_STRYKER_BASELINE_RESULT" \
        && _STRYKER_COMMAND+=" --with-baseline:$STRYKER_DASHBOARD_BASELINE"

    echo "$_STRYKER_COMMAND"

    return 0
}

function get_stryker_json_reporter_command() {
    local _STRYKER_COMMAND ; _STRYKER_COMMAND="dotnet stryker -r json"

    echo "$_STRYKER_COMMAND"

    return 0
}

function run_stryker() {
    local _SOLUTION ; _SOLUTION=$1

    local _SOURCE_PROJECT_RELATIVE_PATHS ; _SOURCE_PROJECT_RELATIVE_PATHS=$(grep "csproj" "$_SOLUTION" | cut -d \, -f 2 | cut -d \" -f 2 | grep -v Tests | rev | cut -d \\ -f 2- | rev | sed 's/\\/\//g')
    local _SOURCE_PROJECT_RELATIVE_PATH
    for _SOURCE_PROJECT_RELATIVE_PATH in $_SOURCE_PROJECT_RELATIVE_PATHS; do
        local _SOURCE_PROJECT ; _SOURCE_PROJECT="$ROOT_PROJECT_DIRECTORY/$_SOURCE_PROJECT_RELATIVE_PATH"

        cd "$_SOURCE_PROJECT"

        local _PROJECT_NAME ; _PROJECT_NAME=$(echo "$_SOURCE_PROJECT" | rev | cut -d / -f 1 | rev)

        local _STRYKER_COMMAND
        [[ $RUNNING_FROM_PIPELINE == "true" ]] \
            && _STRYKER_COMMAND=$(get_stryker_dashboard_reporter_command "$_PROJECT_NAME") \
            || _STRYKER_COMMAND=$(get_stryker_json_reporter_command)

        echo "Running: $_STRYKER_COMMAND"

        $_STRYKER_COMMAND || fail_safely
    done

    return 0
}

function create_json_mutation_report() {
    local _STRYKER_MERGED_REPORT ; _STRYKER_MERGED_REPORT=$1

    local _MUTATION_REPORT_JSON_FRAGMENT ; _MUTATION_REPORT_JSON_FRAGMENT="{\"schemaVersion\":\"1\",\"thresholds\":{\"high\":80,\"low\":60},\"projectRoot\":\"$ROOT_PROJECT_DIRECTORY\",\"files\":{"
    echo -n "$_MUTATION_REPORT_JSON_FRAGMENT" > "$_STRYKER_MERGED_REPORT"

    return 0
}

function generate_merged_stryker_json_report() {
    local _SOURCE_PROJECT_RELATIVE_PATHS ; _SOURCE_PROJECT_RELATIVE_PATHS=$1
    local _STRYKER_RESULTS_OUTPUT ; _STRYKER_RESULTS_OUTPUT=$2
    local _STRYKER_MERGED_REPORT ; _STRYKER_MERGED_REPORT=$3

    local _COUNT ; _COUNT=0
    local _SOURCE_PROJECT_RELATIVE_PATH
    for _SOURCE_PROJECT_RELATIVE_PATH in $_SOURCE_PROJECT_RELATIVE_PATHS; do
        local _SOURCE_PROJECT ; _SOURCE_PROJECT="$ROOT_PROJECT_DIRECTORY/$_SOURCE_PROJECT_RELATIVE_PATH"

        cd "$_SOURCE_PROJECT"

        local _PROJECT_NAME ; _PROJECT_NAME=$(echo "$_SOURCE_PROJECT" | rev | cut -d / -f 1 | rev)

        local _STRYKER_OUTPUT ; _STRYKER_OUTPUT="$_SOURCE_PROJECT/StrykerOutput"

        local _STRYKER_UNMERGED_RESULT_PATH ; _STRYKER_UNMERGED_RESULT_PATH=$(find "$_STRYKER_OUTPUT" -name '*.json')
        local _STRYKER_UNMERGED_RESULT_NEW_PATH ; _STRYKER_UNMERGED_RESULT_NEW_PATH="$_STRYKER_RESULTS_OUTPUT/$_PROJECT_NAME.stryker.json"

        mv "$_STRYKER_UNMERGED_RESULT_PATH" "$_STRYKER_UNMERGED_RESULT_NEW_PATH"

        rm -rf "$_STRYKER_OUTPUT"

        [[ $_COUNT -gt 0 ]] && echo -n "," >> "$_STRYKER_MERGED_REPORT"

        local _GREP_COMMAND
        [[ "$OSTYPE" =~ darwin* ]] && _GREP_COMMAND="ggrep" || _GREP_COMMAND="grep"
        if $_GREP_COMMAND -oP '(?<=files":{).*(?=}}$)' "$_STRYKER_UNMERGED_RESULT_NEW_PATH" | tr -d "\n" >> "$_STRYKER_MERGED_REPORT"; then
            _COUNT=$((_COUNT + 1))
        else
            fail_safely
        fi
    done
    cd "$ROOT_PROJECT_DIRECTORY"

    echo "}}" >> "$_STRYKER_MERGED_REPORT"

    return 0
}

function generate_merged_stryker_html_report() {
    local _STRYKER_REPORTS_OUTPUT ; _STRYKER_REPORTS_OUTPUT=$1

    local _STRYKER_MUTATION_HTML_REPORT_FILE_URL ; _STRYKER_MUTATION_HTML_REPORT_FILE_URL="https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html"
    local _STRYKER_MUTATION_HTML_REPORT ; _STRYKER_MUTATION_HTML_REPORT="$_STRYKER_REPORTS_OUTPUT/mutation-report.html"

    curl -s "$_STRYKER_MUTATION_HTML_REPORT_FILE_URL" --output "$_STRYKER_MUTATION_HTML_REPORT" > /dev/null

    local _SED_COMMAND
    [[ "$OSTYPE" =~ darwin* ]] \
        && _SED_COMMAND="gsed" \
        || _SED_COMMAND="sed"

    $_SED_COMMAND -i 's/>##REPORT_JS##/ defer src="https:\/\/www.unpkg.com\/mutation-testing-elements">/' "$_STRYKER_MUTATION_HTML_REPORT"
    $_SED_COMMAND -i 's/##REPORT_TITLE##/Stryker Mutation Testing/' "$_STRYKER_MUTATION_HTML_REPORT"
    echo -n "$(cat "$STRYKER_MERGED_REPORT");" | $_SED_COMMAND -i -e '/##REPORT_JSON##/{r /dev/stdin' -e '}' "$_STRYKER_MUTATION_HTML_REPORT"
    $_SED_COMMAND -i 's/##REPORT_JSON##;//' "$_STRYKER_MUTATION_HTML_REPORT"

    echo "Stryker HTML Report: $_STRYKER_MUTATION_HTML_REPORT"

    open "$_STRYKER_MUTATION_HTML_REPORT"
}

SOLUTION=$(get_solution "$1")
[[ ! -r "$SOLUTION" ]] && fail_safely "The $SOLUTION file is not readable or doesn't exist."

SOURCE_PROJECT_RELATIVE_PATHS=$(get_source_project_relative_paths "$SOLUTION")

rm_stryker_output "$SOURCE_PROJECT_RELATIVE_PATHS"

TIMESTAMP=$(date +%s)

STRYKER_RESULTS_OUTPUT="$ROOT_PROJECT_DIRECTORY/stryker-results/$TIMESTAMP"
STRYKER_REPORTS_OUTPUT="$ROOT_PROJECT_DIRECTORY/stryker-reports/$TIMESTAMP"

create_stryker_output_directories "$STRYKER_RESULTS_OUTPUT" "$STRYKER_REPORTS_OUTPUT"

restore_dotnet_tools

run_stryker "$SOLUTION"

[[ $RUNNING_FROM_PIPELINE == "true" ]] && exit 0

STRYKER_MERGED_REPORT="$STRYKER_REPORTS_OUTPUT/merged-mutation-report.json"
create_json_mutation_report "$STRYKER_MERGED_REPORT"

generate_merged_stryker_json_report "$SOURCE_PROJECT_RELATIVE_PATHS" "$STRYKER_RESULTS_OUTPUT" "$STRYKER_MERGED_REPORT"
generate_merged_stryker_html_report "$STRYKER_REPORTS_OUTPUT"

cd "$STARTUP_DIRECTORY"

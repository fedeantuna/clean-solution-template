#!/usr/bin/env bash

set -e

# STRYKER_DASHBOARD_API_KEY=''

# RUNNING_FROM_PIPELINE=''
# STRYKER_DASHBOARD_BASELINE=''
# STRYKER_DASHBOARD_VERSION=''
# STRYKER_EXPERIMENTAL=''

STARTUP_DIRECTORY=$(pwd)
ROOT_PROJECT_DIRECTORY=$(dirname -- "$(readlink -f "${BASH_SOURCE[0]}")" | sed 's/\/scripts$//')

cd "$ROOT_PROJECT_DIRECTORY"

function complete_fail_safely() {
    local _MESSAGE ; _MESSAGE=$1

    [[ -n $_MESSAGE ]] && echo "$_MESSAGE"

    cd "$STARTUP_DIRECTORY"

    exit 1
}

function get_solution() {
    local _SOLUTION ; _SOLUTION=$1

    [[ -n $_SOLUTION ]] \
        && echo "$ROOT_PROJECT_DIRECTORY/$_SOLUTION" \
        && return 0

    local _SOLUTIONS ; _SOLUTIONS=$(find ./ -name '*.sln' | sed 's/\.\///')
    local _SOLUTIONS_COUNT ; _SOLUTIONS_COUNT=$(echo "$_SOLUTIONS" | wc -l)

    [[ $_SOLUTIONS_COUNT -gt 1 ]] && complete_fail_safely 'There is more than one solution, please specify which one should Stryker run against.'
    [[ $_SOLUTIONS_COUNT -lt 1 ]] && complete_fail_safely 'There are no solutions in this directory.'

    echo "$ROOT_PROJECT_DIRECTORY/$_SOLUTIONS"

    return 0
}

function get_source_project_relative_paths() {
    local _SOLUTION ; _SOLUTION=$1

    grep 'csproj' "$_SOLUTION" | cut -d , -f 2 | cut -d \" -f 2 | grep -v Tests | rev | cut -d \\ -f 2- | rev | sed 's/\\/\//g'

    return 0
}

function get_default_stryker_output_path () {
    local _SOURCE_PROJECT_PATH ; _SOURCE_PROJECT_PATH=$1

    local _STRYKER_OUTPUT ; _STRYKER_OUTPUT="$_SOURCE_PROJECT_PATH/StrykerOutput"

    echo "$_STRYKER_OUTPUT"

    return 0
}

function rm_default_stryker_output_directories() {
    local _SOURCE_PROJECT_RELATIVE_PATHS ; _SOURCE_PROJECT_RELATIVE_PATHS=$1

    local _SOURCE_PROJECT_RELATIVE_PATH
    for _SOURCE_PROJECT_RELATIVE_PATH in $_SOURCE_PROJECT_RELATIVE_PATHS; do
        local _SOURCE_PROJECT_PATH ; _SOURCE_PROJECT_PATH="$ROOT_PROJECT_DIRECTORY/$_SOURCE_PROJECT_RELATIVE_PATH"
        local _STRYKER_OUTPUT ; _STRYKER_OUTPUT=$(get_default_stryker_output_path "$_SOURCE_PROJECT_PATH")
        rm -rf "$_STRYKER_OUTPUT"
    done

    return 0
}

function new_stryker_output_directories() {
    local _STRYKER_RESULTS_OUTPUT ; _STRYKER_RESULTS_OUTPUT=$1
    local _STRYKER_REPORTS_OUTPUT ; _STRYKER_REPORTS_OUTPUT=$2

    mkdir -p "$_STRYKER_RESULTS_OUTPUT"
    mkdir -p "$_STRYKER_REPORTS_OUTPUT"

    return 0
}

function invoke_dotnet_tool_restore() {
    dotnet tool restore || complete_fail_safely

    return 0
}

function get_stryker_dashboard_reporter_command() {
    local _STRYKER_COMMAND ; _STRYKER_COMMAND="dotnet stryker -r dashboard --version $STRYKER_DASHBOARD_VERSION"

    if [[ $STRYKER_EXPERIMENTAL == 'true' ]]; then
        _STRYKER_COMMAND+=" --with-baseline:$STRYKER_DASHBOARD_BASELINE"
    fi

    echo "$_STRYKER_COMMAND"

    return 0
}

function get_stryker_json_reporter_command() {
    local _STRYKER_COMMAND ; _STRYKER_COMMAND='dotnet stryker -r json'

    echo "$_STRYKER_COMMAND"

    return 0
}

function invoke_dotnet_stryker() {
    local _SOLUTION ; _SOLUTION=$1
    local _STRYKER_COMMAND ; _STRYKER_COMMAND=$2

    local _SOURCE_PROJECT_RELATIVE_PATHS ; _SOURCE_PROJECT_RELATIVE_PATHS=$(get_source_project_relative_paths "$_SOLUTION")
    local _SOURCE_PROJECT_RELATIVE_PATH
    for _SOURCE_PROJECT_RELATIVE_PATH in $_SOURCE_PROJECT_RELATIVE_PATHS; do
        local _SOURCE_PROJECT_PATH ; _SOURCE_PROJECT_PATH="$ROOT_PROJECT_DIRECTORY/$_SOURCE_PROJECT_RELATIVE_PATH"

        cd "$_SOURCE_PROJECT_PATH"

        echo "Running: $_STRYKER_COMMAND"

        $_STRYKER_COMMAND || complete_fail_safely
    done

    return 0
}

function new_merged_stryker_json_report() {
    local _STRYKER_RESULTS_OUTPUT ; _STRYKER_RESULTS_OUTPUT=$1
    local _STRYKER_REPORTS_OUTPUT ; _STRYKER_REPORTS_OUTPUT=$2
    local _SOURCE_PROJECT_RELATIVE_PATHS ; _SOURCE_PROJECT_RELATIVE_PATHS=$3
    local _STRYKER_MERGED_REPORT ; _STRYKER_MERGED_REPORT=$4

    local _MUTATION_REPORT_JSON_FRAGMENT ; _MUTATION_REPORT_JSON_FRAGMENT="{\"schemaVersion\":\"1\",\"thresholds\":{\"high\":80,\"low\":60},\"projectRoot\":\"$ROOT_PROJECT_DIRECTORY\",\"files\":{"
    echo -n "$_MUTATION_REPORT_JSON_FRAGMENT" > "$_STRYKER_MERGED_REPORT"

    local _COUNT ; _COUNT=0
    local _SOURCE_PROJECT_RELATIVE_PATH
    for _SOURCE_PROJECT_RELATIVE_PATH in $_SOURCE_PROJECT_RELATIVE_PATHS; do
        local _SOURCE_PROJECT_PATH ; _SOURCE_PROJECT_PATH="$ROOT_PROJECT_DIRECTORY/$_SOURCE_PROJECT_RELATIVE_PATH"

        cd "$_SOURCE_PROJECT_PATH"

        local _PROJECT_NAME ; _PROJECT_NAME=$(echo "$_SOURCE_PROJECT_PATH" | rev | cut -d / -f 1 | rev)

        local _STRYKER_OUTPUT ; _STRYKER_OUTPUT=$(get_default_stryker_output_path "$_SOURCE_PROJECT_PATH")
        local _STRYKER_UNMERGED_RESULT_PATH ; _STRYKER_UNMERGED_RESULT_PATH=$(find "$_STRYKER_OUTPUT" -name '*.json')
        local _STRYKER_UNMERGED_RESULT_NEW_PATH ; _STRYKER_UNMERGED_RESULT_NEW_PATH="$_STRYKER_RESULTS_OUTPUT/$_PROJECT_NAME.stryker.json"

        mv "$_STRYKER_UNMERGED_RESULT_PATH" "$_STRYKER_UNMERGED_RESULT_NEW_PATH"

        [[ $_COUNT -gt 0 ]] && echo -n "," >> "$_STRYKER_MERGED_REPORT"

        local _GREP_COMMAND
        [[ "$OSTYPE" =~ darwin* ]] && _GREP_COMMAND='ggrep' || _GREP_COMMAND='grep'
        if $_GREP_COMMAND -oP '(?<=files":{).*(?=}}$)' "$_STRYKER_UNMERGED_RESULT_NEW_PATH" | tr -d '\n' >> "$_STRYKER_MERGED_REPORT"; then
            _COUNT=$((_COUNT + 1))
        else
            complete_fail_safely
        fi
    done
    echo '}}' >> "$_STRYKER_MERGED_REPORT"

    cd "$ROOT_PROJECT_DIRECTORY"

    return 0
}

function new_merged_stryker_html_report() {
    local _STRYKER_REPORTS_OUTPUT ; _STRYKER_REPORTS_OUTPUT=$1
    local _STRYKER_MERGED_REPORT ; _STRYKER_MERGED_REPORT=$2

    local _STRYKER_MUTATION_HTML_REPORT_FILE_URL ; _STRYKER_MUTATION_HTML_REPORT_FILE_URL='https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/Reporters/HtmlReporter/Files/mutation-report.html'
    local _STRYKER_MUTATION_HTML_REPORT ; _STRYKER_MUTATION_HTML_REPORT="$_STRYKER_REPORTS_OUTPUT/mutation-report.html"

    curl -s "$_STRYKER_MUTATION_HTML_REPORT_FILE_URL" --output "$_STRYKER_MUTATION_HTML_REPORT" > /dev/null

    local _SED_COMMAND
    [[ "$OSTYPE" =~ darwin* ]] \
        && _SED_COMMAND="gsed" \
        || _SED_COMMAND="sed"

    $_SED_COMMAND -i 's/>##REPORT_JS##/ defer src="https:\/\/www.unpkg.com\/mutation-testing-elements">/' "$_STRYKER_MUTATION_HTML_REPORT"
    $_SED_COMMAND -i 's/##REPORT_TITLE##/Stryker Mutation Testing/' "$_STRYKER_MUTATION_HTML_REPORT"
    echo -n "$(cat "$_STRYKER_MERGED_REPORT");" | $_SED_COMMAND -i -e '/##REPORT_JSON##/{r /dev/stdin' -e '}' "$_STRYKER_MUTATION_HTML_REPORT"
    $_SED_COMMAND -i 's/##REPORT_JSON##;//' "$_STRYKER_MUTATION_HTML_REPORT"

    echo "Stryker HTML Report: $_STRYKER_MUTATION_HTML_REPORT"

    open "$_STRYKER_MUTATION_HTML_REPORT"
}

SOLUTION=$(get_solution "$1")
[[ ! -r "$SOLUTION" ]] && complete_fail_safely "The $SOLUTION file is not readable or doesn't exist."

SOURCE_PROJECT_RELATIVE_PATHS=$(get_source_project_relative_paths "$SOLUTION")

rm_default_stryker_output_directories "$SOURCE_PROJECT_RELATIVE_PATHS"

TIMESTAMP=$(date +%s)

STRYKER_RESULTS_OUTPUT="$ROOT_PROJECT_DIRECTORY/stryker-results/$TIMESTAMP"
STRYKER_REPORTS_OUTPUT="$ROOT_PROJECT_DIRECTORY/stryker-reports/$TIMESTAMP"

new_stryker_output_directories "$STRYKER_RESULTS_OUTPUT" "$STRYKER_REPORTS_OUTPUT"

invoke_dotnet_tool_restore

[[ $RUNNING_FROM_PIPELINE == 'true' ]] \
    && invoke_dotnet_stryker "$SOLUTION" "$(get_stryker_dashboard_reporter_command)" \
    && exit 0

invoke_dotnet_stryker "$SOLUTION" "$(get_stryker_json_reporter_command)"

STRYKER_MERGED_REPORT_PATH="$STRYKER_REPORTS_OUTPUT/merged-mutation-report.json"
new_merged_stryker_json_report "$STRYKER_RESULTS_OUTPUT" "$STRYKER_REPORTS_OUTPUT" "$SOURCE_PROJECT_RELATIVE_PATHS" "$STRYKER_MERGED_REPORT_PATH"
new_merged_stryker_html_report "$STRYKER_REPORTS_OUTPUT" "$STRYKER_MERGED_REPORT_PATH"

rm_default_stryker_output_directories "$SOURCE_PROJECT_RELATIVE_PATHS"

cd "$STARTUP_DIRECTORY"

#!/bin/bash

set -eu
#set -x

# Цвета ANSI для вывода
RED='\033[1;31m'
GREEN='\033[1;32m'
YELLOW='\033[1;33m'
RESET='\033[0m'

# Функция для вывода подробного сравнения файлов
diff_with_details() {
    local expected_file=$1
    local actual_file=$2
    local prefix=$3

    local expected_clean=$(mktemp)
    local actual_clean=$(mktemp)

    # Убираем BOM и обрезаем пробелы справа
    sed '1s/^\xEF\xBB\xBF//' "$expected_file" | sed 's/[[:space:]]*$//' > "$expected_clean"
    sed '1s/^\xEF\xBB\xBF//' "$actual_file"   | sed 's/[[:space:]]*$//' > "$actual_clean"

    # Добавляем пустую строку в конце, если ее нет
    [[ $(tail -c1 "$expected_clean") != "" ]] && echo >> "$expected_clean"
    [[ $(tail -c1 "$actual_clean") != "" ]]   && echo >> "$actual_clean"

    # Гарантируем финальный перевод строки
    if [ -s "$expected_clean" ]; then
        grep -q '[^[:space:]]$' "$expected_clean" && echo >> "$expected_clean"
    fi
    if [ -s "$actual_clean" ]; then
        grep -q '[^[:space:]]$' "$actual_clean" && echo >> "$actual_clean"
    fi
    
    if diff -u "$expected_clean" "$actual_clean" > /tmp/diff_output.txt; then
        rm -f "$expected_clean" "$actual_clean"
        return 0
    else
        printf "${RED}${prefix}Ожидалось:${RESET}\n"
        sed -n '1,10p' "$expected_clean" | sed 's/^/  /'
        printf "${RED}${prefix}Получено:${RESET}\n"
        sed -n '1,10p' "$actual_clean" | sed 's/^/  /'
        printf "${RED}${prefix}Подробности различий (diff -u):${RESET}\n"
        sed 's/^/  /' /tmp/diff_output.txt
        rm -f "$expected_clean" "$actual_clean"
        return 1
    fi
}




# Проверка expected.files.txt
compare_expected_files() {
    local test_dir=$1
    local expected_files_txt="$test_dir/expected.files.txt"
    local failed=false

    [[ -f "$expected_files_txt" ]] || return 0

    local current_file=""
    local current_content=""
    local temp_expected=$(mktemp)
    local temp_actual=$(mktemp)

    while IFS= read -r line || [[ -n "$line" ]]; do
        if [[ -z "$current_file" ]]; then
            current_file="$line"
            current_content=""
        elif [[ "$line" == "===EOF===" ]]; then
            echo "$current_content" > "$temp_expected"
            if [[ ! -f "$current_file" ]]; then
                printf "${RED}Ожидаемый файл '$current_file' не найден${RESET}\n"
                failed=true
            else
                cp "$current_file" "$temp_actual"
                if ! diff_with_details "$temp_expected" "$temp_actual" "Файл '$current_file': "; then
                    failed=true
                fi
            fi
            current_file=""
            current_content=""
        else
            current_content="$current_content$line"$'\n'
        fi
    done < "$expected_files_txt"

    rm -f "$temp_expected" "$temp_actual"

    $failed && return 1 || return 0
}

# Запуск одного теста
run_test_case() {
    local test_dir=$1
    local run_cmd_file="$test_dir/run.txt"
    local run_before_file="$test_dir/run_before.txt"
    local run_after_file="$test_dir/run_after.txt"
    local input_file="$test_dir/input.txt"
    local expected_output_file="$test_dir/expected.output.txt"

    if [[ ! -f "$run_cmd_file" ]]; then
        printf "${YELLOW}Пропущен run.txt в $test_dir${RESET}\n"
        return 1
    fi

    # Запуск run_before.txt если есть
    if [[ -f "$run_before_file" ]]; then
        sh -c "$(cat "$run_before_file")"
        local before_status=$?
        if [[ $before_status -ne 0 ]]; then
            printf "${RED}Ошибка при выполнении run_before.txt в $test_dir${RESET}\n"
            return 1
        fi
    fi

    local run_cmd
    run_cmd=$(cat "$run_cmd_file")

    local output_file
    output_file=$(mktemp)

    # Определяем корень проекта (2_labyrinths)
    local root_dir
    root_dir=$(realpath "$test_dir/../../..")

    # Запуск команды из корня проекта
    pushd "$root_dir" > /dev/null
    if [[ -f "$input_file" ]]; then
        eval "$run_cmd" < "$input_file" > "$output_file" 2>&1
    else
        eval "$run_cmd" > "$output_file" 2>&1
    fi
    local cmd_status=$?
    popd > /dev/null

    local passed=true

    # Проверка вывода
    if [[ -f "$expected_output_file" ]]; then
        if ! diff -u <(sed 's/\r$//' "$expected_output_file") <(sed 's/\r$//' "$output_file") > /tmp/diff_output.txt; then
            printf "${RED}Вывод не совпадает с expected.output.txt:${RESET}\n"
            sed 's/^/  /' /tmp/diff_output.txt
            passed=false
        fi
    fi

    # Проверка файлов из expected.files.txt
    if ! compare_expected_files "$test_dir"; then
        passed=false
    fi

    if ! $passed; then
        printf "${YELLOW}--- Полный вывод команды в тесте $test_dir ---${RESET}\n"
        cat "$output_file"
        printf "${YELLOW}-------------------------------------------${RESET}\n"
    fi

    rm -f "$output_file"

    # Запуск run_after.txt если есть, только если тест прошел
    if $passed && [[ -f "$run_after_file" ]]; then
        sh -c "$(cat "$run_after_file")"
        local after_status=$?
        if [[ $after_status -ne 0 ]]; then
            printf "${RED}Ошибка при выполнении run_after.txt в $test_dir${RESET}\n"
            return 1
        fi
    fi

    return $([ "$passed" = true ] && echo 0 || echo 1)
}


# Запуск всех тестов
run_all_tests() {
    local base_dir=$1
    local passed=0
    local failed=0

    for test_dir in "$base_dir"/*/; do
        [ -d "$test_dir" ] || continue
        printf "Запускаем Тест $test_dir\n"
        if run_test_case "$test_dir"; then
            printf "${GREEN}Тест $test_dir: ПРОЙДЕН${RESET}\n"
            passed=$((passed + 1))
        else
            printf "${RED}Тест $test_dir: НЕ ПРОЙДЕН${RESET}\n"
            failed=$((failed + 1))
        fi
    done

    printf "\nИТОГИ ПРОВЕРКИ:\n"
    printf "${GREEN}Успешно: $passed${RESET}, ${RED}Неудачи: $failed${RESET}\n"

    [[ $failed -gt 0 ]] && exit 1 || exit 0
}

# Проверка параметров
[[ "$#" -ne 1 ]] && { printf "${YELLOW}Использование: %s <каталог_тестов>${RESET}\n" "$0"; exit 1; }

run_all_tests "$1"

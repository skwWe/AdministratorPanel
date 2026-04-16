#!/bin/bash
# collect_web_logs.sh

set -u

SSH_USER_NAME="${SSH_USER_NAME:-Kuznetsov.SS}"
SSH_PASS="${SSH_PASS:-}"
SUDO_PASS="${SUDO_PASS:-$SSH_PASS}"

KEY_PATH="$HOME/.ssh/id_ed25519"
PUB_KEY_PATH="${KEY_PATH}.pub"

TIMESTAMP=$(date +"%Y-%m-%d_%H-%M-%S")
BASE_LOG_DIR="./logs/web/$TIMESTAMP"
MAX_JOBS=4

SERVERS=(
	"10.10.130.173"
    "10.10.130.174"
    "10.10.130.169"
    "10.10.130.166"
    "10.10.130.165"
    "10.10.130.172"
    "10.10.130.247"
    "10.10.130.248"
    "10.10.130.249"
    "10.10.130.250"
    "10.10.130.251"
    "10.10.130.252"
    "10.10.130.253"
    "10.10.130.254"
)

SSH_CHECK_OPTS=(
    -o ConnectTimeout=5
    -o BatchMode=yes
    -o StrictHostKeyChecking=accept-new
)

SSH_OPTS=(
    -o ConnectTimeout=10
    -o StrictHostKeyChecking=accept-new
)

mkdir -p "$BASE_LOG_DIR"

print_header() {
    echo "=============================================="
    echo "Настройка SSH-ключей и сбор логов Docker"
    echo "Старт: $TIMESTAMP"
    echo "Серверов: ${#SERVERS[@]}"
    echo "Параллельных задач: $MAX_JOBS"
    echo "Пользователь SSH: $SSH_USER_NAME"
    echo "=============================================="
}

ensure_ssh_key() {
    if [ -f "$PUB_KEY_PATH" ]; then
        echo "✅ SSH-ключ уже существует: $PUB_KEY_PATH"
        return 0
    fi

    echo "🔑 SSH-ключ не найден, создаю..."
    ssh-keygen -t ed25519 -N "" -f "$KEY_PATH"
    local status=$?

    if [ $status -ne 0 ]; then
        echo "❌ Не удалось создать SSH-ключ"
        exit 1
    fi

    echo "✅ SSH-ключ создан: $PUB_KEY_PATH"
}

ensure_sshpass() {
    if command -v sshpass >/dev/null 2>&1; then
        return 0
    fi

    echo "❌ Не найден sshpass. Установи его в WSL: sudo apt update && sudo apt install -y sshpass"
    exit 2
}

setup_key_for_server() {
    local server="$1"

    echo
    echo "-----------------------------------------"
    echo "Проверка SSH-ключа: $SSH_USER_NAME@$server"
    echo "-----------------------------------------"

    ssh "${SSH_CHECK_OPTS[@]}" "$SSH_USER_NAME@$server" "echo OK" >/dev/null 2>&1
    local status=$?

    if [ $status -eq 0 ]; then
        echo "✅ Доступ по ключу уже настроен"
        return 0
    fi

    if [ -z "$SSH_PASS" ]; then
        echo "❌ SSH_PASS не передан, ключ автоматически установить нельзя"
        return 1
    fi

    echo "🔐 Доступа по ключу нет, запускаю ssh-copy-id через sshpass"

    sshpass -p "$SSH_PASS" ssh-copy-id \
        -o ConnectTimeout=10 \
        -o StrictHostKeyChecking=accept-new \
        "$SSH_USER_NAME@$server"

    status=$?

    if [ $status -eq 0 ]; then
        echo "✅ Ключ установлен"
    else
        echo "❌ Не удалось установить ключ на $server"
    fi

    return $status
}

collect_one_server() {
    local server="$1"
    local server_dir="$BASE_LOG_DIR/$server"
    local tmp_archive="$server_dir/_bundle.tar.gz"
    local tmp_stderr="$server_dir/_ssh_stderr.txt"
    local tmp_status="$server_dir/_status.txt"

    mkdir -p "$server_dir"

    ssh "${SSH_OPTS[@]}" "$SSH_USER_NAME@$server" 'bash --noprofile --norc -s' > "$tmp_archive" 2> "$tmp_stderr" <<EOF
set -u

PATH=/usr/local/bin:/usr/bin:/bin:/usr/local/sbin:/usr/sbin:/sbin

WORKDIR=\$(mktemp -d)
trap 'rm -rf "\$WORKDIR"' EXIT

HOSTNAME_VALUE=\$(hostname 2>/dev/null || echo unknown)
DATE_VALUE=\$(date 2>/dev/null || echo unknown)

cat > "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF
Hostname: \$HOSTNAME_VALUE
Date: \$DATE_VALUE
User: \$(id 2>/dev/null || echo unknown)
PATH: \$PATH
Docker path: \$(command -v docker 2>/dev/null || echo 'docker not found')
Groups: \$(id -nG 2>/dev/null || echo unknown)
DEBUG_EOF

DOCKER_CHECK_OUTPUT=\$(docker ps 2>&1)
DOCKER_CHECK_STATUS=\$?

cat >> "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF

[docker ps status]
\$DOCKER_CHECK_STATUS

[docker ps output]
\$DOCKER_CHECK_OUTPUT
DEBUG_EOF

if [ \$DOCKER_CHECK_STATUS -ne 0 ]; then
    if echo "\$DOCKER_CHECK_OUTPUT" | grep -qi "permission denied while trying to connect to the Docker daemon socket"; then
        cat >> "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF

[action]
Trying to add user to docker group
DEBUG_EOF

        if [ -z "$SUDO_PASS" ]; then
            cat > "\$WORKDIR/DOCKER_ERROR.txt" <<ERR_EOF
Docker не доступен
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE

Не передан SUDO_PASS для исправления прав доступа к Docker.
ERR_EOF
            tar -czf - -C "\$WORKDIR" .
            exit 20
        fi

        USERMOD_OUTPUT=\$(printf '%s\n' "$SUDO_PASS" | sudo -S -p '' usermod -aG docker '$SSH_USER_NAME' 2>&1)
        USERMOD_STATUS=\$?

        cat >> "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF

[usermod status]
\$USERMOD_STATUS

[usermod output]
\$USERMOD_OUTPUT
DEBUG_EOF

        if [ \$USERMOD_STATUS -ne 0 ]; then
            cat > "\$WORKDIR/DOCKER_ERROR.txt" <<ERR_EOF
Docker не доступен
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE

Не удалось добавить пользователя в группу docker.

Ошибка docker ps:
\$DOCKER_CHECK_OUTPUT

Ошибка usermod:
\$USERMOD_OUTPUT
ERR_EOF
            tar -czf - -C "\$WORKDIR" .
            exit 20
        fi

        SG_CHECK_OUTPUT=\$(sg docker -c "docker ps" 2>&1)
        SG_CHECK_STATUS=\$?

        cat >> "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF

[sg docker -c docker ps status]
\$SG_CHECK_STATUS

[sg docker -c docker ps output]
\$SG_CHECK_OUTPUT
DEBUG_EOF

        if [ \$SG_CHECK_STATUS -ne 0 ]; then
            cat > "\$WORKDIR/DOCKER_ERROR.txt" <<ERR_EOF
Docker не доступен
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE

После добавления в группу docker доступ не появился.

Ошибка:
\$SG_CHECK_OUTPUT
ERR_EOF
            tar -czf - -C "\$WORKDIR" .
            exit 21
        fi

        CONTAINERS_OUTPUT=\$(sg docker -c "docker ps --format '{{.Names}}'" 2>&1)
        CONTAINERS_STATUS=\$?

        cat >> "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF

[sg docker -c docker ps --format status]
\$CONTAINERS_STATUS

[sg docker -c docker ps --format output]
\$CONTAINERS_OUTPUT
DEBUG_EOF

        if [ \$CONTAINERS_STATUS -ne 0 ]; then
            cat > "\$WORKDIR/DOCKER_ERROR.txt" <<ERR_EOF
Не удалось получить список контейнеров
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE

Ошибка:
\$CONTAINERS_OUTPUT
ERR_EOF
            tar -czf - -C "\$WORKDIR" .
            exit 22
        fi

        CONTAINERS="\$CONTAINERS_OUTPUT"

        if [ -z "\$CONTAINERS" ]; then
            cat > "\$WORKDIR/NO_CONTAINERS.txt" <<ERR_EOF
Нет запущенных контейнеров
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE
ERR_EOF
            tar -czf - -C "\$WORKDIR" .
            exit 30
        fi

        cat > "\$WORKDIR/INFO.txt" <<INFO_EOF
Сервер: \$HOSTNAME_VALUE
Дата сбора: \$DATE_VALUE

Контейнеры:
\$CONTAINERS
INFO_EOF

        : > "\$WORKDIR/all.log"

        for C in \$CONTAINERS; do
            {
                echo "========== \$C =========="
                sg docker -c "docker logs \$C" 2>&1 || true
                echo
            } >> "\$WORKDIR/all.log"

            {
                echo
                echo "[container: \$C]"
                sg docker -c "docker inspect --format '{{.State.Status}}' \$C" 2>/dev/null || echo "inspect failed"
            } >> "\$WORKDIR/DEBUG.txt"
        done

        tar -czf - -C "\$WORKDIR" .
        exit 0
    else
        cat > "\$WORKDIR/DOCKER_ERROR.txt" <<ERR_EOF
Docker не доступен
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE

Ошибка docker ps:
\$DOCKER_CHECK_OUTPUT
ERR_EOF
        tar -czf - -C "\$WORKDIR" .
        exit 20
    fi
fi

CONTAINERS_OUTPUT=\$(docker ps --format '{{.Names}}' 2>&1)
CONTAINERS_STATUS=\$?

cat >> "\$WORKDIR/DEBUG.txt" <<DEBUG_EOF

[docker ps --format status]
\$CONTAINERS_STATUS

[docker ps --format output]
\$CONTAINERS_OUTPUT
DEBUG_EOF

if [ \$CONTAINERS_STATUS -ne 0 ]; then
    cat > "\$WORKDIR/DOCKER_ERROR.txt" <<ERR_EOF
Не удалось получить список контейнеров
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE

Ошибка:
\$CONTAINERS_OUTPUT
ERR_EOF
    tar -czf - -C "\$WORKDIR" .
    exit 22
fi

CONTAINERS="\$CONTAINERS_OUTPUT"

if [ -z "\$CONTAINERS" ]; then
    cat > "\$WORKDIR/NO_CONTAINERS.txt" <<ERR_EOF
Нет запущенных контейнеров
Hostname: \$HOSTNAME_VALUE
Дата: \$DATE_VALUE
ERR_EOF
    tar -czf - -C "\$WORKDIR" .
    exit 30
fi

cat > "\$WORKDIR/INFO.txt" <<INFO_EOF
Сервер: \$HOSTNAME_VALUE
Дата сбора: \$DATE_VALUE

Контейнеры:
\$CONTAINERS
INFO_EOF

: > "\$WORKDIR/all.log"

for C in \$CONTAINERS; do
    {
        echo "========== \$C =========="
        docker logs "\$C" 2>&1 || true
        echo
    } >> "\$WORKDIR/all.log"

    {
        echo
        echo "[container: \$C]"
        docker inspect --format '{{.State.Status}}' "\$C" 2>/dev/null || echo "inspect failed"
    } >> "\$WORKDIR/DEBUG.txt"
done

tar -czf - -C "\$WORKDIR" .
EOF

    local ssh_status=$?

    if [ $ssh_status -eq 255 ]; then
        cp "$tmp_stderr" "$server_dir/SSH_ERROR.txt" 2>/dev/null
        rm -f "$tmp_archive"
        echo "SSH_ERROR" > "$tmp_status"
        return
    fi

    if [ ! -s "$tmp_archive" ]; then
        cp "$tmp_stderr" "$server_dir/SSH_ERROR.txt" 2>/dev/null
        echo "SSH_ERROR" > "$tmp_status"
        return
    fi

    tar -xzf "$tmp_archive" -C "$server_dir" >/dev/null 2>&1
    local tar_status=$?
    rm -f "$tmp_archive" "$tmp_stderr"

    if [ $tar_status -ne 0 ]; then
        echo "Ошибка распаковки архива" > "$server_dir/SSH_ERROR.txt"
        echo "SSH_ERROR" > "$tmp_status"
        return
    fi

    if [ -f "$server_dir/DOCKER_ERROR.txt" ]; then
        echo "NO_DOCKER" > "$tmp_status"
        return
    fi

    if [ -f "$server_dir/NO_CONTAINERS.txt" ]; then
        echo "NO_CONTAINERS" > "$tmp_status"
        return
    fi

    echo "SUCCESS" > "$tmp_status"
}

wait_for_slot() {
    while true; do
        local running
        running=$(jobs -rp | wc -l | tr -d ' ')
        if [ "$running" -lt "$MAX_JOBS" ]; then
            break
        fi
        sleep 0.5
    done
}

summarize_results() {
    local total=0
    local success=0
    local no_docker=0
    local no_containers=0
    local ssh_errors=0

    echo
    echo "=============================================="
    echo "ГОТОВО"
    echo "=============================================="

    for server in "${SERVERS[@]}"; do
        total=$((total + 1))
        local status_file="$BASE_LOG_DIR/$server/_status.txt"
        local status="UNKNOWN"

        if [ -f "$status_file" ]; then
            status=$(cat "$status_file")
        fi

        case "$status" in
            SUCCESS) success=$((success + 1)) ;;
            NO_DOCKER) no_docker=$((no_docker + 1)) ;;
            NO_CONTAINERS) no_containers=$((no_containers + 1)) ;;
            SSH_ERROR|UNKNOWN) ssh_errors=$((ssh_errors + 1)) ;;
        esac
    done

    echo "Всего серверов: $total"
    echo "✅ Успешно: $success"
    echo "⚠️ Нет Docker: $no_docker"
    echo "⚠️ Нет контейнеров: $no_containers"
    echo "❌ Ошибки SSH/прочее: $ssh_errors"
    echo "Логи: $BASE_LOG_DIR"
    echo "=============================================="

    echo
    echo "Кратко по серверам:"
    for server in "${SERVERS[@]}"; do
        local status_file="$BASE_LOG_DIR/$server/_status.txt"
        local status="UNKNOWN"
        [ -f "$status_file" ] && status=$(cat "$status_file")
        echo " - $server : $status"
    done
}

print_header
ensure_sshpass
ensure_ssh_key

echo
echo "=============================================="
echo "ЭТАП 1: Настройка SSH-ключей"
echo "=============================================="

for server in "${SERVERS[@]}"; do
    setup_key_for_server "$server"
done

echo
echo "=============================================="
echo "ЭТАП 2: Параллельный сбор логов"
echo "=============================================="

for server in "${SERVERS[@]}"; do
    wait_for_slot
    collect_one_server "$server" &
done

wait
summarize_results
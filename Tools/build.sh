#!/usr/bin/env bash
# AnyMMO 데모를 굽는다 — qa: control_cli SDK 를 넣은 Android APK(넣은 흔적은 끝나면 되돌림) · server: Linux 헤드리스 게임 서버
set -euo pipefail

UNITY=${UNITY:-/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity}
CONTROL_CLI=${CONTROL_CLI:-$HOME/Projects/control_cli}
CONTROL_CLI_REF=${CONTROL_CLI_REF:-v0.7.0}
ROOT=$(cd "$(dirname "$0")/.." && pwd)
mkdir -p "$ROOT/Builds"

unity() { "$UNITY" -batchmode -quit -nographics -projectPath "$ROOT" -buildTarget "$1" -executeMethod "$2" -logFile "$ROOT/Builds/$3.log"; }

# control_cli 규칙: 게임 저장소에는 SDK 를 남기지 않는다 — instrument 가 고친 manifest · asmdef 를 빌드 뒤 되돌린다.
restore() {
  git -C "$ROOT" checkout -- Packages
  local changed
  changed=$(git -C "$ROOT" diff --name-only -- '*.asmdef')
  [ -z "$changed" ] || (cd "$ROOT" && echo "$changed" | xargs git checkout --)
}

case "${1:-}" in
  qa)
    node "$CONTROL_CLI/dist/main.js" instrument "$ROOT" --ref "$CONTROL_CLI_REF"
    trap restore EXIT
    unity Android ShowcaseBuild.AndroidQa android
    ls -lh "$ROOT/Builds/android/anymmo-qa.apk"
    ;;
  server)
    unity Linux64 ShowcaseBuild.LinuxServer linux-server
    ls -lh "$ROOT/Builds/linux-server/AnyMMOServer.x86_64"
    ;;
  *)
    echo "쓰는 법: Tools/build.sh qa|server" >&2
    exit 2
    ;;
esac

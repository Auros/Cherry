#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
default_target_version="1.42.0"
target_version="$default_target_version"
deploy=false

usage() {
  echo "usage: ./build.sh [--target-version VERSION] [--ssh-deploy]" >&2
  echo "       ./build.sh [-v VERSION] [--ssh-deploy]" >&2
  exit 1
}

while (( $# )); do
  case "$1" in
    --target-version|--version|-v)
      (( $# >= 2 )) || usage
      target_version="$2"
      shift 2
      ;;
    --ssh-deploy)
      deploy=true
      shift
      ;;
    1.29.0|1.37.1|1.38.0|1.40.0|1.42.0)
      target_version="$1"
      shift
      ;;
    *)
      usage
      ;;
  esac
done

case "$target_version" in
  1.29.0|1.37.1|1.38.0|1.40.0|1.42.0) ;;
  *)
    echo "Unsupported target version '$target_version'." >&2
    usage
    ;;
esac

prop() { sed -nE "s:.*<$1[^>]*>(.*)</$1>.*:\1:p" "$root/Directory.Build.local.props" 2>/dev/null | tail -n 1; }
refs_root="$(prop CherryRefsRoot)"
[[ -n "$refs_root" ]] || refs_root="$root/refs"
[[ -d "$refs_root" ]] || refs_root="$root/../.refs"
refs="$refs_root/$target_version"
if [[ ! -d "$refs/Beat Saber_Data/Managed" && "$target_version" == "$default_target_version" && -d "$refs_root/Beat Saber_Data/Managed" ]]; then
  refs="$refs_root"
fi
catcore_dir="$(prop CherryCatCoreDir)"
output_dll="$root/Cherry/bin/$target_version/Release/Cherry.dll"
output_pdb="${output_dll%.dll}.pdb"
output_dir="$(dirname "$output_dll")"

[[ -d "$refs/Beat Saber_Data/Managed" && -d "$refs/Plugins" && -d "$refs/Libs" ]] || {
  echo "Refs for $target_version are incomplete at $refs; expected Beat Saber_Data/Managed, Plugins and Libs." >&2
  exit 1
}
[[ -f "$refs/Libs/CatCore.dll" || -f "$catcore_dir/CatCore.dll" ]] || {
  echo "CatCore.dll is missing from $refs/Libs and CherryCatCoreDir." >&2
  exit 1
}

rm -rf "$output_dir/Artifact" "$output_dir/zip"

dotnet build "$root/Cherry.sln" -c Release \
  -p:Platform="BS$target_version" \
  -p:LocalRefsDir="$refs" \
  -p:CherryCatCoreDir="$catcore_dir" \
  -p:DisableCopyToGame=True

$deploy || exit 0

version_prop_suffix="${target_version//./_}"
target="$(prop "CherrySshTarget$version_prop_suffix")"
target="${target:-$(prop CherrySshTarget)}"
game="$(prop "CherrySshBeatSaberDir$version_prop_suffix")"
game="${game:-$(prop CherrySshBeatSaberDir)}"
[[ -n "$target" && -n "$game" ]] || {
  echo "Directory.Build.local.props must set CherrySshTarget and CherrySshBeatSaberDir, or version-specific variants for $target_version." >&2
  exit 1
}

ps_encode() { printf "%s" "$1" | iconv -f UTF-8 -t UTF-16LE | base64 | tr -d '\n'; }
deploy_file() {
  local source="$1"
  local destination="$2"
  local staging="Cherry.deploy.$RANDOM"
  local remote_dir="${destination%\\*}"
  [[ -f "$source" ]] || return 0
  scp "$source" "$target:$staging"
  local command="\$ErrorActionPreference = 'Stop'; \$ProgressPreference = 'SilentlyContinue'; New-Item -ItemType Directory -Force -Path '$remote_dir' | Out-Null; Copy-Item -Force -Path (Join-Path \$env:USERPROFILE '$staging') -Destination '$destination'; Remove-Item -Force -Path (Join-Path \$env:USERPROFILE '$staging'); [void]0"
  ssh "$target" powershell -NoProfile -NonInteractive -EncodedCommand "$(ps_encode "$command")"
}

deploy_file "$output_dll" "$game\Plugins\Cherry.dll"
deploy_file "$output_pdb" "$game\Plugins\Cherry.pdb"

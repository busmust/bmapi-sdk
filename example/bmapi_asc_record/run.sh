#!/bin/sh
set -eu
cd "$(dirname "$0")"
bits="$(getconf LONG_BIT)"
case "$(uname -m)" in
    arm*|aarch64) if [ "$bits" = 64 ]; then platform=raspbian64; else platform=linaro; fi ;;
    i386|i486|i586|i686|x86_64|amd64) platform="unix$bits" ;;
    *) echo "Unsupported machine" >&2; exit 2 ;;
esac
libdir="../../bin/$platform/release"
export LD_LIBRARY_PATH="$libdir${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
exec ./bmapi_asc_record "$@"

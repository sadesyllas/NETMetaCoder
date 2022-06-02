#!/bin/bash

pushd "$(dirname "$0")"

PLATFORM="linux-x64"

. ./publish.sh

popd

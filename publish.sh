#!/bin/bash

CONFIGURATION="Debug"
PUBLISH_PARENT_PATH="NETMetaCoder/bin/${CONFIGURATION}/net6.0/${PLATFORM}"
PUBLISH_PATH="${PUBLISH_PARENT_PATH}/publish"

rm -rf "${PUBLISH_PARENT_PATH}"

dotnet publish --force --sc -r ${PLATFORM} -c ${CONFIGURATION} \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -p:DebugType=embedded \
  NETMetaCoder/NETMetaCoder.csproj

DIST_PATH="./dist/${CONFIGURATION}/${PLATFORM}"

rm -rf "${DIST_PATH}"

mkdir -p "${DIST_PATH}"

mv "${PUBLISH_PATH}" "${DIST_PATH}/NETMetaCoder"

7z a "${DIST_PATH}/NETMetaCoder.zip" "${DIST_PATH}/NETMetaCoder"

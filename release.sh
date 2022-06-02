#!/bin/bash

if [ -z "${NMC_S}" ]; then
    echo "The package source repository will be defaulted to https://api.nuget.org/v3/index.json."
    
    NMC_S="https://api.nuget.org/v3/index.json"
fi

if [ -z "${NMC_K}" ]; then
    echo "The API key for the package source repository must be provided through the NMC_K environment variable." >&2

    exit 1
fi

FAILED_TO_PUSH=()

pushd "$(dirname "$0")"

for p in NETMetaCoder.Abstractions NETMetaCoder.SyntaxWrappers NETMetaCoder.MSBuild; do
    echo "Pushing package ${p}."
    pushd "${p}"
    dotnet clean
    rm -rf obj bin
    dotnet build --force -c Release || exit 1
    dotnet pack --force -c Release || exit 1
    dotnet nuget push bin/Release/*.nupkg -s "${NMC_S}" -k "${NMC_K}" -n --skip-duplicate || FAILED_TO_PUSH+=("${p}")
    popd
done

popd

if [ ${#FAILED_TO_PUSH[@]} -gt 0 ]; then
    echo "Failed to push package(s):"

    for package in "${FAILED_TO_PUSH[@]}"; do
        echo -e "\t- ${package}"
    done
fi

#!/bin/sh
find ./src/services -name '*.csproj' -exec dotnet sln add {} \;
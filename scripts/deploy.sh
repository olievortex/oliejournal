#!/bin/sh
basePath=~/source/repos/oliejournal/src
baseApi=${basePath}/oliejournal.api
baseCli=${basePath}/oliejournal.cli
apiPub=/opt/oliejournal.api
cliPath=/opt/oliejournal.cli
sourcePath=~/oliejournal/source.sh
logPath=/var/log/oliejournal
set -e

# Pull code
echo oliejournal- pull latest
cd ~/source/repos/oliejournal
git pull

# Sanity checks
if [ ! -d ${logPath} ]; then
    echo "The log directory ${logPath} doesn't exist"
    exit 1
fi
if [ ! -d ${apiPub} ]; then
    echo "The api publish directory ${apiPub} doesn't exist"
    exit 1
fi
if [ ! -d ${cliPub} ]; then
    echo "The cli publish directory ${cliPub} doesn't exist"
    exit 1
fi
if [ ! -x ${sourcePath} ]; then
    echo "The sourcing file ${sourcePath} doesn't exist"
    exit 1
fi

echo oliejournal - dotnet clean
cd ${basePath}
dotnet clean
rm -rf ${baseApi}/bin
rm -rf ${baseApi}/obj
rm -rf ${baseCli}/bin
rm -rf ${baseCli}/obj

echo oliejournal - dotnet build
dotnet build --configuration Release

echo oliejournal - dotnet test
dotnet test --configuration Release --no-restore --no-build

echo oliejournal.api - dotnet publish
dotnet publish ${baseApi}/oliejournal.api.csproj --configuration Release --no-restore --no-build

echo oliejournal.cli - dotnet publish
dotnet publish ${baseCli}/oliejournal.cli.csproj --configuration Release --no-restore --no-build

echo oliejournal.api - stop website
~/oliejournal/stop_api.sh

echo oliejournal.api - deploy
cd ${baseApi}/bin/Release/net10.0/publish
tar -cf ../publish.tar *
cd ${apiPub}
rm -rf *
tar -xf ${baseApi}/bin/Release/net10.0/publish.tar

echo oliejournal.api - start website
~/oliejournal/start_api.sh

echo oliejournal.cli - deploy
cd ${baseCli}/bin/Release/net10.0/publish
tar -cf ../publish.tar *
cd ${cliPub}
rm -rf *
tar -xf ${baseCli}/bin/Release/net10.0/publish.tar

echo Websites are starting
sleep 5

echo
echo List of dotnet processes
ps -eaf | grep dotnet

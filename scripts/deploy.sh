#!/bin/sh
basePath=~/source/repos/oliejournal/src
baseApi=${basePath}/oliejournal.api
baseCli=${basePath}/oliejournal.cli
baseWeb=${basePath}/oliejournal.web
apiPub=/opt/oliejournal.api
cliPub=/opt/oliejournal.cli
webPub=/opt/oliejournal.web
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
if [ ! -d ${webPub} ]; then
    echo "The web publish directory ${webPub} doesn't exist"
    exit 1
fi

echo
echo oliejournal.cli audiprocessqueue - stop process
~/oliejournal/stop_audioprocessqueue.sh

echo oliejournal - dotnet clean
cd ${basePath}
dotnet clean
rm -rf ${baseApi}/bin
rm -rf ${baseApi}/obj
rm -rf ${baseCli}/bin
rm -rf ${baseCli}/obj
rm -rf ${baseWeb}/bin
rm -rf ${baseWeb}/obj

echo oliejournal - dotnet build
dotnet build --configuration Release

echo oliejournal - dotnet test
dotnet test --configuration Release --no-restore --no-build

echo oliejournal.api - dotnet publish
dotnet publish ${baseApi}/oliejournal.api.csproj --configuration Release --no-restore --no-build

echo oliejournal.cli - dotnet publish
dotnet publish ${baseCli}/oliejournal.cli.csproj --configuration Release --no-restore --no-build

echo oliejournal.web - dotnet publish
dotnet publish ${baseWeb}/oliejournal.web.csproj --configuration Release --no-restore --no-build

echo
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

echo
echo oliejournal.web - stop website
~/oliejournal/stop_web.sh

echo oliejournal.web - deploy
cd ${baseWeb}/bin/Release/net10.0/publish
tar -cf ../publish.tar *
cd ${webPub}
rm -rf *
tar -xf ${baseWeb}/bin/Release/net10.0/publish.tar

echo oliejournal.web - start website
~/oliejournal/start_web.sh

echo
echo oliejournal.cli - deploy
cd ${baseCli}/bin/Release/net10.0/publish
tar -cf ../publish.tar *
cd ${cliPub}
rm -rf *
tar -xf ${baseCli}/bin/Release/net10.0/publish.tar

echo
echo oliejournal.cli audiprocessqueue - start process
~/oliejournal/start_audioprocessqueue.sh

echo
echo Wait to see if websites start - 20s
sleep 5
echo Wait to see if websites start - 15s
sleep 5
echo Wait to see if websites start - 10s
sleep 5
echo Wait to see if websites start - 5s
sleep 5

echo
echo List of dotnet processes
ps -eaf | grep dotnet

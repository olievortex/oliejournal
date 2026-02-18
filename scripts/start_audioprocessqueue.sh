#!/bin/sh
. ~/oliejournal/source.sh
cd /opt/oliejournal.cli
nohup dotnet oliejournal.cli.dll audioprocessqueue >> /var/log/oliejournal/oliejournal.cli.log 2>&1 &

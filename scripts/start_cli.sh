#!/bin/sh
. ~/oliejournal/source.sh
cd /opt/oliejournal.cli
dotnet oliejournal.cli.dll $1 $2 $3 >> /var/log/oliejournal/oliejournal.cli.log 2>&1

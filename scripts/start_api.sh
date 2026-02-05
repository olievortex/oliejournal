#!/bin/sh
. ~/oliejournal/source.sh
cd /opt/oliejournal.api
nohup dotnet oliejournal.api.dll --urls=http://localhost:7021 $1 >> /var/log/oliejournal/oliejournal.api.log 2>&1 &

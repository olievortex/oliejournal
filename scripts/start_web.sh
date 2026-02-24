#!/bin/sh
. ~/oliejournal/source.sh
cd /opt/oliejournal.web
nohup dotnet oliejournal.web.dll --urls=http://localhost:7022 $1 >> /var/log/oliejournal/oliejournal.web.log 2>&1 &

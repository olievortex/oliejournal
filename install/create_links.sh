#!/bin/sh
mkdir -p ~/oliejournal
cd ~/oliejournal
chmod go-rx .
ln --symbolic --force ~/source/repos/oliejournal/scripts/deploy/deploy.sh
ln --symbolic --force ~/source/repos/oliejournal/scripts/start/start_api.sh
ln --symbolic --force ~/source/repos/oliejournal/scripts/start/stop_api.sh

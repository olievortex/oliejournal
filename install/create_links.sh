#!/bin/sh
mkdir -p ~/oliejournal
cd ~/oliejournal
chmod go-rx .
ln --symbolic --force ~/source/repos/oliejournal/scripts/deploy.sh
ln --symbolic --force ~/source/repos/oliejournal/scripts/start_api.sh
ln --symbolic --force ~/source/repos/oliejournal/scripts/start_cli.sh
ln --symbolic --force ~/source/repos/oliejournal/scripts/stop_api.sh

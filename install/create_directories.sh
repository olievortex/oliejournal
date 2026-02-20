#!/bin/sh
mkdir -p /var/log/oliejournal
chown olievortex:olievortex /var/log/oliejournal

mkdir -p /var/backup
chown olievortex:olievortex /var/backup

mkdir -p /opt/oliejournal.api
chown olievortex:olievortex /opt/oliejournal.api
mkdir -p /opt/oliejournal.cli
chown olievortex:olievortex /opt/oliejournal.cli
mkdir -p /var/www/videos
chown olievortex:olievortex /var/www/videos

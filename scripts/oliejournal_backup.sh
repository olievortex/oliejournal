#!/bin/sh
timestamp=$(date +%Y%m%d_%H%M%S)
mkdir -p /var/backup/mysql
mysqldump --host=**** --port=**** --single-transaction --no-tablespaces --user=oliejournal_user --password=******** --set-gtid-purged=off oliejournal > /var/backup/mysql/${timestamp}_oliejournal.sql

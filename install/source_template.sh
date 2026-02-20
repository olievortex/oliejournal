#!/bin/sh
#### Ensure the connection string is wrapped within single quotes
export APPLICATIONINSIGHTS_CONNECTION_STRING='__applicationinsights_connection_string__'
export GOOGLE_APPLICATION_CREDENTIALS=/home/olievortex/oliejournal/xxxx.json
#### On Linux, make sure to remove any brackets around the IPv6 address, otherwise it will not resolve
#### Ensure the connection string is wrapped within single quotes
export OlieMySqlBackupContainer=https://xxxx.blob.core.windows.net/mysql-backups
export OlieMySqlBackupPath=/var/backup/mysql
export OlieMySqlConnection='server=2600:3c00::2000:xxxx;port=12345;uid=oliejournal_user;pwd=xxxx;database=oliejournal;SslMode=Required;'
export OlieAudioProcessQueue=oliejournal_audio_process
export OlieServiceBus=xxxx.servicebus.windows.net
export OlieBlobContainerUri=https://xxxx.blob.core.windows.net/oliejournal
#### The following settings are to create tokens from the ar-olieblind Application Registration.
#### Open the ar-olieblind Application Registration in the Azure portal to get the following values.
#### Replace __tenent_id__ with the "Directory (tenent) Id" from the portal
#### Replace __application_id__ with the "Application (client) Id" from the portal
#### To create a secret, navigate to Manage -> Certificates & secrets -> Client secrets
####   Click "New Client Secret". The value replaces __secret__ below.
####   Once you leave this page, you can never see the secret again.
export AZURE_TENANT_ID=xxxx
export AZURE_CLIENT_ID=xxxx
export AZURE_CLIENT_SECRET=xxxx

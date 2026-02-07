#!/bin/sh
#### Ensure the connection string is wrapped within single quotes
export APPLICATIONINSIGHTS_CONNECTION_STRING='__applicationinsights_connection_string__'
export GOOGLE_APPLICATION_CREDENTIALS=/home/olievortex/oliejournal/virtualstormchasing-de884bb5018e.json
#### On Linux, make sure to remove any brackets around the IPv6 address, otherwise it will not resolve
#### Ensure the connection string is wrapped within single quotes
export OlieMySqlConnection='server=2600:3c00::2000:xxxx;port=12345;uid=oliejournal_user;pwd=xxxx;database=oliejournal;SslMode=Required;'

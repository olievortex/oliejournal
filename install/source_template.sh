#!/bin/sh
#### Make sure Application Insights connection string is contained within single quotes
export APPLICATIONINSIGHTS_CONNECTION_STRING='__applicationinsights_connection_string__'
export GOOGLE_APPLICATION_CREDENTIALS=/home/olievortex/oliejournal/virtualstormchasing-de884bb5018e.json
#### On Linux, make sure to remove any brackets around the IPv6 address, otherwise it will not resolve
export OlieMySqlConnection=server=2600:3c00::2000:xxxx;port=12345;uid=oliejournal_user;pwd=xxxx;database=oliejournal;SslMode=Required;

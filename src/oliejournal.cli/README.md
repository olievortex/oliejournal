The following evironment variables are required to run locally. Replace the dummy values and put these settings in your User Secrets file.

{
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "xxxx",
  "OlieAudioProcessQueue": "oliejournal_audio_process",
  "OlieBlobContainerUri": "https://xxxx.blob.core.windows.net/oliejournal",
  "OlieMySqlConnection": "server=[2600:3c00::2000:xxxx];port=20237;uid=oliejournal_user;pwd=xxxx;database=oliejournal;SslMode=Required;",
  "OlieServiceBus": "xxxx.servicebus.windows.net"
}
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieService : IOlieService
{
    #region Stream

    public async Task<byte[]> StreamToByteArray(Stream stream, CancellationToken ct)
    {
        using var result = new MemoryStream();
        await stream.CopyToAsync(result, ct);

        return result.ToArray();
    }

    #endregion

    #region ServiceBus

    public async Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct)
    {
        var json = JsonConvert.SerializeObject(data);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(message, ct);
    }

    #endregion
}

using Azure.Messaging.ServiceBus;

namespace oliejournal.lib.Services;

public interface IOlieService
{
    #region Stream

    Task<byte[]> StreamToByteArray(Stream stream, CancellationToken ct);

    #endregion

    #region ServiceBus

    Task ServiceBusSendJson(ServiceBusSender sender, object data, CancellationToken ct);

    #endregion
}

using Azure.Messaging.ServiceBus;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieServiceBusReceivedMessage<T>
{
    public required ServiceBusReceivedMessage ServiceBusReceivedMessage { get; init; }
    public required T Body { get; init; }
}

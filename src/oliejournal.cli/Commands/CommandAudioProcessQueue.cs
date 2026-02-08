using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using oliejournal.lib;
using oliejournal.lib.Models;
using oliejournal.lib.Services;

namespace oliejournal.cli.Commands;

public class CommandAudioProcessQueue(IServiceScopeFactory scopeFactory, IOlieConfig config, IOlieService os)
{
    const int MaxIterations = 50;

    public async Task<int> Run(CancellationToken ct)
    {
        var count = 0;
        var timeout = TimeSpan.FromSeconds(5);

        var bcClient = new BlobContainerClient(new Uri(config.BlobContainerUri), new DefaultAzureCredential());
        await using var sbClient = new ServiceBusClient(config.ServiceBus, new DefaultAzureCredential());
        await using var receiver = sbClient.CreateReceiver(config.AudioProcessQueue);
        await using var sender = sbClient.CreateSender(config.AudioProcessQueue);

        do
        {
            var message = await os.ServiceBusReceiveJson<AudioProcessQueueItemModel>(receiver, timeout, ct);
            if (message is null) break;

            using var scope = scopeFactory.CreateScope();
            var process = scope.ServiceProvider.GetRequiredService<IJournalProcess>();

            switch (message.Body.Step)
            {
                case lib.Enums.AudioProcessStepEnum.Transcript:
                    await process.TranscribeAudioEntry(message.Body.Id, bcClient, sender, ct);
                    break;
                case lib.Enums.AudioProcessStepEnum.Chatbot:
                    break;
                default:
                    throw new NotImplementedException();
            }

            await os.ServiceBusCompleteMessage(receiver, message, ct);
        } while (!ct.IsCancellationRequested && ++count < MaxIterations);

        return 0;
    }
}

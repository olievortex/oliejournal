using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using oliejournal.lib.Models;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;

namespace oliejournal.cli.Commands;

public class CommandAudioProcessQueue(IServiceScopeFactory scopeFactory, IOlieConfig config, IOlieService os)
{
    public async Task<int> Run(CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(60);

        var bcClient = new BlobContainerClient(new Uri(config.BlobContainerUri), new DefaultAzureCredential());
        await using var sbClient = new ServiceBusClient(config.ServiceBus, new DefaultAzureCredential());
        await using var receiver = sbClient.CreateReceiver(config.AudioProcessQueue);
        await using var sender = sbClient.CreateSender(config.AudioProcessQueue);

        do
        {
            var message = await os.ServiceBusReceiveJson<AudioProcessQueueItemModel>(receiver, timeout, ct);
            if (message is null) continue;

            using var scope = scopeFactory.CreateScope();
            var process = scope.ServiceProvider.GetRequiredService<IJournalProcess>();
            var id = message.Body.Id;

            switch (message.Body.Step)
            {
                case lib.Enums.AudioProcessStepEnum.Transcript:
                    await process.Transcribe(id, bcClient, sender, CancellationToken.None);
                    break;
                case lib.Enums.AudioProcessStepEnum.Chatbot:
                    await process.Chatbot(id, sender, CancellationToken.None);
                    break;
                case lib.Enums.AudioProcessStepEnum.VoiceOver:
                    await process.Voiceover(id, CancellationToken.None);
                    break;
                default:
                    throw new NotImplementedException();
            }

            await os.ServiceBusCompleteMessage(receiver, message, ct);

            // Be a respectful little background worker
            GC.Collect();
        } while (!ct.IsCancellationRequested);

        return 0;
    }
}

using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using oliejournal.lib;
using oliejournal.lib.Services;

namespace oliejournal.cli.Commands;

public class CommandAudioProcessQueue(IServiceScopeFactory scopeFactory, IOlieConfig config)
{
    public async Task<int> Run(CancellationToken ct)
    {
        var client = new BlobContainerClient(new Uri(config.BlobContainerUri), new DefaultAzureCredential());

        using var scope = scopeFactory.CreateScope();
        var process = scope.ServiceProvider.GetRequiredService<IJournalProcess>();

        return 0;
    }
}

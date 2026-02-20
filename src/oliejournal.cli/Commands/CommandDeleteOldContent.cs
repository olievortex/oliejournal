using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using oliejournal.lib.Processes.DeleteOldContentProcess;
using oliejournal.lib.Services;

namespace oliejournal.cli.Commands;

public class CommandDeleteOldContent(ILogger<CommandDeleteOldContent> logger, IOlieConfig config, OlieHost host)
{
    private const string LoggerName = $"oliejournal.cli {nameof(CommandDeleteOldContent)}";

    public async Task<int> Run(CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"{LoggerName} triggered");
            logger.LogInformation("{loggerName} triggered", LoggerName);

            using var scope = host.ServiceScopeFactory.CreateScope();
            var process = scope.ServiceProvider.GetRequiredService<IDeleteOldContentProcess>();

            var bcc = new BlobContainerClient(config.MySqlBackupContainer, new DefaultAzureCredential());

            await process.Run(bcc, ct);

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{_loggerName} Error: {error}", LoggerName, ex);
            Console.WriteLine($"{LoggerName} Error: {ex}");
            return 1;
        }
    }
}

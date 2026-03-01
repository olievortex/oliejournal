using Azure.Storage.Blobs;
using oliejournal.lib.Processes.JournalProcess;
using oliejournal.lib.Services;

namespace oliejournal.lib.Processes.DeleteUserProcess;

public class DeleteUserProcess(
    IDeleteUserUnit deleteUser,
    IJournalEntryIngestionUnit ingestion,
    IJournalEntryChatbotUnit chatbot,
    IJournalEntryVoiceoverUnit voiceover,
    IOlieKinde kinde) : IDeleteUserProcess
{
    public async Task DeleteAllUserData(string userId, BlobContainerClient client, CancellationToken ct)
    {
        var request = await deleteUser.CreateDeleteLog(userId, DateTime.UtcNow, ct);

        // Get all entries for the user
        var entries = await ingestion.GetJournalEntryList(userId, ct);

        // Delete OpenAI conversations
        await chatbot.DeleteAllConversations(userId, ct);

        // Delete each entry's blobs and database record
        foreach (var entry in entries)
        {
            // Delete original voice
            await ingestion.DeleteVoice(entry, client, ct);

            // Delete chatbot voiceover
            voiceover.DeleteLocalFile(entry);

            // Delete journal entry from database
            await ingestion.DeleteJournalEntry(entry.Id, ct);
        }

        // Delete the user from Kinde
        await kinde.DeleteUser(userId, ct);

        // Complete the request
        await deleteUser.UpdateDeleteLog(request, ct);
    }
}

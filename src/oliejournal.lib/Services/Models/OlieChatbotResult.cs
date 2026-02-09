namespace oliejournal.lib.Services.Models;

public class OlieChatbotResult
{
    public string ConversationId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}

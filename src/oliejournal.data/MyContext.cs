using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyContext(DbContextOptions<MyContext> options) : DbContext(options)
{
    public DbSet<ChatbotConversationEntity> ChatbotConversations { get; set; }
    public DbSet<ChatbotLogEntity> ChatbotLogs { get; set; }
    public DbSet<JournalEntryEntity> JournalEntries { get; set; }
    public DbSet<TranscriptLogEntity> TranscriptLogs { get; set; }
    public DbSet<UserDeleteLogEntity> UserDeleteLogs { get; set; }
}

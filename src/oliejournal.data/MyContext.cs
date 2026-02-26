using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyContext(DbContextOptions<MyContext> options) : DbContext(options)
{
    public virtual DbSet<ChatbotConversationEntity> ChatbotConversations { get; set; }
    public virtual DbSet<ChatbotLogEntity> ChatbotLogs { get; set; }
    public virtual DbSet<JournalEntryEntity> JournalEntries { get; set; }
    public virtual DbSet<TranscriptLogEntity> TranscriptLogs { get; set; }
}

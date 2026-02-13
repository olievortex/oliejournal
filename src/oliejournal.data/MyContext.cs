using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyContext(DbContextOptions<MyContext> options) : DbContext(options)
{
    public virtual DbSet<ConversationEntity> Conversations { get; set; }
    public virtual DbSet<JournalChatbotEntity> JournalChatbots { get; set; }
    public virtual DbSet<JournalEntryEntity> JournalEntries { get; set; }
    public virtual DbSet<JournalEntryListEntity> JournalEntryList { get; set; }
    public virtual DbSet<JournalTranscriptEntity> JournalTranscripts { get; set; }
}

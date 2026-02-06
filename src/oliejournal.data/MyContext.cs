using Microsoft.EntityFrameworkCore;
using oliejournal.data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyContext(DbContextOptions<MyContext> options) : DbContext(options)
{
    public virtual DbSet<JournalEntryEntity> JournalEntries { get; set; }
}

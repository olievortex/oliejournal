using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.data;

[ExcludeFromCodeCoverage]
public class MyContext(DbContextOptions<MyContext> options) : DbContext(options)
{
}

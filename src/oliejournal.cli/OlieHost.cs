using Microsoft.Extensions.DependencyInjection;

namespace oliejournal.cli;

public class OlieHost
{
    public required IServiceScopeFactory ServiceScopeFactory { get; set; }
}

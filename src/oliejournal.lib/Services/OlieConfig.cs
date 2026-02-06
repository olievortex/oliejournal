using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieConfig(IConfiguration config) : IOlieConfig
{
    public string MySqlConnection => GetString("OlieMySqlConnection");

    private string GetString(string key)
    {
        return config[key] ?? throw new ApplicationException($"{key} setting missing from configuration");
    }
}

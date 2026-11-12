using App.Mcp.Data;
using App.Mcp.Tools;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace App.Mcp.Resources;

[McpServerResourceType]
public class ExpenseResources
{
    private static readonly Lazy<string> PolicyText = new(() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "policy.md")));

    private readonly ILogger<ExpenseResources> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { 
            new JsonStringEnumConverter() 
        }
    };
    private readonly ExpenseStore _store;

    public ExpenseResources(ExpenseStore store, ILogger<ExpenseResources> logger)
    {
        _store = store;
        _logger = logger;
    }

    [McpServerResource(UriTemplate = "expenses://policy", Name = "Expense policy", MimeType = "text/markdown")]
    [Description("The App.Mcp expense policy. Attach this when you need to judge whether a report follows company rules.")]
    public string GetExpensePolicy()
    {
        _logger.LogInformation("[RESOURCE] expenses://policy called");

        return PolicyText.Value;
    }

    [McpServerResource(UriTemplate = "expenses://reports/{reportId}", Name = "Expense report", MimeType = "application/json")]
    [Description("A single expense report as JSON, addressed by its id. Attach this when you already know which report you want to talk about.")]
    public TextResourceContents GetExpenseReport(int reportId)
    {
        _logger.LogInformation("[RESOURCE] expenses://reports/{reportId} called", reportId);

        var report = _store.Reports.FirstOrDefault(r => r.Id == reportId)
            ?? throw new McpException($"No expense report found with id {reportId}.");

        return new TextResourceContents
        {
            Uri = $"expenses://reports/{reportId}",
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(report, JsonOptions)
        };
    }
}
using App.Mcp.Data;
using App.Mcp.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace App.Mcp.Prompts;

[McpServerPromptType]
public class ExpenseReviewPrompt
{
    private static readonly Lazy<string> PolicyText = new Lazy<string>(() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "policy.md")));

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ExpensesDbContext _store;
    private readonly ILogger<ExpenseReviewPrompt> _logger;

    public ExpenseReviewPrompt(ExpensesDbContext store, ILogger<ExpenseReviewPrompt> logger)
    {
        _store = store;
        _logger = logger;
    }

    [McpServerPrompt(Name = "review_expense_report")]
    [Description("Reviews one expense report against the App.Mcp expense policy and lists anything that breaks the rules.")]
    public async Task<IEnumerable<PromptMessage>> ReviewExpenseReport(
        [Description("The expense report id to review, for example 1003.")] int reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await _store.ReportDetail(reportId).FirstOrDefaultAsync(cancellationToken)
            ?? throw new McpException($"No expense report found with id {reportId}.");

        _logger.LogInformation("\nℹ️ [PROMPT] review_expense_report called for report {reportId}\n", reportId);

        // DEV note:
        // a prompt returns messages. Here we return 3 messages: the instructions, the policy, and the report data.
        // The prompt will be able to access the policy and report data as if they were files, so it can read them and check the report against the policy.

        return
        [
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock
                {
                    Text = """
                        You are reviewing an expense report for App.Mcp.
 
                        Work through the report line by line against the policy below. For every
                        breach, quote the rule number, the line it applies to and the amount. If a
                        line is fine, do not mention it. Finish with a single recommendation of
                        approve, reject, or return for more information.
                        """
                }
            },
            new PromptMessage
            {
                Role = Role.User,
                Content = new EmbeddedResourceBlock
                {
                    Resource = new TextResourceContents
                    {
                        Uri = "expenses://policy",
                        MimeType = "text/markdown",
                        Text = PolicyText.Value
                    }
                }
            },
            new PromptMessage
            {
                Role = Role.User,
                Content = new EmbeddedResourceBlock
                {
                    Resource = new TextResourceContents
                    {
                        Uri = $"expenses://reports/{reportId}",
                        MimeType = "application/json",
                        Text = JsonSerializer.Serialize(report, JsonOptions)
                    }
                }
            }
        ];
    }
}

using App.Mcp.Contracts;
using App.Mcp.Data;
using App.Mcp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace App.Mcp.Tools;

// Marks the class as containing tools. The SDK scans for this attribute at startup.
// Tool classes are resolved from the DI container, so anything you've registered can be injected.
[McpServerToolType] 
public class ExpenseTools
{
    private readonly ExpensesDbContext _db;
    private readonly ILogger<ExpenseTools> _logger;
    private readonly string? _clientId;

    public ExpenseTools(ExpensesDbContext store, ILogger<ExpenseTools> logger, IConfiguration configuration)
    {
        _db = store;
        _logger = logger;

        // Values from the "env" block in .mcp.json arrive as environment variables,
        // and Host.CreateApplicationBuilder loads all environment variables into IConfiguration.
        // (Same as Environment.GetEnvironmentVariable("clientId").)
        _clientId = configuration["clientId"];
    }

    // DEV note:
    // [Description] attribute is not for humans. It's the only thing the model reads. 
    //[Description("Searches expense reports by employee and by where they are in the approval workflow. Returns the matching reports with their line items.")]


    //[Description("Searches expense reports by employee name and status. Returns every matching report with its line items.")]
    [McpServerTool(Name = "search_expense_reports")]
    [Description("Searches expense reports by employee and by where they are in the approval workflow. Returns a summary of each match without the line items. Call get_expense_report when you need the lines.")]
    public async Task<IReadOnlyList<ExpenseReportSummary>> SearchExpenseReports(
        
        [Description("Part of an employee name, matched case insensitively. Omit to search across all employees.")]
        string? employeeName = null,

        //[Description("Only return reports in this status. Omit to return reports in any status.")]
        [Description("Where the report sits in the approval workflow right now. " +
            "Only use this when the question is about the current state. " +
            "A report that was approved or rejected was also submitted at some point, so do not " +
            "filter on Submitted to answer questions about what somebody has submitted.")]        
        ExpenseReportStatus? currentStatus = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "search_expense_reports called by clientId={ClientId} with employeeName={EmployeeName}, currentStatus={CurrentStatus}",
            _clientId ?? "(null)", employeeName ?? "(null)", currentStatus?.ToString() ?? "(null)");

        var query =
            from report in _db.Reports.AsNoTracking()
            join employee in _db.Employees on report.EmployeeId equals employee.Id
            select new { Report = report, Employee = employee };

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            query = query.Where(x => x.Employee.Name.Contains(employeeName));
        }

        if (currentStatus is not null)
        {
            query = query.Where(x => x.Report.Status == currentStatus);
        }

        return await query
            .OrderByDescending(x => x.Report.SubmittedOn)
            .Select(x => new ExpenseReportSummary(
                x.Report.Id,
                x.Report.Title,
                x.Employee.Name,
                x.Report.Status,
                x.Report.Currency,
                x.Report.TotalAmount,
                x.Report.SubmittedOn,
                x.Report.Lines.Count))
            .ToListAsync(cancellationToken);
    }

    //[Description("Gets a single expense report with its line items, by report id.")]
    [McpServerTool(Name = "get_expense_report")]
    [Description("Gets one expense report with its line items, by id. Call search_expense_reports first if you do not already have an id.")]
    public ExpenseReport GetExpenseReport(
        [Description("The expense report id, for example 1001.")] int reportId)
    {
        // DEV note: 
        // McpException is the one exception type whose message reaches the caller

        _logger.LogInformation("get_expense_report called by clientId={ClientId} with reportId={ReportId}",
            _clientId ?? "(null)", reportId);

        var report = _db.Reports.FirstOrDefault(r => r.Id == reportId);
        if (report is null)
        {
            _logger.LogWarning("get_expense_report: no report found with id {ReportId}", reportId);
            throw new McpException($"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");
        }

        return report;
    }


}

using App.Mcp.Data;
using App.Mcp.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace App.Mcp.Tools;

// Marks the class as containing tools. The SDK scans for this attribute at startup.
// Tool classes are resolved from the DI container, so anything you've registered can be injected.
[McpServerToolType] 
public class ExpenseTools
{
    private readonly ExpenseStore _store;

    public ExpenseTools(ExpenseStore store)
    {
        _store = store;
    }

    // DEV note:
    // [Description] attribute is not for humans. It's the only thing the model reads. 


    [McpServerTool(Name = "search_expense_reports")]
    //[Description("Searches expense reports by employee name and status. Returns every matching report with its line items.")]
    [Description("Searches expense reports by employee and by where they are in the approval workflow. Returns the matching reports with their line items.")]
    //
    public IReadOnlyList<ExpenseReport> SearchExpenseReports(
        [Description("Part of an employee name, matched case insensitively. Omit to search across all employees.")]
        string? employeeName = null,
        //[Description("Only return reports in this status. Omit to return reports in any status.")]
        [Description("Where the report sits in the approval workflow right now. " +
            "Only use this when the question is about the current state. " +
            "A report that was approved or rejected was also submitted at some point, so do not " +
            "filter on Submitted to answer questions about what somebody has submitted.")]        
        ExpenseReportStatus? currentStatus = null)
    {
        IEnumerable<ExpenseReport> results = _store.Reports;

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            var matchingIds = _store.Employees
                .Where(e => e.Name.Contains(employeeName, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Id)
                .ToHashSet();

            results = results.Where(r => matchingIds.Contains(r.EmployeeId));
        }

        if (currentStatus is not null)
        {
            results = results.Where(r => r.Status == currentStatus);
        }

        // assign employee name
        foreach(var report in results)
        {
            report.EmployeeName = _store.Employees.FirstOrDefault(e => e.Id == report.EmployeeId)?.Name ?? string.Empty;
        }

        return results.OrderByDescending(r => r.SubmittedOn).ToList();
    }

    [McpServerTool(Name = "get_expense_report")]
    //[Description("Gets a single expense report with its line items, by report id.")]
    [Description("Gets one expense report with its line items, by id. Call search_expense_reports first if you do not already have an id.")]
    public ExpenseReport GetExpenseReport(
        [Description("The expense report id, for example 1001.")] int reportId)
    {
        // DEV note: 
        // McpException is the one exception type whose message reaches the caller

        return _store
            .Reports
            .FirstOrDefault(r => r.Id == reportId)
            ?? throw new McpException($"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");
    }


}

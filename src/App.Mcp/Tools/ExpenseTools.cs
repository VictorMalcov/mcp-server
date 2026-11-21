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
    [McpServerTool(Name = "search_expense_reports")]
    [Description(
        "Searches expense reports by employee and " +
        "by where they are in the approval workflow. " +
        "Returns a summary of each match without the line items. " +
        "Call get_expense_report when you need the lines.")]
    public async Task<IReadOnlyList<ExpenseReportSummary>> SearchExpenseReports(

        [Description(
        "Part of an employee name, matched case insensitively. " +
        "Omit to search across all employees.")]
        string? employeeName = null,

        [Description("Where the report sits in the approval workflow right now. " +
            "Only use this when the question is about the current state. " +
            "A report that was approved or rejected was also submitted at some point, so do not " +
            "filter on Submitted to answer questions about what somebody has submitted.")]
        ExpenseReportStatus? currentStatus = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "\nℹ️ search_expense_reports called by clientId={ClientId} with employeeName={EmployeeName}, currentStatus={CurrentStatus}\n\n",
            _clientId ?? "(null)", employeeName ?? "(null)", currentStatus?.ToString() ?? "(null)");

        var query =
            from report in _db.Reports.AsNoTracking()
            join employee in _db.Employees on report.EmployeeId equals employee.Id
            select new { Report = report, Employee = employee };

        // DEV note: the same query written with LINQ method syntax (lambdas) instead of query syntax.
        // Both compile to the same expression tree, so EF Core generates identical SQL (an INNER JOIN).
        //var query = _db.Reports.AsNoTracking()
        //    .Join(
        //        _db.Employees,
        //        report => report.EmployeeId,
        //        employee => employee.Id,
        //        (report, employee) => new { Report = report, Employee = employee });

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
                Id: x.Report.Id,
                Title: x.Report.Title,
                EmployeeName: x.Employee.Name,
                Status: x.Report.Status,
                Currency: x.Report.Currency,
                TotalAmount: x.Report.TotalAmount,
                SubmittedOn: x.Report.SubmittedOn,
                LineCount: x.Report.Lines.Count))
            .ToListAsync(cancellationToken);
    }

    //[Description("Gets a single expense report with its line items, by report id.")]
    [McpServerTool(Name = "get_expense_report")]
    [Description("Gets one expense report with its line items, by id. Call search_expense_reports first if you do not already have an id.")]
    public async Task<ExpenseReportDetail> GetExpenseReport(
        [Description("The expense report id, for example 1001.")] int reportId,
        CancellationToken cancellationToken = default)
    {
        // DEV note: 
        // McpException is the one exception type whose message reaches the caller

        _logger.LogInformation("\nℹ️ get_expense_report called by clientId={ClientId} with reportId={ReportId}\n",
            _clientId ?? "(null)", reportId);

        // DEV note: original code before EF and database
        //var report = _db.Reports.FirstOrDefault(r => r.Id == reportId);
        //if (report is null)
        //{
        //    _logger.LogWarning("get_expense_report: no report found with id {ReportId}", reportId);
        //    throw new McpException($"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");
        //}
        //return report;

        return await _db.ReportDetail(reportId).FirstOrDefaultAsync(cancellationToken)
            ?? throw new McpException($"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");
    }



    [McpServerTool(Name = "create_expense_report")]
    [Description(
        "Creates a new expense report as a draft, together with its line items. " +
        "A report cannot be edited after it is created. If the user did not give a title, or gave a date without a year, " +
        "ask them for it before calling this tool rather than choosing a value yourself. " +
        "The report is not submitted, so tell the user the new report id and ask whether they want it submitted, then call submit_expense_report.")]
    public async Task<ExpenseReportDetail> CreateExpenseReportAsync(
        [Description("Part of the name of the employee the report belongs to. Must match exactly one employee.")]
        string employeeName,
        [Description("A short title for the report, for example Berlin customer workshop.")]
        string title,
        [Description("Three letter ISO 4217 code the expenses were paid in, for example EUR.")]
        string currency,
        [Description("The line items on the report. At least one is required.")]
        IReadOnlyList<NewExpenseLine> lines,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new McpException("The report needs a title. Ask the user what the expenses were for.");
        }

        if (lines is null or { Count: 0 })
        {
            throw new McpException("An expense report needs at least one line item. Ask the user what they are claiming for.");
        }

        if (lines.Any(line => line.Amount <= 0))
        {
            throw new McpException("Every line item must have an amount greater than zero.");
        }

        if (currency is null || currency.Trim().Length != 3)
        {
            throw new McpException("Currency must be a three letter ISO 4217 code, for example EUR.");
        }

        var matches = await _db.Employees
            .Where(e => e.Name.Contains(employeeName))
            .Select(e => new { e.Id, e.Name })
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            throw new McpException($"No employee matches '{employeeName}'. Ask the user for the full name.");
        }

        if (matches.Count > 1)
        {
            var names = string.Join(", ", matches.Select(m => m.Name));
            throw new McpException($"'{employeeName}' matches more than one employee: {names}. Ask the user which one they mean.");
        }

        _logger.LogInformation(
            "\nℹ️ create_expense_report called for employee {EmployeeName} with title={Title}, currency={Currency}, lines={LineCount}\n",
            matches[0].Name, title, currency, lines.Count);

        var report = new ExpenseReport
        {
            EmployeeId = matches[0].Id,
            Title = title.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            Status = ExpenseReportStatus.Draft,
            SubmittedOn = null,
            TotalAmount = lines.Sum(line => line.Amount),
            Lines = [
                .. lines.Select(line => new ExpenseLine
                    {
                        Description = line.Description.Trim(),
                        Category = line.Category.Trim(),
                        Amount = line.Amount,
                        IncurredOn = line.IncurredOn
                    })
            ]
        };

        _db.Reports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);

        return await _db.ReportDetail(report.Id).FirstAsync(cancellationToken);
    }


    [McpServerTool(Name = "submit_expense_report")]
    [Description("Submits a draft expense report for approval. Only works on a report that is still a draft, and it cannot be undone.")]
    public async Task<ExpenseReportDetail> SubmitExpenseReportAsync(
    [Description("The id of the draft expense report to submit.")] int reportId,
    CancellationToken cancellationToken = default)
    {
        var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new McpException($"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");

        if (report.Status is not ExpenseReportStatus.Draft)
        {
            throw new McpException($"Report {reportId} is {report.Status}, so it has already been submitted and cannot be submitted again.");
        }

        _logger.LogInformation("\nℹ️ submit_expense_report called for reportId={ReportId}\n", reportId);

        report.Status = ExpenseReportStatus.Submitted;
        report.SubmittedOn = DateOnly.FromDateTime(DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        return await _db.ReportDetail(reportId).FirstAsync(cancellationToken);
    }
}

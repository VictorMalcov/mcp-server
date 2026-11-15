using App.Mcp.Models;

namespace App.Mcp.Contracts;

public record ExpenseReportSummary(
    int Id,
    string Title,
    string EmployeeName,
    ExpenseReportStatus Status,
    string Currency,
    decimal TotalAmount,
    DateOnly? SubmittedOn,
    int LineCount);

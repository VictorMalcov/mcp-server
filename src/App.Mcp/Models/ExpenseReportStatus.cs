namespace App.Mcp.Models;

// Status is an enum rather than a string.
// That turns out to matter a lot once the model gets hold of it,
// and I’ll show you why in later steps
public enum ExpenseReportStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}

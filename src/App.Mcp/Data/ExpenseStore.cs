using App.Mcp.Models;

namespace App.Mcp.Data;

public class ExpenseStore
{
    public IReadOnlyList<Employee> Employees { get; } =
    [
        new Employee { Id = 1, Name = "Sara Haddad", Email = "sara.haddad@bitoftech.net" },
        new Employee { Id = 2, Name = "Omar Nasser", Email = "omar.nasser@bitoftech.net" },
        new Employee { Id = 3, Name = "Lina Farah", Email = "lina.farah@bitoftech.net" },
        new Employee { Id = 4, Name = "Taiseer Joudeh", Email = "taiseer.joudeh@bitoftech.net" }
    ];

    public IReadOnlyList<ExpenseReport> Reports { get; } =
    [
        new() {
            Id = 1001,
            EmployeeId = 1,
            Title = "Berlin customer workshop",
            Status = ExpenseReportStatus.Submitted,
            Currency = "EUR",
            TotalAmount = 842.50m,
            SubmittedOn = (DateOnly?)new DateOnly(2026, 9, 2),
            Lines = [
                new ExpenseLine { Id = 1, ExpenseReportId = 1001, Description = "Return flight LHR to BER", Category = "Travel", Amount = 410.00m, IncurredOn = new DateOnly(2026, 8, 31) },
                new ExpenseLine{ Id = 2, ExpenseReportId = 1001, Description = "Hotel, two nights", Category = "Accommodation", Amount = 312.50m, IncurredOn = new DateOnly(2026, 8, 31) },
                new ExpenseLine{ Id = 3, ExpenseReportId = 1001, Description = "Airport transfers", Category = "Travel", Amount = 120.00m, IncurredOn = new DateOnly(2026, 9, 1) },

            ]
        },
        new() {
            Id = 1002,
            EmployeeId = 1,
            Title = "Azure certification exam",
            Status = ExpenseReportStatus.Approved,
            Currency = "USD",
            TotalAmount = 165.00m,
            SubmittedOn = (DateOnly?)new DateOnly(2026, 8, 14),
            Lines = [
                new ExpenseLine { Id = 4, ExpenseReportId = 1002, Description = "AZ-204 exam voucher", Category = "Training", Amount = 165.00m, IncurredOn = new DateOnly(2026, 8, 12) },
            ]

        },
        new() {
            Id = 1003,
            EmployeeId = 2,
            Title = "London team offsite",
            Status = ExpenseReportStatus.Submitted,
            Currency = "GBP",
            TotalAmount = 1205.75m,
            SubmittedOn = (DateOnly?)new DateOnly(2026, 9, 9),
            Lines = [
                new ExpenseLine{ Id = 5, ExpenseReportId = 1003, Description = "Train tickets", Category = "Travel", Amount = 96.75m, IncurredOn = new DateOnly(2026, 9, 7) },
                new ExpenseLine{ Id = 6, ExpenseReportId = 1003, Description = "Hotel, three nights", Category = "Accommodation", Amount = 689.00m, IncurredOn = new DateOnly(2026, 9, 7) },
                new ExpenseLine{ Id = 7, ExpenseReportId = 1003, Description = "Team meals", Category = "Meals", Amount = 420.00m, IncurredOn = new DateOnly(2026, 9, 8) },
            ]
        },
        new() {
            Id = 1004,
            EmployeeId = 3,
            Title = "Mobile and broadband, August",
            Status = ExpenseReportStatus.Rejected,
            Currency = "GBP",
            TotalAmount = 88.40m,
            SubmittedOn = (DateOnly?)new DateOnly(2026, 8, 28),
            Lines = [
                new ExpenseLine { Id = 8, ExpenseReportId = 1004, Description = "Mobile plan", Category = "Communications", Amount = 48.40m, IncurredOn = new DateOnly(2026, 8, 25) },
                new ExpenseLine{ Id = 9, ExpenseReportId = 1004, Description = "Home broadband", Category = "Communications", Amount = 40.00m, IncurredOn = new DateOnly(2026, 8, 25) },
            ]
        },
        new() {
            Id = 1005,
            EmployeeId = 2,
            Title = "Client dinner, Amsterdam",
            Status = ExpenseReportStatus.Draft,
            Currency = "EUR",
            TotalAmount = 210.00m,
            SubmittedOn = (DateOnly?)null,
            Lines = [
                new ExpenseLine { Id = 10, ExpenseReportId = 1005, Description = "Dinner for four", Category = "Meals", Amount = 210.00m, IncurredOn = new DateOnly(2026, 9, 15) }
            ]
        }

    ];
}

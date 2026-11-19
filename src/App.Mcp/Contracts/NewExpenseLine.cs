namespace App.Mcp.Contracts;

public record NewExpenseLine(
    string Description,
    string Category,
    decimal Amount,
    DateOnly IncurredOn);

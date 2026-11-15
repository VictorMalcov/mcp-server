using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace App.Mcp.Data;

public class ExpensesDbContextFactory : IDesignTimeDbContextFactory<ExpensesDbContext>
{
    public ExpensesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("ExpensesDb")
            ?? throw new InvalidOperationException("ConnectionStrings:ExpensesDb is not set.");

        return new ExpensesDbContext(
            new DbContextOptionsBuilder<ExpensesDbContext>()
                .UseSqlServer(connectionString)
                .Options);
    }
}

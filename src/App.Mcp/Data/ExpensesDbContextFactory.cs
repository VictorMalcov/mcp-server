using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace App.Mcp.Data;

/*
  Design-time factory used only by the EF Core tools (dotnet ef migrations add,
  dotnet ef database update, etc.), never by the running MCP server.

  Why it exists: to create a migration, the EF tools need an ExpensesDbContext
  instance. Without this class they would try to build the app's host from Program.cs to
  resolve the context from DI, which means starting the MCP server. When the tools find an
  IDesignTimeDbContextFactory<TContext>, they call it instead and skip the host.

  How it works: it builds a small configuration from appsettings.json (copied next to
  the build output, hence AppContext.BaseDirectory) plus environment variables,
  so ConnectionStrings__ExpensesDb can override the file. It then reads the
  ExpensesDb connection string and configures SQL Server the same way Program.cs does.
  Because the connection string uses Authentication=Active Directory Default, the tools
  connect with your local Azure sign-in (e.g. az login or Visual Studio). No password is needed.
*/
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

        var dbContextOptions = new DbContextOptionsBuilder<ExpensesDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ExpensesDbContext(dbContextOptions);
    }
}

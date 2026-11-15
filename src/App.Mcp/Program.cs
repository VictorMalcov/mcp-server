using App.Mcp.Data;
using App.Mcp.Logging;
using App.Mcp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


//var builder = Host.CreateApplicationBuilder(args);
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // An MCP host starts us with whatever working directory it likes, so appsettings.json
    // has to be read from the folder the binary is in rather than the current directory.
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddDbContext<ExpensesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExpensesDb")));

builder.Services.AddHttpClient<FrankfurterClient>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// stdout carries the MCP protocol itself, so every log line must go to stderr.
// Sends every log, at every level, to stderr. This is the line that keeps stdout
// clean for the protocol. Leave it out and your server will appear to start and then fail to connect.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

// Also write our own logs (tool calls etc.) to a .txt file next to the built exe:
// bin/Debug/net10.0/logs/mcp-log.txt
var logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "mcp-log.txt");
builder.Logging.AddProvider(new FileLoggerProvider(logFilePath));
// Only App.Mcp.* categories go to the file, so SDK/hosting noise stays out of it.
builder.Logging.AddFilter<FileLoggerProvider>((category, level) =>
    category is not null && category.StartsWith("App.Mcp") && level >= LogLevel.Information);

//builder.Services.AddSingleton<ExpenseStore>();

builder.Services
    // registers the server itself
    .AddMcpServer()
    // configures to talk over standard input and output. `WithHttpTransport()` is another option.
    .WithStdioServerTransport()     
    // scans the assembly for [McpServerToolType] 
    // can be explicit instead with `.WithTools<ExpenseTools>()`
    .WithToolsFromAssembly()
    // scans the assembly for [McpServerResourceType]
    .WithResourcesFromAssembly()
    // scans the assembly for [McpServerPromptType]
    .WithPromptsFromAssembly();


// DEV note:
// AddMcpServer registers the server as a hosted service, so RunAsync starts it
// and blocks until the transport closes, and the SDK dispatches methods like tools/list
// and tools/call to the tools it found at startup.

await builder.Build().RunAsync();
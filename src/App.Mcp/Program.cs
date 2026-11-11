using App.Mcp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP protocol itself, so every log line must go to stderr.
// Sends every log, at every level, to stderr. This is the line that keeps stdout
// clean for the protocol. Leave it out and your server will appear to start and then fail to connect.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<ExpenseStore>();

builder.Services
    // registers the server itself
    .AddMcpServer()
    // configures to talk over standard input and output. `WithHttpTransport()` is another option.
    .WithStdioServerTransport()     
    // scans the assembly for [McpServerToolType] 
    // can be explicit instead with `.WithTools<ExpenseTools>()`
    .WithToolsFromAssembly();


// DEV note:
// AddMcpServer registers the server as a hosted service, so RunAsync starts it
// and blocks until the transport closes, and the SDK dispatches methods like tools/list
// and tools/call to the tools it found at startup.

await builder.Build().RunAsync();
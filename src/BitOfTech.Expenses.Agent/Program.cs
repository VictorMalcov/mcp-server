using Azure.AI.OpenAI;
using Azure.Identity;
//using BitOfTech.Expenses.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

// Models reply in Markdown with characters the default Windows console cannot render.
Console.OutputEncoding = System.Text.Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var mcpEndpoint = configuration["Mcp:Endpoint"]!;
var foundryEndpoint = configuration["Foundry:Endpoint"]!;
var deploymentName = configuration["Foundry:Deployment"]!;

await using var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(mcpEndpoint),
    Name = "bitoftech-expenses"
});

await using var mcpClient = await McpClient.CreateAsync(transport);

var tools = await mcpClient.ListToolsAsync();

Console.WriteLine($"Connected to {mcpClient.ServerInfo?.Name} at {mcpEndpoint}");
Console.WriteLine($"{tools.Count} tools: {string.Join(Environment.NewLine, tools.Select(t => t.Name))}");
using Azure.AI.OpenAI;
using Azure.Identity;
using BitOfTech.Expenses.Agent;

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

// Http client transport for MCP client.
await using var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(mcpEndpoint),
    Name = "bitoftech-expenses"
});

// Create MCP client
await using var mcpClient = await McpClient.CreateAsync(transport);

// List tools available on the MCP server.
var tools = await mcpClient.ListToolsAsync();

Console.WriteLine($"Connected to {mcpClient.ServerInfo?.Name} at {mcpEndpoint}");
Console.WriteLine($"{tools.Count} tools: {string.Join(Environment.NewLine, tools.Select(t => t.Name))}");


// Chat client for Foundry (Azure OpenAI)
IChatClient chatClient = new AzureOpenAIClient(
        endpoint: new Uri(foundryEndpoint),
        credential: new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();


// RUNNING THE AGENT LOOP

Console.WriteLine("ℹ️ Type a question, or an empty line to quit.");
Console.WriteLine();

var loop = new ToolLoop(chatClient, [.. tools]);

while (ReadInput() is { } input)
{
    await loop.RunTurnAsync(input);
    Console.WriteLine();
}

static string? ReadInput()
{
    Console.Write("❓> ");
    var line = Console.ReadLine();
    return string.IsNullOrWhiteSpace(line) ? null : line;
}
# Dev prompts

`Which expense reports were submitted by Sara, and what did they cost?`

`Which expense reports has Lina submitted?`

# Claude Code in terminal

Type `@` to see list of resources that you can attach to your prompt

# MCP server
- Use camelCase when naming a server

To inspect the server response: \
`npx @modelcontextprotocol/inspector dotnet run --project d:/projects/mcp/mcp-server/src/App.Mcp`


**How tool is called:** \
Claude Code sends a tools/call request:
```
{ 
    "jsonrpc": "2.0", "id": 3, "method": "tools/call",
    "params": { 
        "name": "get_expense_report", 
        "arguments": { 
            "reportId": 1001 
        } 
    } 
}
```

The server returns content, usually text, possibly JSON. That content is what I read.

# Rules when developing tools

- Good parameter naming is important. Use descriptive names for parameters, and avoid abbreviations or single-letter names.
- Say when not to use tools.
- Use enums to limit the values of parameters when appropriate.
- Write errors for the model to read and display to the user. Do not throw exceptions for user errors.
- Keep the numbers of tools down. Every tool name and description is sent on every request, so keep them short and descriptive.


| Primitive | Who Invokes It | How, in VS Code |
| :--- | :--- | :--- |
| **Tool** | The model | Automatically, in Agent mode |
| **Resource** | You | Add Context, then Select MCP Resource |
| **Prompt** | You | /mcp.SERVER.PROMPT |



# Docs

**Debug chat interactions** \
https://code.visualstudio.com/docs/agents/agent-troubleshooting/chat-debug-view#_an-mcp-tool-is-not-being-invoked
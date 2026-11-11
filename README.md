# Dev prompts

`Which expense reports were submitted by Sara, and what did they cost?`

`Which expense reports has Lina submitted?`



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
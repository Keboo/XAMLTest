using ModelContextProtocol.Protocol;

internal abstract class BaseTools
{
    protected static CallToolResult Failure(string message)
        => new()
        {
            IsError = true,
            Content = [new TextContentBlock { Text = message }]
        };

    protected static CallToolResult Success(string? message = null)
    {
        IList<ContentBlock> content = message is null
            ? []
            : [ new TextContentBlock { Text = message } ];
        return new()
        {
            IsError = false,
            Content = content
        };
    }
}


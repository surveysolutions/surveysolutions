using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Assistant;

/// <summary>
/// Represents the JSON response from the AI model
/// </summary>
public class AssistantAiResponse
{
    public bool Final { get; set; }
    public List<string> LoadGroups { get; set; } = new();
    public string? Expression { get; set; }
    public string Message { get; set; } = string.Empty;
}


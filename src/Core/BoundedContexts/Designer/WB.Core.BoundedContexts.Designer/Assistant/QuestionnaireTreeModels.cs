using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public class QuestionnaireTreeNode
{
    public string VariableName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<QuestionnaireTreeNode>? Children { get; set; }
}


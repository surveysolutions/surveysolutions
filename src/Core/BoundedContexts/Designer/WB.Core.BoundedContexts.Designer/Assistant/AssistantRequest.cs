using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public record AssistantRequest(
    Guid QuestionnaireId, 
    Guid EntityId, 
    string Prompt, 
    List<AssistantMessage> Messages);

public record AssistantMessage(string role, string content);

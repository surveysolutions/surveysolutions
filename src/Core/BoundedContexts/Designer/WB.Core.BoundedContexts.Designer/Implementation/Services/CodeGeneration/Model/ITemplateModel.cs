using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public interface ITemplateModel
    {
        Guid Id { get; }
        string VariableName { get; }
        string? Condition { get; }
    }
}

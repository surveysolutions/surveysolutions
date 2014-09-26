using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public interface ITemplateModel
    {
        Guid Id { set; get; }
        string VariableName { set; get; }
        string Conditions { set; get; }
    }
}
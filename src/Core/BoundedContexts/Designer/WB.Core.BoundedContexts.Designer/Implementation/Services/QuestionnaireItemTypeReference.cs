using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services;
public class QuestionnaireItemTypeReference
{
    public Guid Id { get; }
    public Type Type { get; }

    public QuestionnaireItemTypeReference(Guid id, Type type)
    {
        this.Id = id;
        this.Type = type;
    }
}


using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public interface IVariable : IComposite
    {
        VariableType Type { get; }
        string Name { get; }
        string Body { get; }
    }
}
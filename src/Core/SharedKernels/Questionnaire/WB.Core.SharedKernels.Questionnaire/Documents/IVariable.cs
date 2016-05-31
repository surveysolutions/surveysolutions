using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public interface IVariable : IComposite
    {
        VariableType Type { get; set; }
        string Name { get; set; }
        string Expression { get; set; }
    }
}
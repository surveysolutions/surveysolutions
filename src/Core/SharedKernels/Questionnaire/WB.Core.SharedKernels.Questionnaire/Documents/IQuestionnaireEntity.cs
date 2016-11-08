using System;
using Main.Core.Entities.Composite;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public interface IQuestionnaireEntity
    {
        Guid PublicKey { get; }

        IComposite GetParent();
    }
}
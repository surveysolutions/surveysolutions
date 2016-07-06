using System;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public interface IQuestionnaireEntity
    {
        Guid PublicKey { get; }
    }
}
using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public interface IQuestionnaireEntity
    {
        Guid PublicKey { get; }

        IEnumerable<IQuestionnaireEntity> Children { get; }

        IQuestionnaireEntity GetParent();
        void SetParent(IQuestionnaireEntity parent);
    }
}

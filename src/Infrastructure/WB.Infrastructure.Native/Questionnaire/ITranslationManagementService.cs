using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Enumerator.Native.Questionnaire
{
    public interface ITranslationManagementService
    {
        List<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId, Guid translationId);
        List<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId);
        void Delete(QuestionnaireIdentity questionnaireId);
        void Store(IEnumerable<TranslationInstance> translationInstances);
    }
}
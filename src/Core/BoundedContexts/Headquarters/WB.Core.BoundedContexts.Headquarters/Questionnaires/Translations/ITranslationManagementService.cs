using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations
{
    public interface ITranslationManagementService
    {
        IList<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId, Guid translationId);
        IList<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId);
        void Delete(QuestionnaireIdentity questionnaireId);
        void Store(IEnumerable<TranslationInstance> translationInstances);
    }
}
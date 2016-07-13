using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations
{
    public interface ITranslationManagementService
    {
        IList<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId, string language);
        IList<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId);
        void Delete(QuestionnaireIdentity questionnaireId);
    }
}
using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface ITranslationsService
    {
        IQuestionnaireTranslation Get(Guid questionnaireId, string culture);
        byte[] GetAsExcelFile(Guid questionnaireId, string culture);
        void Store(Guid questionnaireId, string culture, byte[] excelRepresentation);
        void CloneTranslation(Guid questionnaireId, string culture, Guid newQuestionnaireId, string newCulture);
    }
}
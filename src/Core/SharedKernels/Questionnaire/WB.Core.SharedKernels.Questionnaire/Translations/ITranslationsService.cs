using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ITranslationsService
    {
        ITranslation Get(Guid questionnaireId, Guid translationId);
        void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
    }
}

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface ITranslationStorage
    {
        ITranslation Get(QuestionnaireIdentity questionnaire, string language);
    }
}
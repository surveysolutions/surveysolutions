using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Infrastructure.Native.Questionnaire
{
    public interface IReusableCategoriesFillerIntoQuestionnaire
    {
        QuestionnaireDocument FillCategoriesIntoQuestionnaireDocument(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument);
    }
}

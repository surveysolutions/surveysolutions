using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.ReusableCategories
{
    public interface IReusableCategoriesFillerIntoQuestionnaire
    {
        QuestionnaireDocument FillCategoriesIntoQuestionnaireDocument(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument);
    }
}

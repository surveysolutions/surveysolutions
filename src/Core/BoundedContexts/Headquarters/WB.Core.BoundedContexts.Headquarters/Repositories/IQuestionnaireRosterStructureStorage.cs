using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IQuestionnaireRosterStructureStorage
    {
        QuestionnaireRosterStructure GetQuestionnaireRosterStructure(QuestionnaireIdentity id);
    }
}
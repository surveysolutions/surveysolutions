using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionnairesAndVersionsFactoryTests
{
    internal class QuestionnairesAndVersionsFactoryTestContext
    {
        internal static QuestionnaireBrowseInputModel CreateQuestionnairesAndVersionsInputModel(int pageSize = 2, int page = 0)
        {
            return new QuestionnaireBrowseInputModel{ Page = page, PageSize = pageSize};
        }

        internal static QuestionnairesAndVersionsFactory CreateQuestionnairesAndVersionsFactory(IReadSideRepositoryIndexAccessor indexAccessor = null)
        {
            return new QuestionnairesAndVersionsFactory(indexAccessor ?? Mock.Of<IReadSideRepositoryIndexAccessor>());
        }
    }
}
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamUsersAndQuestionnairesFactoryTests
{
    [NUnit.Framework.TestOf(typeof(TeamUsersAndQuestionnairesFactory))]
    public class TeamUsersAndQuestionnairesFactoryTestContext
    {
        protected static TeamUsersAndQuestionnairesFactory CreateViewFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires = null, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries = null)
        {
            return new TeamUsersAndQuestionnairesFactory(
                questionnaires ?? new TestPlainStorage<QuestionnaireBrowseItem>(), 
                interviewSummaries ?? new TestInMemoryWriter<InterviewSummary>());
        }
    }
}
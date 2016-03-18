using Machine.Specifications;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamUsersAndQuestionnairesFactoryTests
{
    [Subject(typeof(TeamUsersAndQuestionnairesFactory))]
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
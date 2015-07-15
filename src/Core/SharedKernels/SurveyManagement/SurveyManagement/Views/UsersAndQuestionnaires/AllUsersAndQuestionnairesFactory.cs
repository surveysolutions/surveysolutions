using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class AllUsersAndQuestionnairesFactory : IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesReader;

        public AllUsersAndQuestionnairesFactory(
            IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesReader, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.questionnairesReader = questionnairesReader;
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public AllUsersAndQuestionnairesView Load(AllUsersAndQuestionnairesInputModel input)
        {
            var allUsers =
                interviewSummaryReader.Query(
                    _ =>
                        _.Where(i => !i.IsDeleted)
                            .GroupBy(x => new {x.TeamLeadId, x.TeamLeadName})
                            .Where(x => x.Count() > 0)
                            .Select(x => new UsersViewItem {UserId = x.Key.TeamLeadId, UserName = x.Key.TeamLeadName})
                            .OrderBy(x => x.UserName).ToList());

            var questionnaires = this.questionnairesReader.Query(_ => _.Select(questionnaire => new TemplateViewItem
            {
                TemplateId = questionnaire.QuestionnaireId,
                TemplateName = questionnaire.Title,
                TemplateVersion = questionnaire.Version
            }).ToList());

            return new AllUsersAndQuestionnairesView
             {
                 Users = allUsers,
                 Questionnaires = questionnaires
             };
        }
    }
}

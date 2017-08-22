using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public interface IAllUsersAndQuestionnairesFactory
    {
        AllUsersAndQuestionnairesView Load();
        List<TemplateViewItem> GetQuestionnaires();
    }

    public class AllUsersAndQuestionnairesFactory : IAllUsersAndQuestionnairesFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader;

        public AllUsersAndQuestionnairesFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.questionnairesReader = questionnairesReader;
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public AllUsersAndQuestionnairesView Load()
        {
            var allUsers =
                this.interviewSummaryReader.Query(
                    _ =>   _.GroupBy(x => new {x.TeamLeadId, x.TeamLeadName})
                            .Where(x => x.Count() > 0)
                            .Select(x => new UsersViewItem {UserId = x.Key.TeamLeadId, UserName = x.Key.TeamLeadName})
                            .OrderBy(x => x.UserName).ToList());

            var questionnaires = this.GetTemplates();

            return new AllUsersAndQuestionnairesView
             {
                 Users = allUsers,
                 Questionnaires = questionnaires
             };
        }

        public List<TemplateViewItem> GetQuestionnaires()
        {
            return this.GetTemplates();
        }

        private List<TemplateViewItem> GetTemplates()
        {
            var questionnaires = this.questionnairesReader.Query(_ => _.Where(q => !q.IsDeleted)
                .Select(questionnaire => new TemplateViewItem
                {
                    TemplateId = questionnaire.QuestionnaireId,
                    TemplateName = questionnaire.Title,
                    TemplateVersion = questionnaire.Version
                }).OrderBy(x => x.TemplateName).ThenBy(n => n.TemplateVersion).ToList());
            return questionnaires;
        }
    }
}

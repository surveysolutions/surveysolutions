using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesFactory :
        IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public TeamUsersAndQuestionnairesFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input)
        {
            string indexName = typeof(InterviewsSearchIndex).Name;
            var indexedInterviewSummaries = indexAccessor.Query<SeachIndexContent>(indexName);

            var availableInterviews = indexedInterviewSummaries.Where(x => !x.IsDeleted && x.TeamLeadId == input.ViewerId);

            var users = availableInterviews
                .Select(i => new { i.ResponsibleId, i.ResponsibleName })
                .Distinct()
                .ToList()
                .Select(x => new UsersViewItem { UserId = x.ResponsibleId, UserName = x.ResponsibleName });

            var questionnaires = availableInterviews
               .Select(i => new { i.QuestionnaireId, i.QuestionnaireTitle, i.QuestionnaireVersion })
               .Distinct()
               .ToList()
               .Select(x =>  new TemplateViewItem
                        {
                            TemplateId = x.QuestionnaireId,
                            TemplateName = x.QuestionnaireTitle,
                            TemplateVersion = x.QuestionnaireVersion
                        });

            return new TeamUsersAndQuestionnairesView
                {
                    Users = users,
                    Questionnaires = questionnaires
                };
        }
    }
}

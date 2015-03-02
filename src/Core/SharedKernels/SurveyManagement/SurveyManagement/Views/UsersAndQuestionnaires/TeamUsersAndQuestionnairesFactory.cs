using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
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
            string interviewIndexName = typeof(InterviewsSearchIndex).Name;
            string userIndexName = typeof(UserDocumentsByBriefFields).Name;

            var allUsers = indexAccessor.Query<UserDocument>(userIndexName).Where(u => u.Supervisor.Id == input.ViewerId && !u.IsLockedByHQ && !u.IsLockedBySupervisor && !u.IsDeleted && u.Roles.Contains(UserRoles.Operator))
                .QueryAll()
                .Select(x => new UsersViewItem { UserId = x.PublicKey, UserName = x.UserName });

            var questionnaires = indexAccessor.Query<SeachIndexContent>(interviewIndexName).Where(x => !x.IsDeleted && x.TeamLeadId == input.ViewerId)
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
                    Users = allUsers,
                    Questionnaires = questionnaires
                };
        }
    }
}

using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class AllUsersAndQuestionnairesFactory : IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> usersReader;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesReader;

        public AllUsersAndQuestionnairesFactory(IQueryableReadSideRepositoryReader<UserDocument> usersReader,
            IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesReader)
        {
            this.usersReader = usersReader;
            this.questionnairesReader = questionnairesReader;
        }

        public AllUsersAndQuestionnairesView Load(AllUsersAndQuestionnairesInputModel input)
        {
            var allUsers = usersReader.Query(_ => 
                _.Where(u => !u.IsLockedByHQ  && !u.IsArchived && u.Roles.Contains(UserRoles.Supervisor))
                 .Select(x => new UsersViewItem { UserId = x.PublicKey, UserName = x.UserName })
                 .OrderBy(x => x.UserName)
                 .ToList());

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

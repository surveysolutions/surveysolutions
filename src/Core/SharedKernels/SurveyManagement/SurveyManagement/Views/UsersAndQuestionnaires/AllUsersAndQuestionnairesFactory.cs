using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class AllUsersAndQuestionnairesFactory : IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
    {
       private readonly IReadSideRepositoryIndexAccessor indexAccessor;

       public AllUsersAndQuestionnairesFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

       public AllUsersAndQuestionnairesView Load(AllUsersAndQuestionnairesInputModel input)
        {
           string userIndexName = typeof(UserDocumentsByBriefFields).Name;
           string questionnaireIndexName = typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex).Name;

           var allUsers = indexAccessor.Query<UserDocument>(userIndexName).Where(u => !u.IsLockedByHQ  && !u.IsDeleted && u.Roles.Contains(UserRoles.Supervisor))
               .QueryAll()
               .Select(x => new UsersViewItem { UserId = x.PublicKey, UserName = x.UserName });

           var allQuestionnaires = indexAccessor.Query<QuestionnaireAndVersionsItem>(questionnaireIndexName).QueryAll();

           var questionnaires = allQuestionnaires.SelectMany(questionnaire => questionnaire.Versions.Select(version => new TemplateViewItem
           {
               TemplateId = questionnaire.QuestionnaireId,
               TemplateName = questionnaire.Title,
               TemplateVersion = version
           })).ToList();

           return new AllUsersAndQuestionnairesView
            {
                Users = allUsers,
                Questionnaires = questionnaires
            };
        }
    }
}

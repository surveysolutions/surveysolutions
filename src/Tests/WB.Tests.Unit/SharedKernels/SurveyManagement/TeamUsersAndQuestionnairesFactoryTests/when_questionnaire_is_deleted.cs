using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamUsersAndQuestionnairesFactoryTests
{
    internal class when_questionnaire_is_deleted : TeamUsersAndQuestionnairesFactoryTestContext
    {
        Establish context = () =>
        {
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires = new TestPlainStorage<QuestionnaireBrowseItem>();
            var questionnaireBrowseItem = Create.QuestionnaireBrowseItem();
            questionnaireBrowseItem.IsDeleted = true;
            questionnaires.Store(questionnaireBrowseItem, "id");
            viewFactory = CreateViewFactory(questionnaires);
        };

        Because of = () => view = viewFactory.Load(new TeamUsersAndQuestionnairesInputModel(Guid.Empty));

        It should_not_return_deleted_questionnaires = () => view.Questionnaires.Count().ShouldEqual(0);

        static TeamUsersAndQuestionnairesFactory viewFactory;
        static TeamUsersAndQuestionnairesView view;
    }
}
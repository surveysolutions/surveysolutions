﻿using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamUsersAndQuestionnairesFactoryTests
{
    internal class when_questionnaire_is_deleted : TeamUsersAndQuestionnairesFactoryTestContext
    {
        Establish context = () =>
        {
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires = new TestPlainStorage<QuestionnaireBrowseItem>();
            var questionnaireBrowseItem = Create.Entity.QuestionnaireBrowseItem();
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
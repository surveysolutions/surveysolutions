﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireHelper
{
    internal class when_getting_public_questionnaire_data_deleted_questionnaires_should_not_be_editable_ : QuestionnaireHelperTestContext
    {
        Establish context = () =>
        {
            var user = Mock.Of<IMembershipWebUser>(x =>
                x.IsAdmin == true &&
                    x.UserId == Guid.NewGuid()
                );

            var userHelperMock = Mock.Of<IMembershipUserService>(x =>
                x.WebUser == user);

            var userViewFactoryMock = Mock.Of<IViewFactory<QuestionnaireListInputModel, QuestionnaireListView>>(x =>
                x.Load(Moq.It.IsAny<QuestionnaireListInputModel>()) == CreateQuestionnaireListView(user));

            questionnaireHelper = new UI.Designer.Code.QuestionnaireHelper(userHelperMock, userViewFactoryMock);
        };

        Because of = () =>
            result = questionnaireHelper.GetPublicQuestionnaires(Moq.It.IsAny<Guid>());

        It should_be_not_allowed_to_open_deleted_questionnaire_for_zero_element = () =>
            result[0].CanOpen.ShouldEqual(false);

        It should_be_allowed_to_open_not_deleted_questionnaire_for_first_element = () =>
            result[1].CanOpen.ShouldEqual(true);

        private static UI.Designer.Code.QuestionnaireHelper questionnaireHelper;
        private static IPagedList<QuestionnairePublicListViewModel> result;
    }
}
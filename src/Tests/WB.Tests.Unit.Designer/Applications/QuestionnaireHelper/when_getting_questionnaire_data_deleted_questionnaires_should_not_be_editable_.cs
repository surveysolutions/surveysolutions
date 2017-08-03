using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireHelper
{
    internal class when_getting_questionnaire_data_deleted_questionnaires_should_not_be_editable_ : QuestionnaireHelperTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            user = Mock.Of<IMembershipWebUser>(x =>
                x.IsAdmin == true &&
                x.UserId == Guid.NewGuid()
                );

            var userViewFactoryMock = Mock.Of<IQuestionnaireListViewFactory>(x =>
                x.Load(Moq.It.IsAny<QuestionnaireListInputModel>()) == CreateQuestionnaireListView(user));

            questionnaireHelper = new UI.Designer.Code.QuestionnaireHelper(userViewFactoryMock);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = questionnaireHelper.GetQuestionnaires(user.UserId, user.IsAdmin);

        [NUnit.Framework.Test] public void should_be_not_allowed_to_open_deleted_questionnaire_for_zero_element () =>
            result[0].CanOpen.ShouldEqual(false);

        [NUnit.Framework.Test] public void should_be_allowed_to_open_not_deleted_questionnaire_for_first_element () =>
            result[1].CanOpen.ShouldEqual(true);

        private static UI.Designer.Code.QuestionnaireHelper questionnaireHelper;
        private static IPagedList<QuestionnaireListViewModel> result;
        private static IMembershipWebUser user;
    }
}
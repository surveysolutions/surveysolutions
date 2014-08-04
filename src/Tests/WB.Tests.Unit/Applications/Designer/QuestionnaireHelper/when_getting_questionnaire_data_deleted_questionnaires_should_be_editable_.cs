using System;
using Machine.Specifications;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireHelper
{
    internal class when_getting_questionnaire_data_deleted_questionnaires_should_be_editable_ : QuestionnaireHelperTestContext
    {
        Establish context = () =>
        {
            helper = CreateQuestionnaireHelper();
        };

        Because of = () =>
            result = helper.GetPublicQuestionnaires(Guid.NewGuid());

        It should_be_not_allowed_to_edit_deleted_questionnaire = () =>
            result[0].CanEdit.ShouldEqual(false);

        It should_be_allowed_to_edit_not_deleted_questionnaire = () =>
            result[1].CanEdit.ShouldEqual(true);

        private static UI.Designer.Code.QuestionnaireHelper helper;
        private static IPagedList<QuestionnairePublicListViewModel> result;
    }
}
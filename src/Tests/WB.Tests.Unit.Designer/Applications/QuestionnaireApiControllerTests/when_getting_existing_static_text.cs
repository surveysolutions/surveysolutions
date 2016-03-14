using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class when_getting_existing_static_text : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            var questionnaireInfoFactory = Mock.Of<IQuestionnaireInfoFactory>(
                x => x.GetStaticTextEditView(questionnaireId, entityId) == CreateStaticTextView());

            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory);
        };

        Because of = () =>
            result = controller.EditStaticText(id: questionnaireId, staticTextId: entityId);

        It should_static_text_not_be_null = () =>
            result.ShouldNotBeNull();

        It should_static_text_has_type_of_NewEditStaticTextView = () =>
            result.ShouldBeOfExactType<NewEditStaticTextView>();

        private static QuestionnaireController controller;
        private static NewEditStaticTextView result;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid entityId = Guid.Parse("22222222222222222222222222222222");
    }
}
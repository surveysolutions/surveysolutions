using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_text_edit_view_and_questionnaire_is_absent : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireEntityDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetStaticTextEditView(questionnaireId, entityId);

        [NUnit.Framework.Test] public void should_return_null () =>
            result.Should().BeNull();

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static Mock<IDesignerQuestionnaireStorage> questionnaireEntityDetailsReaderMock;
        private static Guid entityId = st1Id;
    }
}

using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_text_edit_view : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireEntityDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireView = CreateQuestionnaireDocument();
            questionnaireEntityDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetStaticTextEditView(questionnaireId, EntityId);

        [NUnit.Framework.Test] public void should_return_not_null_view () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_question_with_Id_equals_questionId () =>
            result.Id.Should().Be(EntityId);

        [NUnit.Framework.Test] public void should_return_question_equals_g3 () =>
            result.Text.Should().Be(GetStaticText(EntityId).Text);

        private static IStaticText GetStaticText(Guid entityId)
        {
            return questionnaireView.Find<IStaticText>(entityId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IDesignerQuestionnaireStorage> questionnaireEntityDetailsReaderMock;
        private static readonly Guid EntityId = st1Id;

    }
}

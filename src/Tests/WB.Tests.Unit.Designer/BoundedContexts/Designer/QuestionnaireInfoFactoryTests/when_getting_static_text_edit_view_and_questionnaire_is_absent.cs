using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_text_edit_view_and_questionnaire_is_absent : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireEntityDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetStaticTextEditView(questionnaireId, entityId);

        [NUnit.Framework.Test] public void should_return_null () =>
            result.ShouldBeNull();

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionnaireEntityDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid entityId = st1Id;
    }
}
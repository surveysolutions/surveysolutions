using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_text_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionnaireEntityDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireView = CreateQuestionnaireDocument();
            questionnaireEntityDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetStaticTextEditView(questionnaireId, entityId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_question_with_Id_equals_questionId = () =>
            result.Id.ShouldEqual(entityId);

        It should_return_question_equals_g3 = () =>
            result.Text.ShouldEqual(GetStaticText(entityId).Text);

        private static IStaticText GetStaticText(Guid entityId)
        {
            return questionnaireView.Find<IStaticText>(entityId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionnaireEntityDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid entityId = st1Id;

    }
}
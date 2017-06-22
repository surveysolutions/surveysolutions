using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_variable_edit_view : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireEntityDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireView = CreateQuestionnaireDocument();
            questionnaireEntityDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
        }

        private void BecauseOf() =>
            result = factory.GetVariableEditView(questionnaireId, entityId);

        [NUnit.Framework.Test] public void should_return_not_null_view () =>
            result.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_return_variable_with_Id_equals_variableId () =>
            result.Id.ShouldEqual(entityId);

        [NUnit.Framework.Test] public void should_return_variable_with_itemId_equals_formated_variableId () =>
            result.ItemId.ShouldEqual(entityId.FormatGuid());

        [NUnit.Framework.Test] public void should_return_variable_name_equals () =>
            result.VariableData.Name.ShouldEqual(GetVariable(entityId).Name);

        [NUnit.Framework.Test] public void should_return_variable_type_equals () =>
            result.VariableData.Type.ShouldEqual(GetVariable(entityId).Type);

        [NUnit.Framework.Test] public void should_return_variable_expression_equals () =>
            result.VariableData.Expression.ShouldEqual(GetVariable(entityId).Expression);

        private static IVariable GetVariable(Guid entityId)
        {
            return questionnaireView.Find<IVariable>(entityId);
        }

        private static QuestionnaireInfoFactory factory;
        private static VariableView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionnaireEntityDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid entityId = var1Id;

    }
}
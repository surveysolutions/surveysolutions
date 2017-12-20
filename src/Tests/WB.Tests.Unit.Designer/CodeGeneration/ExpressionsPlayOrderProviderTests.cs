using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.Designer.CodeGeneration
{
    [TestFixture]
    public class ExpressionsPlayOrderProviderTests
    {
        [Test]
        public void when_GetExpressionsPlayOrder_for_question_and_depended_variable()
        {
            var variableId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.NumericIntegerQuestion(intQuestionId, variable: "i", enablementCondition: "v > 5"),
                Create.Variable(variableId, variableName: "v", expression: "i"));

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetExpressionsPlayOrder(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder, Is.EqualTo(new[] { chapterId, intQuestionId, variableId }));
        }

        [Test]
        public void when_GetExpressionsPlayOrder_for_2_questions_and_depended_variable()
        {
            var variableId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var textQuestionId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.NumericIntegerQuestion(intQuestionId, variable: "i", enablementCondition: "v > 5"),
                Create.Variable(variableId, variableName: "v", expression: "i"),
                Create.TextQuestion(textQuestionId, enablementCondition: "v < 10"));

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetExpressionsPlayOrder(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder, Is.EqualTo(new[] { chapterId, intQuestionId, variableId, textQuestionId }));
        }



        [Test]
        public void when_GetExpressionsPlayOrder_for_sections_without_condition_dependencies()
        {
            var realNumericQuestion = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var textQuestionId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.NumericIntegerQuestion(intQuestionId, variable: "i"),
                Create.NumericRealQuestion(realNumericQuestion, variable: "r"),
                Create.TextQuestion(textQuestionId));

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetExpressionsPlayOrder(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder, Is.EqualTo(new[] { chapterId, textQuestionId, realNumericQuestion, intQuestionId }));
        }
    }
}
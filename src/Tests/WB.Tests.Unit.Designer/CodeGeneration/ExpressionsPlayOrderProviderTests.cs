using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.QuestionnaireEntities;
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

        [Test]
        public void when_GetValidationDependencyGraph_for_2_questions_and_depended_variable_without_validations()
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

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetValidationDependencyGraph(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder.Count, Is.EqualTo(0));
        }

        [Test]
        public void when_GetValidationDependencyGraph_for_2_questions_and_depended_variable_with_validations()
        {
            var variableId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var int2QuestionId = Guid.NewGuid();
            var textQuestionId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.NumericIntegerQuestion(intQuestionId, variable: "i", validationConditions: new[] {new ValidationCondition("v > 5", "error"),}),
                Create.NumericIntegerQuestion(int2QuestionId, variable: "i2", validationConditions: new[] {new ValidationCondition("i2 > 5 && v> 5", "error"),}),
                Create.Variable(variableId, variableName: "v", expression: "i"),
                Create.TextQuestion(textQuestionId, variable: "t", enablementCondition: "v < 10", validationConditions: new[] {new ValidationCondition("t == \"\"", "error"),})
            );

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetValidationDependencyGraph(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder.Count, Is.EqualTo(3));
            Assert.That(expressionsPlayOrder[variableId], Is.EqualTo(new[] { intQuestionId, int2QuestionId }));
            Assert.That(expressionsPlayOrder[int2QuestionId], Is.EqualTo(new[] { int2QuestionId }));
            Assert.That(expressionsPlayOrder[textQuestionId], Is.EqualTo(new[] { textQuestionId }));
        }

        [Test]
        public void when_GetValidationDependencyGraph_for_question_inside_roster()
        {
            var intQuestionId = Guid.NewGuid();
            var int2QuestionId = Guid.NewGuid();
            var rosterId = Guid.NewGuid();
            var rosterTrigerId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.NumericIntegerQuestion(rosterTrigerId, variable: "rt"),
                Create.Roster(rosterId, variable:"r", rosterSizeQuestionId: rosterTrigerId, children: new []
                {
                    Create.NumericIntegerQuestion(intQuestionId, variable: "i", validationConditions: new[] { new ValidationCondition("r.Count > 5", "error"), }),
                    Create.NumericIntegerQuestion(int2QuestionId, variable: "i2", validationConditions: new[] { new ValidationCondition("rt > 5", "error"), }),
                })
            );

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetValidationDependencyGraph(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder.Count, Is.EqualTo(2));
            Assert.That(expressionsPlayOrder[rosterId], Is.EqualTo(new[] { intQuestionId }));
            Assert.That(expressionsPlayOrder[rosterTrigerId], Is.EqualTo(new[] { int2QuestionId }));
        }
    }
}
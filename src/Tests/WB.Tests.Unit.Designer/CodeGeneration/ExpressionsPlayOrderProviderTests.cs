using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
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

            Assert.That(expressionsPlayOrder[variableId], Is.EqualTo(new[] { intQuestionId, int2QuestionId }));
            Assert.That(expressionsPlayOrder.ContainsKey(int2QuestionId), Is.False);
            Assert.That(expressionsPlayOrder.ContainsKey(textQuestionId), Is.False);
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

            Assert.That(expressionsPlayOrder[rosterId], Is.EqualTo(new[] { intQuestionId }));
            Assert.That(expressionsPlayOrder[rosterTrigerId], Is.EqualTo(new[] { int2QuestionId }));
        }

        [Test]
        public void when_GetValidationDependencyGraph_for_question_with_validation_inside_formula()
        {
            var intQuestionId = Guid.NewGuid();
            var macrosId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.NumericIntegerQuestion(intQuestionId, validationConditions: new []{ new ValidationCondition("$valid", "error"), })
            );
            questionnaireDocument.Macros = new Dictionary<Guid, Macro> {{macrosId, Create.Macro("valid", "self > 10")}};


            var expressionProcessor = Create.RoslynExpressionProcessor();
            var macrosesService = Create.MacrosSubstitutionService();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor, macrosesService);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetValidationDependencyGraph(questionnaireDocument.AsReadOnly());

            Assert.That(expressionsPlayOrder.ContainsKey(intQuestionId), Is.False);
        }

        [Test]
        public void when_GetDependencyGraph_for_linked_question_with_filter()
        {
            Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
            Guid rosterId = Guid.Parse("88888888888888888888888888888888");
            Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
            Guid q2Id = Guid.Parse("22222222222222222222222222222222");
            Guid q3Id = Guid.Parse("33333333333333333333333333333333");
            Guid q4Id = Guid.Parse("44444444444444444444444444444444");
            Guid q5Id = Guid.Parse("55555555555555555555555555555555");

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2"),
                Abc.Create.Entity.Roster(rosterId, variable:"r", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(q3Id, variable: "age")
                }),
                Abc.Create.Entity.Roster(roster1Id, variable:"r1", fixedTitles: new[] { "1", "2"}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(q5Id, variable: "ageFilter"),
                    Abc.Create.Entity.SingleQuestion(q4Id, variable: "q4", linkedToQuestionId: q3Id, linkedFilter: "age > current.ageFilter")
                })
            });

            var dependensies = GetDependensies(questionnaireDocument, q5Id);

            Assert.That(dependensies, Is.EqualTo(new[] { q5Id, q4Id }));
        }

        [Test]
        public void when_GetDependencyGraph_for_linked_question_with_filter_2()
        {
            Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
            Guid rosterId = Guid.Parse("88888888888888888888888888888888");
            Guid groupId = Guid.Parse("77777777777777777777777777777777");
            Guid q2Id = Guid.Parse("22222222222222222222222222222222");
            Guid q3Id = Guid.Parse("33333333333333333333333333333333");
            Guid q4Id = Guid.Parse("44444444444444444444444444444444");
            Guid q5Id = Guid.Parse("55555555555555555555555555555555");

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.MultyOptionsQuestion(q2Id, variable: "mo_start"),
                Abc.Create.Entity.NumericIntegerQuestion(q5Id, "ie"),
                Abc.Create.Entity.Roster(rosterId, variable:"r", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(q3Id, variable: "ii")
                }),
                Abc.Create.Entity.Group(groupId, variable:"gr", children: new IComposite[]
                {
                    Abc.Create.Entity.MultyOptionsQuestion(q4Id, variable: "mo", linkedToRosterId: rosterId, 
                        linkedFilter: "ii > 10", enablementCondition: "ie == 1")
                })
            });

            var dependensies = GetDependensies(questionnaireDocument, q3Id);

            Assert.That(dependensies, Is.EqualTo(new[] { q3Id, q4Id }));
        }

        [Test]
        public void when_GetDependencyGraph_for_dependent_questions()
        {
            Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
            Guid q1Id = Guid.Parse("11111111111111111111111111111111");
            Guid q2Id = Guid.Parse("22222222222222222222222222222222");
            Guid q3Id = Guid.Parse("33333333333333333333333333333333");
            Guid q4Id = Guid.Parse("44444444444444444444444444444444");

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.MultyOptionsQuestion(q1Id, variable: "mo_start"),
                Abc.Create.Entity.NumericIntegerQuestion(q2Id, "n1", enablementCondition: "mo_start.Contains(1)"),
                Abc.Create.Entity.NumericIntegerQuestion(q3Id, "n2", enablementCondition: "mo_start.Contains(2)"),
                Abc.Create.Entity.MultyOptionsQuestion(q4Id, variable: "mo_end", enablementCondition: "n1>0 || n2>0"),
            });

            var dependensies = GetDependensies(questionnaireDocument, q1Id);

            Assert.That(dependensies, Is.EqualTo(new[] { q1Id, q3Id, q2Id, q4Id }));
        }



        private static List<Guid> GetDependensies(QuestionnaireDocument questionnaireDocument, Guid entityId)
        {
            var expressionProcessor = Create.RoslynExpressionProcessor();
            var macrosesService = Create.MacrosSubstitutionService();
            var expressionsPlayOrderProvider = new ServiceFactory().ExpressionsPlayOrderProvider(expressionProcessor, macrosesService);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetDependencyGraph(questionnaireDocument.AsReadOnly());
            var dependensies = new TopologicalSorter<Guid>().Sort(expressionsPlayOrder, entityId);
            return dependensies;
        }
    }
}
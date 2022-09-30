using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.CodeGeneration
{
    [TestFixture]
    [TestOf(typeof(ExpressionsPlayOrderProvider))]
    public class ExpressionsPlayOrderProviderTests
    {
        [Test]
        public void when_GetExpressionsPlayOrder_for_question_and_depended_variable()
        {
            var variableId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var chapterId = Guid.NewGuid();

            var questionnaireDocument = Create.QuestionnaireDocumentWithoutChildren(
                children: Create.Section(sectionId: chapterId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(intQuestionId, variable: "i", enablementCondition: "v > 5"),
                    Create.Variable(variableId, variableName: "v", expression: "i"),
                }));

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor);

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

            var questionnaireDocument = Create.QuestionnaireDocumentWithoutChildren(
                children: Create.Section(sectionId: chapterId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(intQuestionId, variable: "i", enablementCondition: "v > 5"),
                    Create.Variable(variableId, variableName: "v", expression: "i"),
                    Create.TextQuestion(textQuestionId, enablementCondition: "v < 10"),
                }));

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor);

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

            var questionnaireDocument = Create.QuestionnaireDocumentWithoutChildren(
                children: Create.Section(sectionId: chapterId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(intQuestionId, variable: "i"),
                    Create.NumericRealQuestion(realNumericQuestion, variable: "r"),
                    Create.TextQuestion(textQuestionId),
                }));

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor);

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
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor);

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
                Create.NumericIntegerQuestion(intQuestionId, variable: "i", validationConditions: new[] { new ValidationCondition("v > 5", "error"), }),
                Create.NumericIntegerQuestion(int2QuestionId, variable: "i2", validationConditions: new[] { new ValidationCondition("i2 > 5 && v> 5", "error"), }),
                Create.Variable(variableId, variableName: "v", expression: "i"),
                Create.TextQuestion(textQuestionId, variable: "t", enablementCondition: "v < 10", validationConditions: new[] { new ValidationCondition("t == \"\"", "error"), })
            );

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor);

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
                Create.Roster(rosterId, variable: "r", rosterSizeQuestionId: rosterTrigerId, children: new[]
                {
                    Create.NumericIntegerQuestion(intQuestionId, variable: "i", validationConditions: new[] { new ValidationCondition("r.Count > 5", "error"), }),
                    Create.NumericIntegerQuestion(int2QuestionId, variable: "i2", validationConditions: new[] { new ValidationCondition("rt > 5", "error"), }),
                })
            );

            var expressionProcessor = Create.RoslynExpressionProcessor();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor);

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
                Create.NumericIntegerQuestion(intQuestionId, validationConditions: new[] { new ValidationCondition("$valid", "error"), })
            );
            questionnaireDocument.Macros = new Dictionary<Guid, Macro> { { macrosId, Create.Macro("valid", "self > 10") } };


            var expressionProcessor = Create.RoslynExpressionProcessor();
            var macrosesService = Create.MacrosSubstitutionService();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor, macrosesService);

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

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Create.MultyOptionsQuestion(q2Id, variable: "q2"),
                Create.Roster(rosterId, variable:"r",
                    rosterSizeQuestionId: q2Id,
                    children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q3Id, variable: "age")
                }),
                Create.Roster(roster1Id, variable:"r1", fixedTitles: new[] { "1", "2"}, 
                    children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q5Id, variable: "ageFilter"),
                    Create.SingleQuestion(q4Id, variable: "q4", linkedToQuestionId: q3Id, linkedFilter: "age > current.ageFilter")
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

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Create.MultyOptionsQuestion(q2Id, variable: "mo_start"),
                Create.NumericIntegerQuestion(q5Id, "ie"),
                Create.Roster(rosterId, variable:"r", rosterSizeQuestionId: q2Id, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q3Id, variable: "ii")
                }),
                Create.Group(groupId, variable:"gr", children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(q4Id, variable: "mo", linkedToRosterId: rosterId,
                        linkedFilterExpression: "ii > 10", enablementCondition: "ie == 1")
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

            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Create.MultyOptionsQuestion(q1Id, variable: "mo_start"),
                Create.NumericIntegerQuestion(q2Id, "n1", enablementCondition: "mo_start.Contains(1)"),
                Create.NumericIntegerQuestion(q3Id, "n2", enablementCondition: "mo_start.Contains(2)"),
                Create.MultyOptionsQuestion(q4Id, variable: "mo_end", enablementCondition: "n1>0 || n2>0"),
            });

            var dependensies = GetDependensies(questionnaireDocument, q1Id);

            Assert.That(dependensies, Is.EqualTo(new[] { q1Id, q3Id, q2Id, q4Id }));
        }

        [Test]
        public void when_GetDependencyGraph_for_linked_to_list_with_option_filter()
        {
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Id.g9, children: new IComposite[]
            {
                Create.TextListQuestion(Id.g1, variable: "list"),
                Create.NumericIntegerQuestion(Id.g7, "num"),
                Create.MultyOptionsQuestion(Id.g2, variable: "multi_without_filter", linkedToQuestionId: Id.g1, enablementCondition: "num == 7"),
                Create.MultyOptionsQuestion(Id.g3, variable: "multi_with_filter", linkedToQuestionId: Id.g1, optionsFilterExpression: "multi_without_filter.Contains(@optioncode)"),
                Create.TextQuestion(Id.g4, variable: "text", enablementCondition: "multi_with_filter == 5"),
            });

            var dependencies = GetDependensies(questionnaireDocument, Id.g7);
            CollectionAssert.AreEqual(dependencies, new[] { Id.g7, Id.g2, Id.g3, Id.g4});
        }

        [Test]
        public void when_static_text_depends_on_variable_Should_add_such_dependency_in_graph()
        {
            Guid questionnaireId = Id.g9;
            Guid v1Id = Id.g1;
            Guid s2Id = Id.g2;
            Guid q3Id = Id.g3;
            
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
            {
                Create.TextQuestion(q3Id, variable: "txt"),
                Create.Variable(v1Id, VariableType.String, "variable1"),
                Create.StaticText(s2Id, attachmentName: "variable1"),
            });

            var dependensies = GetDependensies(questionnaireDocument, s2Id);

            Assert.That(dependensies, Is.EquivalentTo(new[] { s2Id, v1Id }));
        }
        

        private static List<Guid> GetDependensies(QuestionnaireDocument questionnaireDocument, Guid entityId)
        {
            var expressionProcessor = Create.RoslynExpressionProcessor();
            var macrosesService = Create.MacrosSubstitutionService();
            var expressionsPlayOrderProvider = Create.ExpressionsPlayOrderProvider(expressionProcessor, macrosesService);

            var expressionsPlayOrder = expressionsPlayOrderProvider.GetDependencyGraph(questionnaireDocument.AsReadOnly());
            var dependensies = new TopologicalSorter<Guid>().Sort(expressionsPlayOrder, entityId);
            return dependensies;
        }
    }
}

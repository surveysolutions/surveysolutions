using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class ErrorsTests
    {
        private static readonly Guid Id1 = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid Id2 = Guid.Parse("22222222222222222222222222222222");

        [Test]
        public void question_with_validation_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.TextQuestion(validationConditions: new[] { Create.ValidationCondition("System.DateTime.UtcNow.ToString() == self") })
                })
                .ExpectError("WB0118");

        [Test]
        public void question_with_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.TextQuestion(enablementCondition: "DateTime.Now.AddMonth(1) > System.DateTime.Today")
                })
                .ExpectError("WB0118");

        [Test]
        public void question_with_option_filter_uses_forbidden_DateTime_properties()
           => Create.QuestionnaireDocumentWithOneChapter(new[]
               {
                    Create.SingleQuestion(optionsFilter: "(System.DateTime.Today.AddMonth(1) - System.DateTime.Today).TotalSeconds > 10")
               })
               .ExpectError("WB0118");

        [Test]
        public void question_with_linked_filter_uses_forbidden_DateTime_properties()
           => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
               {
                    Create.TextListQuestion(questionId: Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1, linkedFilter: "(System.DateTime.Today.AddMonth(1) - DateTime.Today).TotalSeconds > 10")
               })
               .ExpectError("WB0118");

        [Test]
        public void static_text_with_validation_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(validationConditions: new[] { Create.ValidationCondition("System.DateTime.UtcNow.ToString() == self") })
                })
                .ExpectError("WB0118");

        [Test]
        public void static_text_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(enablementCondition: "DateTime.Now.AddMonth(1) > System.DateTime.Today")
                })
                .ExpectError("WB0118");

        [Test]
        public void group_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Group(enablementCondition: "DateTime.Now.AddMonth(1) > DateTime.Today")
                })
                .ExpectError("WB0118");

        [Test]
        public void variable_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Variable(expression: "(System.DateTime.UtcNow.AddMonth(1) - System.DateTime.Today).TotalSeconds")
                })
                .ExpectError("WB0118");

        [Test]
        public void circular_reference_in_enablings()
            => Create.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Question(variable: "x", enablementCondition: "y > 0"),
                    Create.Question(variable: "y", enablementCondition: "x > 0"),
                })
                .ExpectError("WB0056");

        [Test]
        public void circular_reference_in_enablings_hidden_by_macro()
            => Create.QuestionnaireDocumentWithOneChapter(
                    macros: new[]
                    {
                        Create.Macro(name: "m", content: "x > 0"),
                    },
                    children: new[]
                    {
                        Create.Question(variable: "x", enablementCondition: "y > 0"),
                        Create.Question(variable: "y", enablementCondition: "$m"),
                    })
                .ExpectError("WB0056");

        [Test]
        public void linked_question_reference_on_TextList_question_from_wrong_scope()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Roster(children: new IComposite[]
                    {
                        Create.TextListQuestion(Id1),
                    }),
                    Create.SingleQuestion(linkedToQuestionId: Id1)
                })
                .ExpectError("WB0116");


        [Test]
        public void linked_question_reference_on_TextList_question_from_current_and_child_scope()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.Roster(children: new IComposite[]
                    {
                        Create.TextListQuestion(Id2),
                        Create.SingleQuestion(linkedToQuestionId: Id1),
                        Create.SingleQuestion(linkedToQuestionId: Id2)
                    }),
                })
                .ExpectNoError("WB0116");

        [Test]
        public void linked_question_reference_on_TextList_question()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1)
                })
                .ExpectNoError("WB0012");

        [Test]
        public void linked_to_TextList_question_has_options_filter()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1, optionsFilter: "something")
                })
                .ExpectCritical("WB0117");

        [Test]
        public void linked_to_TextList_question_has_linked_filter()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1, linkedFilter: "something")
                })
                .ExpectCritical("WB0117");

        [Test]
        public void lookup_table_has_reserved_word_in_header()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(Id1, variable: "q1")
            });

            Guid tableId = Guid.Parse("11111111111111111111111111111111");
            questionnaire.LookupTables.Add(tableId, Create.LookupTable("hello"));

            var lookupTableContent = Create.LookupTableContent(new[] { "int", "long"},
            Create.LookupTableRow(1, new decimal?[] { 1.15m, 10 }));
            Mock<ILookupTableService> lookupTableServiceMock = new Mock<ILookupTableService>();

            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaire.PublicKey, tableId))
                .Returns(lookupTableContent);

            var verifier = Create.QuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);

            var result = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            result.Single().Code.ShouldEqual("WB0031");
        }
        
    }
}
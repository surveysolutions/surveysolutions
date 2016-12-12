using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class ErrorsTests
    {
        private static readonly Guid Id1 = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid Id2 = Guid.Parse("22222222222222222222222222222222");

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
    }
}
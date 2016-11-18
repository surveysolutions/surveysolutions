using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class ErrorsTests
    {
        private readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private readonly Guid r1Id = Guid.Parse("99999999999999999999999999999999");

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
                Create.Roster(r1Id, fixedRosterTitles: new [] { Create.FixedRosterTitle(1, "1")}, children: new IComposite[]
                {
                    Create.TextListQuestion(q1Id, variable: "var1"),
                }),
                Create.SingleQuestion(q2Id, variable: "var2", linkedToQuestionId: q1Id)
            }).ExpectError("WB0116");


        [Test]
        public void linked_question_reference_on_TextList_question_from_current_and_child_scope()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(q1Id, variable: "var1"),
                Create.Roster(r1Id, fixedRosterTitles: new [] { Create.FixedRosterTitle(1, "1")}, children: new IComposite[]
                {
                    Create.TextListQuestion(q2Id, variable: "var2"),
                    Create.SingleQuestion(q3Id, variable: "var3", linkedToQuestionId: q1Id),
                    Create.SingleQuestion(q4Id, variable: "var4", linkedToQuestionId: q2Id)
                }),
            }).ExpectNoError("WB0116");

        [Test]
        public void linked_question_reference_on_TextList_question()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(q1Id, variable: "var1"),
                Create.SingleQuestion(q2Id, variable: "var2", linkedToQuestionId: q1Id)
            }).ExpectNoError("WB0012");

        [Test]
        public void linked_to_TextList_question_has_options_filter()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(q1Id, variable: "var1"),
                Create.SingleQuestion(q2Id, variable: "var2", linkedToQuestionId: q1Id, optionsFilter: "something")
            }).ExpectCritical("WB0117");

        [Test]
        public void linked_to_TextList_question_has_linked_filter()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(q1Id, variable: "var1"),
                Create.SingleQuestion(q2Id, variable: "var2", linkedToQuestionId: q1Id, linkedFilter: "something")
            }).ExpectCritical("WB0117");
    }
}
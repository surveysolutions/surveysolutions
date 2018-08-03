using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_single_option_linked_question_with_option_not_from_the_list : InterviewTestsContext
    {
        [Test] public void should_throw_exception_with_message_containting__type_QRBarcode_expected__ () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var triggerQuestionId = Guid.NewGuid();
            var titleQuestionId = Guid.NewGuid();
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.SingleQuestion(id: linkedToQuestionId, linkedToQuestionId: titleQuestionId,
                    variable: "link_single"),
                Abc.Create.Entity.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Abc.Create.Entity.Roster(rosterId: Guid.NewGuid(), rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: titleQuestionId, variable: "ros1",
                    children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericRealQuestion(id: titleQuestionId, variable: "link_source")
                    })
            });

            var interview = SetupInterview(questionnaireDocument: questionnaireDocument);

            interview.AnswerNumericIntegerQuestion(userId: userId, questionId: triggerQuestionId,
                originDate: DateTimeOffset.Now, rosterVector: new decimal[0], answer: 1);
            interview.AnswerNumericRealQuestion(userId: userId, questionId: titleQuestionId,
                originDate: DateTimeOffset.Now, rosterVector: new decimal[] {0}, answer: 2.3);

            var exception = Assert.Throws<InterviewException>(() => interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                originDate: DateTimeOffset.Now, rosterVector: new decimal[0], selectedRosterVector: answer));

            Assert.That(exception, Has.Property(nameof(exception.Message)).EqualTo("Answer on linked categorical question cannot be saved. Specified option is absent"));
        }

        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] answer = { 1 };
    }
}

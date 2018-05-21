using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_get_rejected_interview__with_linked_question : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var textListId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            singleLinkedId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.TextListQuestion(textListId, variable: "textList"),
                Create.Entity.SingleOptionQuestion(singleLinkedId, "single", linkedToQuestionId: textListId),
            });

            interview = Setup.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.InterviewSynchronized(
                Create.Entity.InterviewSynchronizationDto(answers: new AnsweredQuestionSynchronizationDto[]
                {
                    new AnsweredQuestionSynchronizationDto(textListId, new decimal[0], new Tuple<decimal, string>[]
                    {
                        new Tuple<decimal, string>(1m, "1"), 
                        new Tuple<decimal, string>(2m, "2"), 
                        new Tuple<decimal, string>(3m, "3"), 
                    }, new CommentSynchronizationDto[0], null),
                    new AnsweredQuestionSynchronizationDto(singleLinkedId, new decimal[0], 2m, new CommentSynchronizationDto[0], null),     
                })));
            BecauseOf();
        }

        private void BecauseOf() => linkedToListQuestion = interview.GetSingleOptionLinkedToListQuestion(Create.Entity.Identity(singleLinkedId));

        [NUnit.Framework.Test] public void should_set_linked_answer () => linkedToListQuestion.GetAnswer().SelectedValue.Should().Be(2);

        private static StatefulInterview interview;
        private static Guid singleLinkedId;
        private static InterviewTreeSingleOptionLinkedToListQuestion linkedToListQuestion;
    }
}


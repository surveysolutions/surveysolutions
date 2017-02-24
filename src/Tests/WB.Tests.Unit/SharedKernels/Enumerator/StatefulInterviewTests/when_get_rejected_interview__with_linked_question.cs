using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_get_rejected_interview__with_linked_question : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
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
                    }, new CommentSynchronizationDto[0]),
                    new AnsweredQuestionSynchronizationDto(singleLinkedId, new decimal[0], 2m, new CommentSynchronizationDto[0]),     
                })));
        };

        Because of = () => linkedToListQuestion = interview.GetSingleOptionLinkedToListQuestion(Create.Entity.Identity(singleLinkedId));

        It should_set_linked_answer = () 
            => linkedToListQuestion.GetAnswer().SelectedValue.ShouldEqual(2);

        private static StatefulInterview interview;
        private static Guid singleLinkedId;
        private static InterviewTreeSingleOptionLinkedToListQuestion linkedToListQuestion;
    }
}


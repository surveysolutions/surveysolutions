using System;
using System.Collections.Generic;
using Machine.Specifications;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_chagned : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            linkSourceQuestionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();

            eventData = new[]
            {
                new ChangedLinkedOptions(linkedQuestionId, 
                    new[]
                    {
                        Create.Entity.RosterVector(1),
                        Create.Entity.RosterVector(2)
                    }),
            };

            linkedOptionTextInInterview = "answer in init";

            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                    .Returns(new List<BaseInterviewAnswer> { Create.Entity.InterviewTreeTextQuestion(linkedOptionTextInInterview) });

            IQuestionnaire questionnaire = Substitute.For<IQuestionnaire>();
            questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestionId.Id)
                         .Returns(linkSourceQuestionId.Id);

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: questionnaire);
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());

            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                  .Returns(new List<BaseInterviewAnswer>
                  {
                      Create.Entity.InterviewTreeTextQuestion("one"),
                      Create.Entity.InterviewTreeTextQuestion("two")
                  });
        };

        Because of = () => viewModel.Handle(Create.Event.LinkedOptionsChanged(eventData));

        It should_replace_answer_title_from_add_provided_by_event_options_to_self = () => viewModel.Options.Count.ShouldEqual(2);

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Identity linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static ChangedLinkedOptions[] eventData;
        static string linkedOptionTextInInterview;
    }
}
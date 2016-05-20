using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_chagned_and_some_existing_were_answered : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            linkSourceQuestionId = Create.Other.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Other.RosterVector(1));
            linkedQuestionId = Create.Other.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();

            eventData = new[]
            {
                new ChangedLinkedOptions(linkedQuestionId, 
                    new[]
                    {
                        Create.Other.RosterVector(1),
                        Create.Other.RosterVector(2),
                        Create.Other.RosterVector(3)
                    }),
            };

            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                  .Returns(new List<BaseInterviewAnswer>
                  {
                      Create.Other.TextAnswer("two", 
                        questionId: linkSourceQuestionId.Id, 
                        rosterVector: Create.Other.RosterVector(2))
                  });

            IQuestionnaire questionnaire = SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(linkedQuestionId.Id, linkSourceQuestionId.Id);

            viewModel = Create.Other.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: questionnaire);
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());

            viewModel.Options.First().Selected = true;

            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                 .Returns(new List<BaseInterviewAnswer>
                 {
                      Create.Other.TextAnswer("one",
                        questionId: linkSourceQuestionId.Id,
                        rosterVector: Create.Other.RosterVector(1)),
                      Create.Other.TextAnswer("two",
                        questionId: linkSourceQuestionId.Id,
                        rosterVector: Create.Other.RosterVector(2)),
                        Create.Other.TextAnswer("three",
                        questionId: linkSourceQuestionId.Id,
                        rosterVector: Create.Other.RosterVector(3))
                 });
        };

        Because of = () => viewModel.Handle(Create.Event.LinkedOptionsChanged(eventData));

        It should_not_remove_existing_selected_option = () => viewModel.Options.Second().Selected.ShouldBeTrue();

        It should_add_new_options_as_non_selected = () => viewModel.Options.First().Selected.ShouldBeFalse();
        
        It should_add_all_options_from_event = () => viewModel.Options.Count.ShouldEqual(3);

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Identity linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static ChangedLinkedOptions[] eventData;
    }
}
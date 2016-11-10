using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class when_linked_options_changed_with_reduced_amound_of_items : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            linkSourceQuestionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Entity.RosterVector(1));
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();

            eventData = new[]
            {
                new ChangedLinkedOptions(linkedQuestionId,
                    new[]
                    {
                        Create.Entity.RosterVector(1)
                    }),
            };

            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                  .Returns(new List<BaseInterviewAnswer>
                  {
                      Create.Entity.InterviewTreeTextQuestion("one",
                        questionId: linkSourceQuestionId.Id,
                        rosterVector: Create.Entity.RosterVector(1)),
                       Create.Entity.InterviewTreeTextQuestion("two",
                        questionId: linkSourceQuestionId.Id,
                        rosterVector: Create.Entity.RosterVector(2))
                  });

            interview.FindBaseAnswerByOrDeeperRosterLevel(linkSourceQuestionId.Id, Create.Entity.RosterVector(1))
                     .Returns(Create.Entity.InterviewTreeTextQuestion("one"));

            IQuestionnaire questionnaire = SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(linkedQuestionId.Id, linkSourceQuestionId.Id);

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: questionnaire);
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());

            viewModel.Options.First().Selected = true;

            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                 .Returns(new List<BaseInterviewAnswer>
                 {
                      Create.Entity.InterviewTreeTextQuestion("one",
                        questionId: linkSourceQuestionId.Id,
                        rosterVector: Create.Entity.RosterVector(1)),
                 });
        };

        Because of = () => viewModel.Handle(Create.Event.LinkedOptionsChanged(eventData));

        It should_synchronize_visible_options_with_event_data = () => viewModel.Options.Count.ShouldEqual(1);
        It should_keep_not_removed_options_as_they_were = () => viewModel.Options.First().Title.ShouldEqual("one");
        It should_not_remove_selected_option = () => viewModel.Options.First().Selected.ShouldBeTrue();

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Identity linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static ChangedLinkedOptions[] eventData;
    }
}
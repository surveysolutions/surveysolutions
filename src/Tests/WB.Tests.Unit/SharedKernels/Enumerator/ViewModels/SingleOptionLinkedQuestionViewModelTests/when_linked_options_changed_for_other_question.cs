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

using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_changed_for_other_question : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            linkSourceQuestionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();

            eventData = new[]
            {
                new ChangedLinkedOptions(Create.Entity.Identity(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), RosterVector.Empty),
                    new[]
                    {
                        Create.Entity.RosterVector(1)
                    }),
            };

            linkedOptionTextInInterview = "answer in init";
            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceQuestionId.Id, linkedQuestionId)
                     .Returns(new List<BaseInterviewAnswer> { Create.Entity.InterviewTreeTextQuestion(linkedOptionTextInInterview) });

            interview.FindBaseAnswerByOrDeeperRosterLevel(linkSourceQuestionId.Id, Create.Entity.RosterVector(1))
                     .Returns(Create.Entity.InterviewTreeTextQuestion("answer in event"));

            IQuestionnaire questionnaire = 
                SetupQuestionnaireWithSingleOptionQuestionLinkedToTextQuestion(linkedQuestionId.Id, linkSourceQuestionId.Id);
            
                viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: questionnaire);
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());
        };

        Because of = () => viewModel.Handle(Create.Event.LinkedOptionsChanged(eventData));

        It should_not_modify_list_of_options = () => viewModel.Options.Count.ShouldEqual(1);

        It should_not_modify_option_title = () => viewModel.Options.First().Title.ShouldEqual(linkedOptionTextInInterview);

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Identity linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
        static ChangedLinkedOptions[] eventData;
        static string linkedOptionTextInInterview;
    }
}
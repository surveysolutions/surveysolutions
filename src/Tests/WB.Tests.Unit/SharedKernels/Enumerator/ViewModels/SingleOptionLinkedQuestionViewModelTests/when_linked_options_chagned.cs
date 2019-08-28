using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_linked_options_chagned : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkSourceQuestionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            linkedQuestionId = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").FormatGuid();

            var answer = Mock.Of<InterviewTreeSingleLinkedToRosterQuestion>(_ 
                => _.IsAnswered() == true
                && _.GetAnswer() == Create.Entity.LinkedSingleOptionAnswer(Create.Entity.RosterVector(2))
                && _.Options == new List<RosterVector> { Create.Entity.RosterVector(1), Create.Entity.RosterVector(2), Create.Entity.RosterVector(3)});

            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.GetLinkedSingleOptionQuestion(linkedQuestionId).Returns(answer);

            IQuestionnaire questionnaire = Substitute.For<IQuestionnaire>();
            questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestionId.Id)
                         .Returns(linkSourceQuestionId.Id);

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(interview: interview, questionnaire: questionnaire);
            viewModel.Init(interviewId, linkedQuestionId, Create.Other.NavigationState());

            var newAnswer = Mock.Of<InterviewTreeSingleLinkedToRosterQuestion>(_
               => _.IsAnswered() == false
               && _.Options == new List<RosterVector> { Create.Entity.RosterVector(1), Create.Entity.RosterVector(2) });

            interview.GetLinkedSingleOptionQuestion(linkedQuestionId).Returns(newAnswer);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.HandleAsync(Create.Event.LinkedOptionsChanged(new[]
        {
            new ChangedLinkedOptions(linkedQuestionId, new[] { Create.Entity.RosterVector(1), Create.Entity.RosterVector(2) }),
        }));

        [NUnit.Framework.Test] public void should_replace_answer_title_from_add_provided_by_event_options_to_self () => viewModel.Options.Count.Should().Be(2);

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Identity linkSourceQuestionId;
        static Identity linkedQuestionId;
        static string interviewId;
    }
}

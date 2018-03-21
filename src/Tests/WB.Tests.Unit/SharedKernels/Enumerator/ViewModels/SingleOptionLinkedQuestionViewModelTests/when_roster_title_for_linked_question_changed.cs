using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_roster_title_for_linked_question_changed : SingleOptionLinkedQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var level1TriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var level2triggerId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var linkToQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var linkedQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            topRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var questionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);
            var sourceIdentity = Create.Entity.Identity(linkToQuestionId, Create.Entity.RosterVector(1));

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId: level1TriggerId),
                Create.Entity.Roster(rosterSizeQuestionId: level1TriggerId, rosterId: topRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.Entity.TextListQuestion(questionId: level2triggerId),
                    Create.Entity.Roster(rosterSizeQuestionId: level2triggerId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: linkToQuestionId)
                    })
                }),
                Create.Entity.SingleOptionQuestion(questionId: linkedQuestionId, linkedToQuestionId: linkToQuestionId));

            var newAnswer = Mock.Of<InterviewTreeSingleLinkedToRosterQuestion>(_
                => _.IsAnswered() == false
                   && _.Options == new List<RosterVector> { Create.Entity.RosterVector(1) });
            var interview = Substitute.For<IStatefulInterview>();

            interview.GetLinkedSingleOptionQuestion(questionIdentity).Returns(newAnswer);
            interview.GetAnswerAsString(sourceIdentity).Returns("subtitle");
            interview.GetLinkedOptionTitle(questionIdentity, Create.Entity.RosterVector(1)).Returns("title: subtitle");

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(interview.Id.FormatGuid(), questionIdentity, Create.Other.NavigationState());

            BecauseOf();
        }

        public void BecauseOf() => 
            viewModel.Handle(Create.Event.RosterInstancesTitleChanged(rosterId: topRosterId));

        [NUnit.Framework.Test] public void should_refresh_list_of_options () => viewModel.Options.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_prefix_option_with_parent_title () => viewModel.Options.First().Title.Should().Be("title: subtitle");

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid topRosterId;
    }
}

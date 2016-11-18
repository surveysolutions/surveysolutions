using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    internal class when_roster_title_for_linked_question_changed : SingleOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
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
                Create.Entity.SingleOptionQuestion(questionId: linkedQuestionId, linkedToQuestionId: linkToQuestionId)
                );

            var newAnswer = Mock.Of<InterviewTreeSingleLinkedToRosterQuestion>(_
                => _.IsAnswered == false
                   && _.Options == new List<RosterVector> { Create.Entity.RosterVector(1) });
            var interview = Substitute.For<IStatefulInterview>();

            interview.GetLinkedSingleOptionQuestion(questionIdentity).Returns(newAnswer);
            interview.GetAnswerAsString(sourceIdentity).Returns("subtitle");
            interview.GetParentRosterTitlesWithoutLast(sourceIdentity).Returns(new string[] { "bla", "title" });

            viewModel = Create.ViewModel.SingleOptionLinkedQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(interview.Id.FormatGuid(), questionIdentity, Create.Other.NavigationState());
        };

        Because of = () => viewModel.Handle(Create.Event.RosterInstancesTitleChanged(rosterId: topRosterId));

        It should_refresh_list_of_options = () => viewModel.Options.Count.ShouldEqual(1);

        It should_prefix_option_with_parent_title = () => viewModel.Options.First().Title.ShouldEqual("title: subtitle");

        static SingleOptionLinkedQuestionViewModel viewModel;
        static Guid topRosterId;
    }
}
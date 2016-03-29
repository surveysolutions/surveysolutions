using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionRosterLinkedQuestionViewModelTests
{
    internal class when_question_is_linked_to_second_level_question_and_title_changes : SingleOptionRosterLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var linkToRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            parentRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionIdentity = Create.Identity(questionId, RosterVector.Empty);

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[] {
                Create.FixedRoster(
                    rosterId: parentRosterId,
                    children: new IComposite[] {
                        Create.FixedRoster(rosterId: linkToRosterId)
                }),
                Create.SingleOptionQuestion(
                    questionId: questionId,
                    linkedToRosterId: linkToRosterId)
            });

            IStatefulInterview interview = Substitute.For<IStatefulInterview>();

            var questionnaireRepository = Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaire.PublicKey,
                Create.PlainQuestionnaire(questionnaire));
            var interviewRepository = Create.StatefulInterviewRepositoryWith(interview);

            viewModel = CreateViewModel(interviewRepository, questionnaireRepository);
            viewModel.InitAsync("interview", questionIdentity, Create.NavigationState(interviewRepository)).WaitAndUnwrapException();

            interview.FindReferencedRostersForLinkedQuestion(linkToRosterId, questionIdentity)
                .Returns(new List<InterviewRoster> { Create.InterviewRoster(rosterId: linkToRosterId, rosterTitle: "title1")});
            interview.GetParentRosterTitlesWithoutLastForRoster(linkToRosterId, RosterVector.Empty)
                .ReturnsForAnyArgs(new List<string>() {"item"});
        };

        Because of = () => viewModel.Handle(Create.Event.RosterInstancesTitleChanged(parentRosterId, Create.RosterVector(1, 2)));

        It should_refresh_roster_titles_in_options = () => viewModel.Options.FirstOrDefault().Title.ShouldEqual("item: title1");

        static SingleOptionRosterLinkedQuestionViewModel viewModel;
        static Guid parentRosterId;
    }
}
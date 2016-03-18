using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_roster_titles_changed : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var level1TriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var level2triggerId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var linkToQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var linkedQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            topRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var questionIdentity = new Identity(linkedQuestionId, RosterVector.Empty);

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.TextListQuestion(questionId: level1TriggerId),
                Create.Roster(rosterSizeQuestionId: level1TriggerId, rosterId: topRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.TextListQuestion(questionId: level2triggerId),
                    Create.Roster(rosterSizeQuestionId: level2triggerId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.TextQuestion(questionId: linkToQuestionId)
                    })
                }),
                Create.MultipleOptionsQuestion(questionId: linkedQuestionId, linkedToQuestionId: linkToQuestionId)
                );

            var interview = Substitute.For<IStatefulInterview>();
            interview.GetParentRosterTitlesWithoutLast(linkToQuestionId, Create.RosterVector(1, 1))
                .Returns(new List<string> { "title" });
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkToQuestionId, questionIdentity)
                .Returns(new[] { Create.TextAnswer("subtitle", linkToQuestionId, Create.RosterVector(1, 1)) });

            viewModel = CreateViewModel(Create.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(interview.Id.FormatGuid(), questionIdentity, Create.NavigationState());
        };

        Because of = () => viewModel.Handle(Create.RosterInstancesTitleChanged(rosterId: topRosterId));

        It should_refresh_list_of_options = () => viewModel.Options.Count.ShouldEqual(1);

        It should_prefix_option_with_parent_title = () => viewModel.Options.First().Title.ShouldEqual("title: subtitle");

        static MultiOptionLinkedToQuestionQuestionViewModel viewModel;
        static Guid topRosterId;
    }
}
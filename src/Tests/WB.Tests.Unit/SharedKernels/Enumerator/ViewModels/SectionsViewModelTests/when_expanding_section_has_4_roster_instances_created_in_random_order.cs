using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_expanding_section_has_4_roster_instances_created_in_random_order : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.TextListQuestion(listQuestionId),
                Create.Entity.ListRoster(listRosterId, rosterSizeQuestionId: listQuestionId),
                Create.Entity.MultyOptionsQuestion(multiQuestionId, options: Create.Entity.Options(1, 2, 3)),
                Create.Entity.MultiRoster(multiRosterId, rosterSizeQuestionId: multiQuestionId));

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId, Guid.NewGuid(), plainQuestionnaire);
            interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new []
            {
                new Tuple<decimal, string>(1, "List 1")
            });
            interview.AnswerMultipleOptionsQuestion(userId, multiQuestionId, RosterVector.Empty, DateTime.Now, new [] { 1 });
            interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new []
            {
                new Tuple<decimal, string>(1, "List 1"),
                new Tuple<decimal, string>(2, "List 2")
            });
            interview.AnswerMultipleOptionsQuestion(userId, multiQuestionId, RosterVector.Empty, DateTime.Now, new [] { 1, 2 });

            navigationState = Substitute.For<NavigationState>();
            viewModel = CreateSectionsViewModel(plainQuestionnaire, interview);
            viewModel.Init(interview.Id.FormatGuid(), Create.Entity.QuestionnaireIdentity(questionnaireId, 1), navigationState);
        };

        Because of = () =>
        {
            navigationState.ScreenChanged += Raise.Event<GroupChanged>(Create.Entity.ScreenChangedEventArgs(targetStage: ScreenType.Group, targetGroup: Create.Entity.Identity(chapterId)));
        };

        It should_add_sections_children_in_questionnaire_order = () =>
        {
            List<SideBarSectionViewModel> rosters = viewModel.Sections.ElementAt(1).Children;

            rosters[0].SectionIdentity.ShouldEqual(Create.Entity.Identity(listRosterId, Create.Entity.RosterVector(1)));
            rosters[1].SectionIdentity.ShouldEqual(Create.Entity.Identity(listRosterId, Create.Entity.RosterVector(2)));
            rosters[2].SectionIdentity.ShouldEqual(Create.Entity.Identity(multiRosterId, Create.Entity.RosterVector(1)));
            rosters[3].SectionIdentity.ShouldEqual(Create.Entity.Identity(multiRosterId, Create.Entity.RosterVector(2)));
        };

        static SideBarSectionsViewModel viewModel;
        static NavigationState navigationState;

        private static StatefulInterview interview;
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static readonly Guid listQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid listRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid multiQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid multiRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid chapterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}
using System;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_sections_model_has_disabled_items : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(Guid.NewGuid(),
                Create.Entity.Group(sectionAId),
                Create.Entity.Group(sectionBId),
                Create.Entity.Group(sectionCId),
                Create.Entity.Group(sectionDId, title: "D"));
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var interview = Setup.StatefulInterview(questionnaire);

            interview.Apply(Create.Event.GroupsDisabled(new[]
            {
                Identity.Create(sectionBId, RosterVector.Empty),
                Identity.Create(sectionCId, RosterVector.Empty),
                Identity.Create(sectionDId, RosterVector.Empty)
            }));

            viewModel = CreateSectionsViewModel(plainQuestionnaire, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", Create.Entity.QuestionnaireIdentity(), navigationState);

            viewModel.Handle(Create.Event.GroupsDisabled(sectionBId, Empty.RosterVector));
            viewModel.Handle(Create.Event.GroupsDisabled(sectionCId, Empty.RosterVector));
            viewModel.Handle(Create.Event.GroupsDisabled(sectionDId, Empty.RosterVector));

            interview.Apply(Create.Event.GroupsEnabled(new[]
            {
                Identity.Create(sectionDId, RosterVector.Empty),
            }));
        };

        Because of = () =>
        {
            viewModel.Handle(Create.Event.GroupsEnabled(sectionDId, Empty.RosterVector));
        };

        It should_contains_two_sections_and_complete_and_cover_button = () =>
            viewModel.Sections.Count.ShouldEqual(2 + 1 + 1);

        It should_add_section_D_at_third_position = () =>
        {
            viewModel.Sections.ElementAt(2).SectionIdentity.Id.Equals(sectionDId);
            viewModel.Sections.ElementAt(2).SectionIdentity.RosterVector.Equals(Empty.RosterVector);
            viewModel.Sections.ElementAt(2).Title.PlainText.ShouldEqual("D");
        };

        static SideBarSectionsViewModel viewModel;
        static NavigationState navigationState;

        static readonly Guid sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid sectionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid sectionDId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
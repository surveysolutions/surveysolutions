using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_handling_groupEnabledEvent_and_roster_is_not_present : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(Guid.NewGuid(),
                Create.Entity.Group(groupId: sectionAId, children: new List<IComposite>()
                {
                    Create.Entity.NumericIntegerQuestion(questionCId),
                    Create.Entity.Roster(rosterBId, rosterSizeQuestionId:questionCId)
                }
                ));

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var interview = Setup.StatefulInterview(questionnaire);

            viewModel = CreateSectionsViewModel(plainQuestionnaire, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", Create.Entity.QuestionnaireIdentity(), navigationState);
        };

        Because of = () =>
        {
            viewModel.Handle(Create.Event.GroupsEnabled(new[]
            {
                Identity.Create(rosterBId, new RosterVector(new decimal[] {1}))
            }));
        };

        It should_have_3_section = () =>
            viewModel.Sections.Count.ShouldEqual(3);

        
        static SideBarSectionsViewModel viewModel;
        static NavigationState navigationState;

        static readonly Guid sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
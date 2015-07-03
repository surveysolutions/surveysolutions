using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_roster_instances_added : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            QuestionnaireModel questionnaire = Create.QuestionnaireModel();
            var sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire.GroupsHierarchy = new List<GroupsHierarchyModel>
            {
                new GroupsHierarchyModel
                {
                    Title = "Section 1",
                    Id = sectionId,
                    Children = new List<GroupsHierarchyModel>
                    {
                        new GroupsHierarchyModel
                        {
                            Title = "Roster 1",
                            Id = rosterId,
                            IsRoster = true
                        }
                    }
                }
            };

            questionnaire.GroupsParentIdMap = new Dictionary<Guid, Guid?>
            {
                {
                    rosterId, sectionId
                }
            };

            interview = Substitute.For<IStatefulInterview>();
            interview.GetEnabledGroupInstances(sectionId, Arg.Any<decimal[]>())
                .Returns(new List<Identity> { new Identity(sectionId, Empty.RosterVector) });

            interview.GetEnabledGroupInstances(rosterId, Arg.Any<decimal[]>())
                .Returns(new List<Identity> {
                    new Identity(rosterId, new []{1m}),
                    new Identity(rosterId, new []{2m})
                });

            viewModel = CreateSectionsViewModel(questionnaire, interview);
            viewModel.Init("", "", Create.NavigationState());
        };

        Because of = () =>
        {
            interview.GetEnabledGroupInstances(rosterId, Arg.Any<decimal[]>())
               .Returns(new List<Identity> {
                    new Identity(rosterId, new []{1m}),
                    new Identity(rosterId, new []{2m}),
                    new Identity(rosterId, new []{3m})
                });
            viewModel.Handle(Create.Event.RosterInstancesAdded(rosterId, new[] {3m}));
        };

        It should_add_roster_into_a_tree = () => viewModel.Sections.First().Children.Count.ShouldEqual(3);

        static SideBarSectionsViewModel viewModel;
        static Guid rosterId;
        static IStatefulInterview interview;
    }
}


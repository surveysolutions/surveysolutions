using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
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
            group1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            group2Id = Guid.Parse("11111111111111111111111111111111");
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
                            Title = "Group 1",
                            Id = group1Id
                        },
                        new GroupsHierarchyModel
                        {
                            Title = "Roster 1",
                            Id = rosterId,
                            IsRoster = true
                        },
                        new GroupsHierarchyModel
                        {
                            Title = "Group 2",
                            Id = group2Id
                        }
                    }
                }
            };
            questionnaire.GroupsWithFirstLevelChildrenAsReferences = new Dictionary<Guid, GroupModel>();
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[rosterId] = new RosterModel
            {
                Title = "Roster 1",
                Id = rosterId
            };
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[group1Id] = new GroupModel
            {
                Title = "Group 1",
                Id = group1Id
            };
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[group2Id] = new GroupModel
            {
                Title = "Group 2",
                Id = group2Id
            };
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionId] = new GroupModel
            {
                Title = "Section 1",
                Id = sectionId
            };

            questionnaire.GroupsParentIdMap = new Dictionary<Guid, Guid?> {{ rosterId, sectionId }};

            interview = Substitute.For<IStatefulInterview>();
            sectionIdentity = new Identity(sectionId, Empty.RosterVector);
            addedInstanceIdentity = new Identity(rosterId, new[] {3m});
            interview.GetParentGroup(addedInstanceIdentity)
                .Returns(sectionIdentity);

            interview.GetEnabledSubgroups(sectionIdentity)
               .Returns(new List<Identity> {
                    new Identity(group1Id, Empty.RosterVector),
                    new Identity(rosterId, new []{0m}),
                    new Identity(rosterId, new []{1m}),
                    new Identity(group2Id, Empty.RosterVector)
                });

            viewModel = CreateSectionsViewModel(questionnaire, interview);
            viewModel.Init("", "", Create.NavigationState());
            viewModel.Sections[0].SectionIdentity = new Identity(sectionId, Empty.RosterVector);
        };

        Because of = () =>
        {
             interview.GetEnabledSubgroups(sectionIdentity)
               .Returns(new List<Identity> {
                    new Identity(group1Id, Empty.RosterVector),
                    new Identity(rosterId, new []{0m}),
                    new Identity(rosterId, new []{1m}),
                    new Identity(rosterId, new []{2m}),
                    new Identity(group2Id, Empty.RosterVector)
                });
            viewModel.Handle(Create.Event.RosterInstancesAdded(addedInstanceIdentity.Id, addedInstanceIdentity.RosterVector));
        };

        It should_add_roster_into_a_tree = () => viewModel.Sections.First().Children.Count.ShouldEqual(5);

        static SideBarSectionsViewModel viewModel;
        static Guid rosterId;
        static IStatefulInterview interview;
        private static Guid group2Id;
        private static Guid group1Id;
        private static Identity sectionIdentity;
        private static Identity addedInstanceIdentity;
    }
}


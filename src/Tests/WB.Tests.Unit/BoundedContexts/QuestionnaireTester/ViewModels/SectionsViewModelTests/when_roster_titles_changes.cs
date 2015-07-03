using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_roster_titles_changed : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            QuestionnaireModel questionnaire = Create.QuestionnaireModel();
            var sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterGroupName = "Roster 1";
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
                            Title = rosterGroupName,
                            Id = rosterId,
                            IsRoster = true
                        }
                    }
                }
            };

            questionnaire.GroupsWithFirstLevelChildrenAsReferences = new Dictionary<Guid, GroupModel>
            {
                { rosterId, new GroupModel {Title = rosterGroupName} }
            };


            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.GetEnabledGroupInstances(sectionId, Arg.Any<decimal[]>())
                .Returns(new List<Identity> { new Identity(sectionId, Empty.RosterVector) });

            changedRosterIdentity = new Identity(rosterId, new []{1m});
            interview.GetEnabledGroupInstances(rosterId, Arg.Any<decimal[]>())
                .Returns(new List<Identity> {
                    changedRosterIdentity,
                    new Identity(rosterId, new []{2m})
                });

            viewModel = CreateSectionsViewModel(questionnaire, interview);
            viewModel.Init("", "", Create.NavigationState());
        };

        Because of = () => viewModel.Handle(Create.Event.RosterInstancesTitleChanged(changedRosterIdentity.Id, changedRosterIdentity.RosterVector, "answer 1"));

        It should_substitute_roster_title = () => viewModel.Sections.First().Children[0].Title.ShouldEqual("Roster 1 - answer 1");

        private static SectionsViewModel viewModel;
        private static Identity changedRosterIdentity;
    }
}


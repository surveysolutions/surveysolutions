using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_initing : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            QuestionnaireModel questionnaire = Create.QuestionnaireModel();
            var sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire.GroupsHierarchy = new List<GroupsHierarchyModel>();
            questionnaire.GroupsHierarchy.Add(new GroupsHierarchyModel
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
            });

            IStatefulInterview interview = Substitute.For<IStatefulInterview>();
            interview.GetGroupInstances(sectionId, Arg.Any<decimal[]>())
                .Returns(new List<Identity> {new Identity(sectionId, Empty.RosterVector)});

            interview.GetGroupInstances(rosterId, Arg.Any<decimal[]>())
                .Returns(new List<Identity> {
                    new Identity(rosterId, new []{1m}),
                    new Identity(rosterId, new []{2m})
                });

            viewModel = CreateSectionsViewModel(questionnaire, interview);
        };

        Because of = () => viewModel.Init("questionnaire", "id", Create.NavigationState());

        It should_fill_groups_tree = () =>
        {
            viewModel.Sections.Count.ShouldEqual(1);
            var firstSection = viewModel.Sections.First();
            firstSection.Children.Count.ShouldEqual(2);
            firstSection.Children.First().SectionIdentity.RosterVector.Identical(new[] {1m});
        };

        static SectionsViewModel viewModel;
    }
}


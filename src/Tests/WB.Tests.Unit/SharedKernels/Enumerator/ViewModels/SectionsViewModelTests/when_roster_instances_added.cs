using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_roster_instances_added : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
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
            interview.IsEnabled(Moq.It.IsAny<Identity>()).ReturnsForAnyArgs(true);

            var questionnaire = Mock.Of<IQuestionnaire>(_ 
                => _.GetAllSections() == listOfSections);

            viewModel = CreateSectionsViewModel(questionnaire, interview);
            viewModel.Init("", Create.Entity.QuestionnaireIdentity(), Create.Other.NavigationState());
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

        It should_add_roster_into_a_tree = () => 
            viewModel.Sections.Second().Children.Count.ShouldEqual(5);

        static SideBarSectionsViewModel viewModel;
        static IStatefulInterview interview;
        private static Identity sectionIdentity;
        private static Identity addedInstanceIdentity;
        private static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid group1Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid group2Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly List<Guid> listOfSections = new List<Guid> { sectionId };
    }
}


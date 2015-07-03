using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_navigating_to_some_subsection_that_is_not_section : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            selectedGroupIdentity = Create.Identity(Section2Id, Empty.RosterVector);
            someGroupThatIsNotSectionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var section2 = CreateGroupsHierarchyModel(Section2Id, "Section 2");
            section2.Children.Add(CreateGroupsHierarchyModel(someGroupThatIsNotSectionId, "title"));
            List<GroupsHierarchyModel> listOfSection = new List<GroupsHierarchyModel>
                                                                           {
                                                                               CreateGroupsHierarchyModel(Guid.Parse("11111111111111111111111111111111"),"Section 1"),
                                                                               section2,
                                                                               CreateGroupsHierarchyModel(Guid.Parse("33333333333333333333333333333333"),"Section 3"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("44444444444444444444444444444444"),"Section 4")
                                                                           };

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.GroupsHierarchy == listOfSection);

            Section2Id = Guid.Parse("22222222222222222222222222222222");
            navigationState = Create.NavigationState();

            var questionnaireRepositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireRepositoryMock
                .Setup(x => x.GetById("questionnaire Id"))
                .Returns(questionnaireModel);

            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.IsEnabled(Moq.It.IsAny<Identity>()))
                .Returns(true);

            sectionsModel = CreateSectionsViewModel(questionnaireModel, interview.Object);

            sectionsModel.Init("questionnaire Id", "id", navigationState);

            navigationState.NavigateTo(selectedGroupIdentity).WaitAndUnwrapException();
        };

        Because of = () =>
            navigationState.NavigateTo(Create.Identity(someGroupThatIsNotSectionId, Empty.RosterVector)).WaitAndUnwrapException();

        It should_mark_one_section_as_selected = () =>
            sectionsModel.Sections.Count(x => x.IsSelected).ShouldEqual(1);

        It should_not_change_selected_section = () =>
            sectionsModel.Sections.First(x => x.IsSelected).SectionIdentity.ShouldEqual(selectedGroupIdentity);

        private static SideBarSectionsViewModel sectionsModel;
        private static NavigationState navigationState;
        private static Guid Section2Id;
        private static Guid someGroupThatIsNotSectionId;
        private static Identity selectedGroupIdentity;
    }
}
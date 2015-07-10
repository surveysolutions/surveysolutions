using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Specifications;

using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_navigating_to_selected_section : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.GroupsHierarchy == listOfSection);

            questionnaireRepositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireModel);

            sectionsModel = CreateSectionsViewModel(questionnaireRepository: questionnaireRepositoryMock.Object);

            sectionsModel.Init(questionnaireId, "id", navigationState);

            navigationState.NavigateTo(selectedGroupIdentity).WaitAndUnwrapException();
        };

        Because of = () =>
            navigationState.NavigateTo(selectedGroupIdentity).WaitAndUnwrapException();

        It should_mark_one_section_as_selected = () =>
            sectionsModel.Sections.Count(x => x.IsSelected).ShouldEqual(1);

        It should_not_change_selected_section = () =>
            sectionsModel.Sections.First(x => x.IsSelected).SectionIdentity.ShouldEqual(selectedGroupIdentity);

        private static SideBarSectionsViewModel sectionsModel;
        private const string questionnaireId = "questionnaire Id";
        private static readonly NavigationState navigationState = Create.NavigationState(interviewRepository: Setup.StatefulInterviewRepositoryWithInterviewsWithAllGroupsEnabledAndExisting());
        private static readonly Mock<IPlainKeyValueStorage<QuestionnaireModel>> questionnaireRepositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
        private static readonly Guid Section2Id = Guid.Parse("22222222222222222222222222222222");

        private static readonly List<GroupsHierarchyModel> listOfSection = new List<GroupsHierarchyModel>
                                                                           {
                                                                               CreateGroupsHierarchyModel(Guid.Parse("11111111111111111111111111111111"),"Section 1"),
                                                                               CreateGroupsHierarchyModel(Section2Id,"Section 2"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("33333333333333333333333333333333"),"Section 3"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("44444444444444444444444444444444"),"Section 4")
                                                                           };

        private static readonly Identity selectedGroupIdentity = Create.Identity(Section2Id, new decimal[0]);
    }
}
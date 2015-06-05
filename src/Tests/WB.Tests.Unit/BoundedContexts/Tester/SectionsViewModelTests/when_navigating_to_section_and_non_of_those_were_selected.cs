using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_navigating_to_section_and_non_of_those_were_selected : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.GroupsHierarchy == listOfSection);

            questionnaireRepositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireModel);

            sectionsModel = CreateSectionsViewModel(questionnaireRepository: questionnaireRepositoryMock.Object);

            sectionsModel.Init(questionnaireId, navigationState);
        };

        Because of = () => 
            navigationState.NavigateTo(toBeSelectedGroupIdentity);

        It should_mark_one_section_as_selected = () =>
            sectionsModel.Sections.Count(x => x.IsSelected).ShouldEqual(1);

        It should_select_section_that_was_navigated_to = () =>
            sectionsModel.Sections.First(x => x.IsSelected).SectionIdentity.ShouldEqual(toBeSelectedGroupIdentity);

        private static SectionsViewModel sectionsModel;
        private const string questionnaireId = "questionnaire Id";
        private static readonly NavigationState navigationState = CreateNavigationState();
        private static readonly Mock<IPlainKeyValueStorage<QuestionnaireModel>> questionnaireRepositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
        private static readonly Guid Section2Id = Guid.Parse("22222222222222222222222222222222");

        private static readonly List<GroupsHierarchyModel> listOfSection = new List<GroupsHierarchyModel>
                                                                           {
                                                                               CreateGroupsHierarchyModel(Guid.Parse("11111111111111111111111111111111"),"Section 1"),
                                                                               CreateGroupsHierarchyModel(Section2Id,"Section 2"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("33333333333333333333333333333333"),"Section 3"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("44444444444444444444444444444444"),"Section 4")
                                                                           };

        private static readonly Identity toBeSelectedGroupIdentity = Create.Identity(Section2Id, new decimal[0]);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_initializing_sections_view_model : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.GroupsHierarchy == listOfSection);

            questionnaireRepositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireModel);

            sectionsModel = CreateSectionsViewModel(questionnaireRepository: questionnaireRepositoryMock.Object);
        };

        Because of = () => 
            sectionsModel.Init(questionnaireId, "id", navigationState);

        It should_get_questionnaire_by_id_once = () =>
            questionnaireRepositoryMock.Verify(x => x.GetById(questionnaireId), Times.Once);

        It should_initialize_section_list = () =>
            sectionsModel.Sections.ShouldNotBeEmpty();

        It should_create_the_same_amount_of_sections_as_in_questionnaire_model = () =>
            sectionsModel.Sections.Count.ShouldEqual(listOfSection.Count);

        It should_section_views_with_empty_roster_vector = () =>
            sectionsModel.Sections.Sum(x => x.SectionIdentity.RosterVector.Length).ShouldEqual(0);

        It should_section_views_same_ids_as_in_questionnaire_model = () =>
            sectionsModel.Sections.Select(x => x.SectionIdentity.Id).ShouldContainOnly(listOfSection.Select(x =>x.Id));

        It should_section_views_same_titles_as_in_questionnaire_model = () =>
            sectionsModel.Sections.Select(x => x.Title).ShouldContainOnly(listOfSection.Select(x => x.Title));

        static SideBarSectionsViewModel sectionsModel;

        private const string questionnaireId = "questionnaire Id";
        private static readonly NavigationState navigationState = Mock.Of<NavigationState>();
        private static readonly Mock<IPlainKeyValueStorage<QuestionnaireModel>> questionnaireRepositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();

        private static readonly List<GroupsHierarchyModel> listOfSection = new List<GroupsHierarchyModel>
                                                                           {
                                                                               CreateGroupsHierarchyModel(Guid.Parse("11111111111111111111111111111111"),"Section 1"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("22222222222222222222222222222222"),"Section 2"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("33333333333333333333333333333333"),"Section 3"),
                                                                               CreateGroupsHierarchyModel(Guid.Parse("44444444444444444444444444444444"),"Section 4")
                                                                           };
    }
}
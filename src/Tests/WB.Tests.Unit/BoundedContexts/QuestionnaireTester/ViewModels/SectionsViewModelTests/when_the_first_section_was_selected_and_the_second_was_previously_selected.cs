using System;
using System.Collections.Generic;
using System.Linq;

using Cirrious.MvvmCross.Plugins.Messenger;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_the_first_section_was_selected_and_the_second_was_previously_selected : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.GroupsHierarchy == listOfSection);

            questionnaireRepositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireModel);

            sectionsModel = CreateSectionsViewModel(
                messenger: messenger.Object,
                questionnaireRepository: questionnaireRepositoryMock.Object);

            sectionsModel.Init(questionnaireId, navigationState);

            navigationState.NavigateTo(selectedGroupIdentity);
        };

        Because of = () =>
            sectionsModel.Sections[0].NavigateToSectionCommand.Execute(sectionsModel.Sections[0]);

        It should_mark_one_section_as_selected = () =>
            sectionsModel.Sections.Count(x => x.IsSelected).ShouldEqual(1);

        It should_select_section_that_was_navigated_to = () =>
            sectionsModel.Sections.First(x => x.IsSelected).SectionIdentity.ShouldEqual(sectionsModel.Sections[0].SectionIdentity);

        It should_publish_message_that_section_was_changed = () =>
            messenger.Verify(x => x.Publish(Moq.It.IsAny<SectionChangeMessage>()), Times.Once);

        private static SectionsViewModel sectionsModel;
        private const string questionnaireId = "questionnaire Id";
        private static readonly NavigationState navigationState = CreateNavigationState();
        private static readonly Mock<IMvxMessenger> messenger = new Mock<IMvxMessenger>();
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
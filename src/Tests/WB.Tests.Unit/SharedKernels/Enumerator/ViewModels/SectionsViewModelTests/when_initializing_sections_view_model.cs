using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_initializing_sections_view_model : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            //var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.GroupsWithFirstLevelChildrenAsReferences == listOfSection && _.GroupsHierarchy == sectionsHierarchy);
            var questionnaire = Mock.Of<IQuestionnaire>();
            var interview = Mock.Of<IStatefulInterview>(_ => _.IsEnabled(Moq.It.IsAny<Identity>()) == true);

            questionnaireRepositoryMock.SetReturnsDefault(questionnaire);
            interviewRepositoryMock
                .Setup(x => x.Get(interviewId))
                .Returns(interview);
            sectionsModel = CreateSectionsViewModel(
                questionnaireRepository: questionnaireRepositoryMock.Object,
                interviewRepository: interviewRepositoryMock.Object);
        };

        Because of = () => 
            sectionsModel.Init(questionnaireId, interviewId, navigationState);

        It should_initialize_section_list = () =>
            sectionsModel.Sections.ShouldNotBeEmpty();

        It should_create_the_same_amount_of_sections_as_in_questionnaire_model_plus_complete_button = () =>
            sectionsModel.Sections.Count.ShouldEqual(listOfSection.Count + 1);

        static SideBarSectionsViewModel sectionsModel;

        private const string questionnaireId = "questionnaire Id";
        private const string interviewId = "interview Id";
        private static readonly NavigationState navigationState = Mock.Of<NavigationState>();
        private static readonly Mock<IPlainQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();
        private static readonly Mock<IStatefulInterviewRepository> interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();

        private static readonly Dictionary<Guid, GroupModel> listOfSection = new Dictionary<Guid, GroupModel> 
        {
            {Guid.Parse("11111111111111111111111111111111"), Create.GroupModel(Guid.Parse("11111111111111111111111111111111"),"Section 1")  },
            {Guid.Parse("22222222222222222222222222222222"), Create.GroupModel(Guid.Parse("22222222222222222222222222222222"),"Section 2")  },
            {Guid.Parse("33333333333333333333333333333333"), Create.GroupModel(Guid.Parse("33333333333333333333333333333333"),"Section 3")  },
            {Guid.Parse("44444444444444444444444444444444"), Create.GroupModel(Guid.Parse("44444444444444444444444444444444"),"Section 4")  },
        };

        private static readonly List<GroupsHierarchyModel> sectionsHierarchy = new List<GroupsHierarchyModel>
        {
            CreateGroupsHierarchyModel(Guid.Parse("11111111111111111111111111111111"),"Section 1"),
            CreateGroupsHierarchyModel(Guid.Parse("22222222222222222222222222222222"),"Section 2"),
            CreateGroupsHierarchyModel(Guid.Parse("33333333333333333333333333333333"),"Section 3"),
            CreateGroupsHierarchyModel(Guid.Parse("44444444444444444444444444444444"),"Section 4")
        };
    }
}
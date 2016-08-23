using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
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
            questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetAllSections() == listOfSections);
            var interview = Mock.Of<IStatefulInterview>(_ 
                => _.QuestionnaireIdentity == questionnaireIdentity
                && _.IsEnabled(Moq.It.IsAny<Identity>()) == true);

            questionnaireRepositoryMock.SetReturnsDefault(questionnaire);
            interviewRepositoryMock
                .Setup(x => x.Get(interviewId))
                .Returns(interview);
            sectionsModel = CreateSectionsViewModel(
                questionnaireRepository: questionnaireRepositoryMock.Object,
                interviewRepository: interviewRepositoryMock.Object);
        };

        Because of = () => 
            sectionsModel.Init(interviewId, questionnaireIdentity, navigationState);

        It should_initialize_section_list = () =>
            sectionsModel.Sections.ShouldNotBeEmpty();

        It should_create_the_same_amount_of_sections_as_in_questionnaire_model_plus_complete_and_cover_button = () =>
            sectionsModel.Sections.Count.ShouldEqual(listOfSections.Count + 1 + 1);

        static SideBarSectionsViewModel sectionsModel;

        private const string questionnaireId = "questionnaire Id";
        private const string interviewId = "interview Id";
        private static readonly NavigationState navigationState = Mock.Of<NavigationState>();
        private static readonly Mock<IQuestionnaireStorage> questionnaireRepositoryMock = new Mock<IQuestionnaireStorage>();
        private static readonly Mock<IStatefulInterviewRepository> interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
        private static readonly List<Guid> listOfSections = new List<Guid> { Id.g1, Id.g2, Id.g3, Id.g4};
        private static QuestionnaireIdentity questionnaireIdentity;
    }
}
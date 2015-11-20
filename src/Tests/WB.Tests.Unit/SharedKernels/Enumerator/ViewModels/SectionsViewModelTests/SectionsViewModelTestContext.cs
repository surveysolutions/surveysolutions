using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    [Subject(typeof(SideBarSectionsViewModel))]
    internal class SectionsViewModelTestContext
    {
        protected static SideBarSectionsViewModel CreateSectionsViewModel(
            IMvxMessenger messenger = null,
            IStatefulInterviewRepository interviewRepository = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            ISubstitutionService substitutionService = null,
            ISideBarSectionViewModelsFactory sideBarSectionViewModelsFactory = null)
        {

            return new SideBarSectionsViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                Create.LiteEventRegistry(),
                sideBarSectionViewModelsFactory ?? Stub.SideBarSectionViewModelsFactory());
        }


        protected static SideBarSectionsViewModel CreateSectionsViewModel(QuestionnaireModel questionnaire, IStatefulInterview interview)
        {
            var questionnaireRepository = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireRepository.SetReturnsDefault(questionnaire);

            var interviewsRepository = new Mock<IStatefulInterviewRepository>();
            interviewsRepository.SetReturnsDefault(interview);

            Func<SideBarSectionViewModel> sideBarSectionViewModel = () =>
            {
                var barSectionViewModel = new SideBarSectionViewModel(interviewsRepository.Object,
                    questionnaireRepository.Object,
                    Create.SubstitutionService(),
                    Create.LiteEventRegistry(),
                    Stub.SideBarSectionViewModelsFactory(),
                    Mock.Of<IMvxMessenger>());
                barSectionViewModel.NavigationState = Create.NavigationState(); 
                return barSectionViewModel;
            };

            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);

            serviceLocatorMock.Setup(x => x.GetInstance<GroupStateViewModel>())
                .Returns(Mock.Of<GroupStateViewModel>());

            serviceLocatorMock.Setup(x => x.GetInstance<InterviewStateViewModel>())
                .Returns(Mock.Of<InterviewStateViewModel>());

            var sideBarSectionViewModelsFactory =  new SideBarSectionViewModelFactory(serviceLocatorMock.Object);
           
            return CreateSectionsViewModel(questionnaireRepository: questionnaireRepository.Object,
                interviewRepository: interviewsRepository.Object,
                sideBarSectionViewModelsFactory: sideBarSectionViewModelsFactory);
        }

        protected static GroupsHierarchyModel CreateGroupsHierarchyModel(Guid id, string title)
        {
            return new GroupsHierarchyModel
                   {
                       Id = id,
                       Title = title,
                       IsRoster = false
                   };
        }
    }
}
using System;

using Cirrious.MvvmCross.Plugins.Messenger;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    [Subject(typeof(SideBarSectionsViewModel))]
    public class SectionsViewModelTestContext
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
                substitutionService ?? Create.SubstitutionService(),
                Create.LiteEventRegistry(),
                sideBarSectionViewModelsFactory ?? Stub.SideBarSectionViewModelsFactory(),
                Stub.MvxMainThreadDispatcher());
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
                    Stub.MvxMainThreadDispatcher(),
                    Mock.Of<ISideBarSectionViewModelsFactory>(),
                    Mock.Of<IMvxMessenger>());
                barSectionViewModel.NavigationState = Create.NavigationState(); 
                return barSectionViewModel;
            };

            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);

            var sideBarSectionViewModelsFactory =  new SideBarSectionViewModelsFactory(serviceLocatorMock.Object);
           
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
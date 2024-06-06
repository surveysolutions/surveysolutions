using System;
using Main.Core.Documents;
using Moq;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Integration
{
    internal static class SetUp
    {
        public static void MockedServiceLocator()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            IExpressionProcessor roslynExpressionProcessor = new RoslynExpressionProcessor();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(roslynExpressionProcessor);

            var fileSystemIoAccessor = new FileSystemIOAccessor();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IFileSystemAccessor>())
                .Returns(fileSystemIoAccessor);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance(typeof(TInstance)))
                .Returns(instance);
        }

        public static SideBarSectionsViewModel SidebarSectionViewModel(QuestionnaireDocument questionnaireDocument, StatefulInterview interview)
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1);
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(
                x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire
                && x.GetQuestionnaireOrThrow(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            var interviewsRepository = Mock.Of<IStatefulInterviewRepository>(x => 
                x.Get(It.IsAny<string>()) == interview
                && x.GetOrThrow(It.IsAny<string>()) == interview);

            var sideBarSectionViewModelsFactory = new SideBarSectionViewModelFactory(ServiceLocator.Current);

            var navigationState = Mock.Of<NavigationState>(_ => _.InterviewId == interview.Id.FormatGuid());

            var liteEventRegistry = Abc.Create.Service.LiteEventRegistry();

            Func<SideBarSectionViewModel> sideBarSectionViewModel = ()
                => new SideBarSectionViewModel(
                    interviewsRepository,
                    questionnaireRepository,
                    liteEventRegistry,
                    Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository),
                    IntegrationCreate.AnswerNotifier(liteEventRegistry))
                {
                    NavigationState = navigationState,
                };

            var interviewStateViewModel = new InterviewStateViewModel(
                interviewsRepository,
                Mock.Of<IInterviewStateCalculationStrategy>(_ => 
                    _.GetInterviewSimpleStatus(It.IsAny<IStatefulInterview>()) == new InterviewSimpleStatus() ),
                questionnaireRepository);
           
            var coverStateViewModel = new CoverStateViewModel(
                interviewsRepository,
                Mock.Of<IGroupStateCalculationStrategy>(),
                questionnaireRepository);
            
            var groupStateViewModel = new GroupStateViewModel(
                interviewsRepository,
                Mock.Of<IGroupStateCalculationStrategy>(),
                questionnaireRepository);
            
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<CoverStateViewModel>())
                .Returns(() => coverStateViewModel);
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<GroupStateViewModel>())
                .Returns(() => groupStateViewModel);
            
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarVirtualCoverSectionViewModel>())
                .Returns(() => new SideBarVirtualCoverSectionViewModel( Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository), coverStateViewModel));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarCompleteSectionViewModel>())
                .Returns(() => new SideBarCompleteSectionViewModel( Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository), interviewStateViewModel,
                        IntegrationCreate.AnswerNotifier(liteEventRegistry)));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarOverviewViewModel>())
                .Returns(() => new SideBarOverviewViewModel( Create.ViewModel.DynamicTextViewModel(
                    liteEventRegistry,
                    interviewRepository: interviewsRepository), interviewStateViewModel, Mock.Of<AnswerNotifier>()));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);
            SetUp.InstanceToMockedServiceLocator<InterviewStateViewModel>(interviewStateViewModel);

            var sidebarViewModel = new SideBarSectionsViewModel(
                statefulInterviewRepository: interviewsRepository,
                questionnaireRepository: questionnaireRepository,
                modelsFactory: sideBarSectionViewModelsFactory,
                eventRegistry: liteEventRegistry,
                Create.Fake.MvxMainThreadDispatcher());

            sidebarViewModel.Init("", navigationState);

            return sidebarViewModel;
        }
    }
}

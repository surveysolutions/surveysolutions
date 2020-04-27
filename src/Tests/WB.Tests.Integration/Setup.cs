using System;
using Main.Core.Documents;
using Moq;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
                x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            var interviewsRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(It.IsAny<string>()) == interview);

            var sideBarSectionViewModelsFactory = new SideBarSectionViewModelFactory(ServiceLocator.Current);

            var navigationState = Mock.Of<NavigationState>(_ => _.InterviewId == interview.Id.FormatGuid());

            var liteEventRegistry = Abc.Create.Service.LiteEventRegistry();
            var mvxMessenger = Mock.Of<IMvxMessenger>();

            Func<SideBarSectionViewModel> sideBarSectionViewModel = ()
                => new SideBarSectionViewModel(
                    interviewsRepository,
                    questionnaireRepository,
                    mvxMessenger,
                    liteEventRegistry,
                    Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository),
                    IntegrationCreate.AnswerNotifier(liteEventRegistry))
                {
                    NavigationState = navigationState,
                };
           
            SetUp.InstanceToMockedServiceLocator<CoverStateViewModel>(Mock.Of<CoverStateViewModel>());
            SetUp.InstanceToMockedServiceLocator<GroupStateViewModel>(Mock.Of<GroupStateViewModel>());
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarCoverSectionViewModel>())
                .Returns(()=>new SideBarCoverSectionViewModel(mvxMessenger, Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository), Mock.Of<CoverStateViewModel>()));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarCompleteSectionViewModel>())
                .Returns(() => new SideBarCompleteSectionViewModel(mvxMessenger, Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository), Mock.Of<InterviewStateViewModel>(),
                        IntegrationCreate.AnswerNotifier(liteEventRegistry)));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarOverviewViewModel>())
                .Returns(() => new SideBarOverviewViewModel(mvxMessenger, Create.ViewModel.DynamicTextViewModel(
                    liteEventRegistry,
                    interviewRepository: interviewsRepository), Mock.Of<InterviewStateViewModel>(), Mock.Of<AnswerNotifier>()));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);
            SetUp.InstanceToMockedServiceLocator<InterviewStateViewModel>(Mock.Of<InterviewStateViewModel>());

            var sidebarViewModel = new SideBarSectionsViewModel(
                statefulInterviewRepository: interviewsRepository,
                questionnaireRepository: questionnaireRepository,
                modelsFactory: sideBarSectionViewModelsFactory,
                eventRegistry: liteEventRegistry,
                mainThreadDispatcher: Stub.MvxMainThreadAsyncDispatcher());

            sidebarViewModel.Init("", navigationState);

            return sidebarViewModel;
        }
    }
}

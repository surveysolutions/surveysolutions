using System;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using MvvmCross.Plugins.Messenger;
using Ncqrs;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Integration
{
    internal static class Setup
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
        }

        public static SideBarSectionsViewModel SidebarSectionViewModel(QuestionnaireDocument questionnaireDocument, StatefulInterview interview)
        {
            var questionnaire = new PlainQuestionnaire(questionnaireDocument, 1);
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(
                x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            var interviewsRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(It.IsAny<string>()) == interview);

            var sideBarSectionViewModelsFactory = new SideBarSectionViewModelFactory(ServiceLocator.Current);

            var navigationState = Mock.Of<NavigationState>(_ => _.InterviewId == interview.Id.FormatGuid());

            Func<SideBarSectionViewModel> sideBarSectionViewModel = ()
                => new SideBarSectionViewModel(
                    interviewsRepository,
                    questionnaireRepository,
                    new SubstitutionService(),
                    new LiteEventRegistry(),
                    sideBarSectionViewModelsFactory,
                    Mock.Of<IMvxMessenger>(),
                    Create.DynamicTextViewModel(
                        questionnaireRepository: questionnaireRepository,
                        interviewRepository: interviewsRepository))
                {
                    NavigationState = navigationState,
                };

            Setup.InstanceToMockedServiceLocator<GroupStateViewModel>(Mock.Of<GroupStateViewModel>());
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns(sideBarSectionViewModel);
            Setup.InstanceToMockedServiceLocator<InterviewStateViewModel>(Mock.Of<InterviewStateViewModel>());

            var sidebarViewModel = new SideBarSectionsViewModel(
                statefulInterviewRepository: interviewsRepository,
                questionnaireRepository: questionnaireRepository,
                eventRegistry: new LiteEventRegistry(),
                modelsFactory: sideBarSectionViewModelsFactory);

            sidebarViewModel.Init("", new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version), navigationState);

            return sidebarViewModel;
        }
    }
}
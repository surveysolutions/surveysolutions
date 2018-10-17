using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Tests.Abc.TestFactories
{
    internal class ViewModelFactory
    {
        public AttachmentViewModel AttachmentViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAttachmentContentStorage attachmentContentStorage)
            => new AttachmentViewModel(questionnaireRepository, interviewRepository, attachmentContentStorage);

        public DynamicTextViewModel DynamicTextViewModel(
            ILiteEventRegistry eventRegistry = null, 
            IStatefulInterviewRepository interviewRepository = null)
            => new DynamicTextViewModel(
                eventRegistry ?? Create.Service.LiteEventRegistry(),
                interviewRepository: interviewRepository,
                substitutionService: Create.Service.SubstitutionService());

        public ErrorMessageViewModel ErrorMessageViewModel(
            ILiteEventRegistry eventRegistry = null,
            IStatefulInterviewRepository interviewRepository = null)
            => new ErrorMessageViewModel(
                eventRegistry ?? Create.Service.LiteEventRegistry(),
                interviewRepository: interviewRepository,
                substitutionService: Create.Service.SubstitutionService());

        public EnumerationStageViewModel EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ISubstitutionService substitutionService = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMessenger messenger = null,
            IUserInterfaceStateService userInterfaceStateService = null,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher = null,
            ICompositeCollectionInflationService compositeCollectionInflationService = null,
            IVirbationService virbationService = null)
            => new EnumerationStageViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>(),
                mvxMainThreadDispatcher ?? Stub.MvxMainThreadAsyncDispatcher(),
                Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: eventRegistry,
                    interviewRepository: interviewRepository),
                compositeCollectionInflationService ?? Mock.Of<ICompositeCollectionInflationService>(),
                Mock.Of<ILiteEventRegistry>(),
                Mock.Of<ICommandService>());

        public ErrorMessagesViewModel ErrorMessagesViewModel(
            IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            var dynamicTextViewModelFactory = Mock.Of<IDynamicTextViewModelFactory>();

            Mock.Get(dynamicTextViewModelFactory)
                .Setup(factory => factory.CreateDynamicTextViewModel())
                .Returns(() => Create.ViewModel.DynamicTextViewModel(
                    interviewRepository: interviewRepository));
            Mock.Get(dynamicTextViewModelFactory)
                .Setup(factory => factory.CreateErrorMessage())
                .Returns(() => Create.ViewModel.ErrorMessageViewModel(
                    interviewRepository: interviewRepository));

            return new ErrorMessagesViewModel(dynamicTextViewModelFactory);
        }
        
        public SingleOptionLinkedToListQuestionViewModel SingleOptionLinkedToListQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
            => new SingleOptionLinkedToListQuestionViewModel(
                Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == (questionnaire ?? Mock.Of<IQuestionnaire>())),
                Mock.Of<IStatefulInterviewRepository>(_ => _.Get(It.IsAny<string>()) == (interview ?? Mock.Of<IStatefulInterview>())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadAsyncDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionQuestionAnswered>>.WithNotEmptyValues,
                Mock.Of<QuestionInstructionViewModel>(),
                answering ?? Mock.Of<AnsweringViewModel>(),
                Create.ViewModel.ThrottlingViewModel());

        public MultiOptionLinkedToListQuestionQuestionViewModel MultiOptionLinkedToListQuestionQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
            => new MultiOptionLinkedToListQuestionQuestionViewModel(
                questionState ?? Stub<QuestionStateViewModel<MultipleOptionsQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<QuestionInstructionViewModel>(),
                Mock.Of<IStatefulInterviewRepository>(_ => _.Get(It.IsAny<string>()) == (interview ?? Mock.Of<IStatefulInterview>())),
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == (questionnaire ?? Mock.Of<IQuestionnaire>())),
                Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadAsyncDispatcher(),
                Create.ViewModel.ThrottlingViewModel());

        public SingleOptionLinkedQuestionViewModel SingleOptionLinkedQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null) => new SingleOptionLinkedQuestionViewModel(
            Mock.Of<IPrincipal>(_ =>
                _.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
            Mock.Of<IQuestionnaireStorage>(_ =>
                _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) ==
                (questionnaire ?? Mock.Of<IQuestionnaire>())),
            Mock.Of<IStatefulInterviewRepository>(_ =>
                _.Get(It.IsAny<string>()) == (interview ?? Mock.Of<IStatefulInterview>())),
            eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
            Stub.MvxMainThreadAsyncDispatcher(),
            questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
            Mock.Of<QuestionInstructionViewModel>(),
            answering ?? Mock.Of<AnsweringViewModel>(),
            Create.ViewModel.ThrottlingViewModel());

        public TextQuestionViewModel TextQuestionViewModel(
            ILiteEventRegistry eventRegistry = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IStatefulInterviewRepository interviewRepository = null,
            QuestionStateViewModel<TextQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
        {
            var statefulInterviewRepository = interviewRepository ?? Stub<IStatefulInterviewRepository>.WithNotEmptyValues;
            var questionnaireRepository = questionnaireStorage ?? Stub<IQuestionnaireStorage>.WithNotEmptyValues;
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid()));
            var liteEventRegistry = eventRegistry ?? Create.Service.LiteEventRegistry();

            var questionStateViewModel = questionState ??
                                         this.QuestionState<TextQuestionAnswered>(liteEventRegistry: eventRegistry,
                                             interviewRepository: statefulInterviewRepository, questionnaireStorage: questionnaireRepository);

            return new TextQuestionViewModel(
                liteEventRegistry,
                principal,
                questionnaireRepository,
                statefulInterviewRepository,
                questionStateViewModel,
                new QuestionInstructionViewModel(questionnaireRepository, statefulInterviewRepository, 
                    new DynamicTextViewModel(eventRegistry ?? Stub<ILiteEventRegistry>.WithNotEmptyValues, statefulInterviewRepository, new SubstitutionService())),
                answering ?? this.AnsweringViewModel());
        }

        public AnsweringViewModel AnsweringViewModel(ICommandService commandService = null,
            IUserInterfaceStateService userInterfaceStateService = null)
            => new AnsweringViewModel(
                commandService ?? Stub<ICommandService>.WithNotEmptyValues,
                userInterfaceStateService ?? Stub<IUserInterfaceStateService>.WithNotEmptyValues,
                Mock.Of<IMvxMessenger>());

        public ValidityViewModel ValidityViewModel(
            ILiteEventRegistry eventRegistry = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaire questionnaire = null,
            Identity entityIdentity = null)
        {
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(
                x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            return new ValidityViewModel(
                eventRegistry ?? Create.Service.LiteEventRegistry(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Stub.MvxMainThreadAsyncDispatcher(),
                Create.ViewModel.ErrorMessagesViewModel(
                    questionnaireRepository: questionnaireRepository,
                    interviewRepository: interviewRepository));
        }

        public VariableViewModel VariableViewModel(
            ILiteEventRegistry eventRegistry = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaire questionnaire = null,
            Identity entityIdentity = null)
        {
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(
                x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            return new VariableViewModel(
                questionnaireRepository,
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                eventRegistry ?? Create.Service.LiteEventRegistry());
        }

        public QuestionInstructionViewModel QuestionInstructionViewModel()
        {
            return Mock.Of<QuestionInstructionViewModel>();
        }

        public QuestionStateViewModel<T> QuestionState<T>(
            ILiteEventRegistry liteEventRegistry = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null) where T : QuestionAnswered
        {
            var questionnaireRepository = questionnaireStorage ?? Stub<IQuestionnaireStorage>.WithNotEmptyValues;
            liteEventRegistry = liteEventRegistry ?? Stub<ILiteEventRegistry>.WithNotEmptyValues;
            interviewRepository = interviewRepository ?? Stub<IStatefulInterviewRepository>.WithNotEmptyValues;

            var headerViewModel = new QuestionHeaderViewModel(
                enablementViewModel: Create.ViewModel.EnablementViewModel(
                    interviewRepository: interviewRepository,
                    eventRegistry: liteEventRegistry,
                    questionnaireRepository: questionnaireRepository),
                dynamicTextViewModel: Create.ViewModel.DynamicTextViewModel(eventRegistry: liteEventRegistry,
                    interviewRepository: interviewRepository));

            var validityViewModel = new ValidityViewModel(
                liteEventRegistry: liteEventRegistry,
                interviewRepository: interviewRepository,
                mainThreadDispatcher: Create.Fake.MvxMainThreadDispatcher(),
                errorMessagesViewModel: new ErrorMessagesViewModel(Stub<IDynamicTextViewModelFactory>.WithNotEmptyValues));

            var warningsViewModel = new WarningsViewModel(
                liteEventRegistry: liteEventRegistry,
                interviewRepository: interviewRepository,
                mainThreadDispatcher: Create.Fake.MvxMainThreadDispatcher(),
                errorMessagesViewModel: new ErrorMessagesViewModel(Stub<IDynamicTextViewModelFactory>.WithNotEmptyValues));

            var commentsViewModel = new CommentsViewModel(interviewRepository: interviewRepository,
                                    commandService: Stub<ICommandService>.WithNotEmptyValues,
                                    principal: Stub<IPrincipal>.WithNotEmptyValues,
                                    mainThreadDispatcher: Stub.MvxMainThreadAsyncDispatcher());

            var answersRemovedNotifier = new AnswersRemovedNotifier(liteEventRegistry);

            return new QuestionStateViewModel<T>(
                liteEventRegistry: liteEventRegistry,
                interviewRepository: interviewRepository,
                validityViewModel: validityViewModel,
                questionHeaderViewModel: headerViewModel,
                enablementViewModel: Create.ViewModel.EnablementViewModel(
                    interviewRepository: interviewRepository,
                    eventRegistry: liteEventRegistry,
                    questionnaireRepository: questionnaireRepository),
                commentsViewModel: commentsViewModel,
                answersRemovedNotifier: answersRemovedNotifier, 
                warningsViewModel: warningsViewModel);
        }

        private EnablementViewModel EnablementViewModel(IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry, IQuestionnaireStorage questionnaireRepository)
            => new EnablementViewModel(interviewRepository, eventRegistry, questionnaireRepository);

        public FilteredOptionsViewModel FilteredOptionsViewModel(
            Identity questionId,
            QuestionnaireDocument questionnaire, 
            IStatefulInterview statefulInterview)
        {
            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);

            var result = new FilteredOptionsViewModel(questionnaireRepository, interviewRepository, new AnswerNotifier(Create.Service.LiteEventRegistry()));
            result.Init(statefulInterview.Id.FormatGuid(), questionId, 30);
            return result;
        }

        public SideBarSectionsViewModel SidebarSectionsViewModel(
            QuestionnaireDocument questionnaireDocument, 
            IStatefulInterview interview, 
            LiteEventRegistry liteEventRegistry,
            NavigationState navigationState = null)
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1);
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(
                x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            var interviewsRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(It.IsAny<string>()) == interview);

            var sideBarSectionViewModelsFactory = new SideBarSectionViewModelFactory(ServiceLocator.Current);
            
            var mvxMessenger = Mock.Of<IMvxMessenger>();
            navigationState = navigationState ?? Create.Other.NavigationState();

            SideBarSectionViewModel SideBarSectionViewModel()
            {
                return new SideBarSectionViewModel(interviewsRepository, questionnaireRepository, mvxMessenger, liteEventRegistry,
                    Create.ViewModel.DynamicTextViewModel(liteEventRegistry, interviewsRepository),
                    Create.Entity.AnswerNotifier(liteEventRegistry))
                {
                    NavigationState = navigationState,
                };
            }

            Setup.InstanceToMockedServiceLocator(Mock.Of<CoverStateViewModel>());
            Setup.InstanceToMockedServiceLocator(Mock.Of<GroupStateViewModel>());
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns((Func<SideBarSectionViewModel>) SideBarSectionViewModel);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarCoverSectionViewModel>())
                .Returns(() => new SideBarCoverSectionViewModel(mvxMessenger, Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository), Mock.Of<CoverStateViewModel>()));

            
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarOverviewViewModel>())
                .Returns(() => new SideBarOverviewViewModel(mvxMessenger, Create.ViewModel.DynamicTextViewModel(
                    liteEventRegistry,
                    interviewRepository: interviewsRepository), Mock.Of<InterviewStateViewModel>()));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarCompleteSectionViewModel>())
                .Returns(() => new SideBarCompleteSectionViewModel(mvxMessenger, Create.ViewModel.DynamicTextViewModel(
                        liteEventRegistry,
                        interviewRepository: interviewsRepository), Mock.Of<InterviewStateViewModel>(),
                        Create.Entity.AnswerNotifier(liteEventRegistry)));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<SideBarSectionViewModel>())
                .Returns((Func<SideBarSectionViewModel>) SideBarSectionViewModel);
            Setup.InstanceToMockedServiceLocator(Mock.Of<InterviewStateViewModel>());


            var sidebarViewModel = new SideBarSectionsViewModel(
                statefulInterviewRepository: interviewsRepository,
                questionnaireRepository: questionnaireRepository,
                modelsFactory: sideBarSectionViewModelsFactory,
                eventRegistry: liteEventRegistry,
                mainThreadDispatcher: Stub.MvxMainThreadAsyncDispatcher());

            sidebarViewModel.Init("", navigationState);

            return sidebarViewModel;
        }

        public MultiOptionLinkedToRosterQuestionViewModel MultiOptionLinkedToRosterQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
            => new MultiOptionLinkedToRosterQuestionViewModel(
                questionState ?? Stub<QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<QuestionInstructionViewModel>(),
                Mock.Of<IStatefulInterviewRepository>(_ => _.Get(It.IsAny<string>()) == (interview ?? Mock.Of<IStatefulInterview>())),
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == (questionnaire ?? Mock.Of<IQuestionnaire>())),
                Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadAsyncDispatcher(),
                Create.ViewModel.ThrottlingViewModel());

        public VibrationViewModel VibrationViewModel(ILiteEventRegistry eventRegistry = null,
            IEnumeratorSettings enumeratorSettings = null, IVirbationService vibrationService = null)
            => new VibrationViewModel(
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                enumeratorSettings ?? Mock.Of<IEnumeratorSettings>(), 
                vibrationService ?? Mock.Of<IVirbationService>());

        public SingleOptionQuestionOptionViewModel SingleOptionQuestionOptionViewModel(int? value = null)
        {
            return new SingleOptionQuestionOptionViewModel()
            {
                Value = value ?? 0
            };
        }

        public SpecialValuesViewModel SpecialValues(
            FilteredOptionsViewModel optionsViewModel = null,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            return new SpecialValuesViewModel(
                optionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(), 
                mvxMainThreadDispatcher ?? Create.Fake.MvxMainThreadDispatcher(), 
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>());
        }

        public SideBarCompleteSectionViewModel SideBarCompleteSectionViewModel()
        {
            return new SideBarCompleteSectionViewModel(Mock.Of<IMvxMessenger>(),
                Create.ViewModel.DynamicTextViewModel(),
                Mock.Of<InterviewStateViewModel>(),
                Create.Entity.AnswerNotifier(Create.Service.LiteEventRegistry()));
        }

        public WaitingForSupervisorActionViewModel WaitingForSupervisorActionViewModel(
            IDashboardItemsAccessor dashboardItemsAccessor = null,
            IInterviewViewModelFactory viewModelFactory = null)
            => new WaitingForSupervisorActionViewModel(dashboardItemsAccessor ?? Mock.Of<IDashboardItemsAccessor>(),
                viewModelFactory ?? Create.Service.SupervisorInterviewViewModelFactory());

        public SupervisorDashboardInterviewViewModel SupervisorDashboardInterviewViewModel(Guid? interviewId = null,
            IPrincipal principal = null,
            IPlainStorage<InterviewerDocument> interviewers = null)
        {
            var viewModel = new SupervisorDashboardInterviewViewModel(
                Mock.Of<IServiceLocator>(),
                Mock.Of<IAuditLogService>(),
                Mock.Of<IViewModelNavigationService>(),
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Create.Other.SupervisorIdentity(null, null, null)),
                interviewers ?? new InMemoryPlainStorage<InterviewerDocument>());

            if (interviewId.HasValue)
            {
                viewModel.Init(Create.Entity.InterviewView(interviewId: interviewId,
                    questionnaireId: Create.Entity.QuestionnaireIdentity().ToString()), new List<PrefilledQuestion>());
            }

            return viewModel;
        }

        public FinishInstallationViewModel FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IPrincipal principal = null,
            IPasswordHasher passwordHasher = null,
            IPlainStorage<SupervisorIdentity> interviewersPlainStorage = null,
            IDeviceSettings deviceSettings = null,
            ISupervisorSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IQRBarcodeScanService qrBarcodeScanService = null,
            ISerializer serializer = null,
            IUserInteractionService userInteractionService = null)
            => new FinishInstallationViewModel(viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                principal ?? Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Create.Other.SupervisorIdentity(null, null, null)),
                passwordHasher?? Mock.Of<IPasswordHasher>(),
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<SupervisorIdentity>>(),
                deviceSettings ?? Mock.Of <IDeviceSettings>(),
                synchronizationService ?? Mock.Of<ISupervisorSynchronizationService>(),
                logger ?? Mock.Of<ILogger>(),
                qrBarcodeScanService ?? Mock.Of<IQRBarcodeScanService>(),
                serializer?? Mock.Of <ISerializer>(),
                userInteractionService?? Mock.Of<IUserInteractionService>());

        public ConnectedDeviceSynchronizationViewModel ConnectedDeviceSynchronizationViewModel()
            => new ConnectedDeviceSynchronizationViewModel();

        public ThrottlingViewModel ThrottlingViewModel(IUserInterfaceStateService userInterfaceStateService = null)
        {
            return new ThrottlingViewModel(userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>())
            {
                ThrottlePeriod = 0
            };
        }
        
        public SearchViewModel SearchViewModel(
            IPrincipal principal = null,
            IViewModelNavigationService viewModelNavigationService = null,
            IInterviewViewModelFactory viewModelFactory = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo = null,
            IAssignmentDocumentsStorage assignmentsRepository = null,
            IMvxMessenger messenger = null)
            => new SearchViewModel(
                principal ?? Mock.Of<IPrincipal>(),
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                viewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(m => m.LoadAll() == Enumerable.Empty<InterviewView>().ToReadOnlyCollection()),
                identifyingQuestionsRepo ?? Mock.Of<IPlainStorage<PrefilledQuestionView>>(m => m.LoadAll() == Enumerable.Empty<PrefilledQuestionView>().ToReadOnlyCollection()),
                assignmentsRepository ?? Mock.Of<IAssignmentDocumentsStorage>(),
                messenger ?? Mock.Of<IMvxMessenger>());
    }
}

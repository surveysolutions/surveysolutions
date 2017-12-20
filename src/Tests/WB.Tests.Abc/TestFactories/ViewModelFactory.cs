﻿using System;
using Main.Core.Documents;
using Moq;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
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
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

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
            IMvxMainThreadDispatcher mvxMainThreadDispatcher = null,
            ICompositeCollectionInflationService compositeCollectionInflationService = null,
            IVirbationService virbationService = null)
            => new EnumerationStageViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>(),
                mvxMainThreadDispatcher ?? Stub.MvxMainThreadDispatcher(),
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
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionQuestionAnswered>>.WithNotEmptyValues,
                Mock.Of<QuestionInstructionViewModel>(),
                answering ?? Mock.Of<AnsweringViewModel>());

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
                Stub.MvxMainThreadDispatcher());

        public SingleOptionLinkedQuestionViewModel SingleOptionLinkedQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
            => new SingleOptionLinkedQuestionViewModel(
                Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid())),
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == (questionnaire ?? Mock.Of<IQuestionnaire>())),
                Mock.Of<IStatefulInterviewRepository>(_ => _.Get(It.IsAny<string>()) == (interview ?? Mock.Of<IStatefulInterview>())),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                Mock.Of<QuestionInstructionViewModel>(),
                answering ?? Mock.Of<AnsweringViewModel>());
        
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
                new QuestionInstructionViewModel(questionnaireRepository, statefulInterviewRepository),
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
                Stub.MvxMainThreadDispatcher(),
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

            var commentsViewModel = new CommentsViewModel(interviewRepository: interviewRepository,
                                    commandService: Stub<ICommandService>.WithNotEmptyValues,
                                    principal: Stub<IPrincipal>.WithNotEmptyValues,
                                    mainThreadDispatcher: Stub.MvxMainThreadDispatcher());

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
                answersRemovedNotifier: answersRemovedNotifier);
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
                eventRegistry: liteEventRegistry);

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
                Stub.MvxMainThreadDispatcher());

        public VibrationViewModel VibrationViewModel(ILiteEventRegistry eventRegistry = null,
            IEnumeratorSettings enumeratorSettings = null, IVirbationService virbationService = null)
            => new VibrationViewModel(
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                enumeratorSettings ?? Mock.Of<IEnumeratorSettings>(), 
                virbationService ?? Mock.Of<IVirbationService>());

        public SingleOptionQuestionOptionViewModel SingleOptionQuestionOptionViewModel(int? value = null)
        {
            return new SingleOptionQuestionOptionViewModel()
            {
                Value = value ?? 0
            };
        }
    }
}
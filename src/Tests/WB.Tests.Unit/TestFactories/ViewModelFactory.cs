using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using System;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.TestFactories
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

        public EnumerationStageViewModel EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ISubstitutionService substitutionService = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMessenger messenger = null,
            IUserInterfaceStateService userInterfaceStateService = null,
            IMvxMainThreadDispatcher mvxMainThreadDispatcher = null,
            ICompositeCollectionInflationService compositeCollectionInflationService = null)
            => new EnumerationStageViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                questionnaireRepository ?? Stub<IQuestionnaireStorage>.WithNotEmptyValues,
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>(),
                mvxMainThreadDispatcher ?? Stub.MvxMainThreadDispatcher(),
                Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: eventRegistry,
                    interviewRepository: interviewRepository),
                Mock.Of<IMvxMessenger>(),
                Mock.Of<IEnumeratorSettings>(),
                compositeCollectionInflationService ?? Mock.Of<ICompositeCollectionInflationService>());

        public ErrorMessagesViewModel ErrorMessagesViewModel(
            IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            var dynamicTextViewModelFactory = Mock.Of<IDynamicTextViewModelFactory>();

            Mock.Get(dynamicTextViewModelFactory)
                .Setup(factory => factory.CreateDynamicTextViewModel())
                .Returns(() => Create.ViewModel.DynamicTextViewModel(
                    interviewRepository: interviewRepository));

            return new ErrorMessagesViewModel(dynamicTextViewModelFactory);
        }

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
                answering ?? AnsweringViewModel());
        }

        public AnsweringViewModel AnsweringViewModel(ICommandService commandService = null,
            IUserInterfaceStateService userInterfaceStateService = null)
            => new AnsweringViewModel(
                commandService ?? Stub<ICommandService>.WithNotEmptyValues,
                userInterfaceStateService ?? Stub<IUserInterfaceStateService>.WithNotEmptyValues);

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
                questionnaireRepository,
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
                questionnaireRepository: questionnaireRepository,
                mainThreadDispatcher: Create.Fake.MvxMainThreadDispatcher(),
                errorMessagesViewModel: new ErrorMessagesViewModel(Stub<IDynamicTextViewModelFactory>.WithNotEmptyValues));

            var commentsViewModel = new CommentsViewModel(interviewRepository: interviewRepository,
                                    commandService: Stub<ICommandService>.WithNotEmptyValues,
                                    principal: Stub<IPrincipal>.WithNotEmptyValues);

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
    }
}
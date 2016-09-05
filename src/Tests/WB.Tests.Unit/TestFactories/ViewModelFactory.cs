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
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
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
            SubstitutionViewModel substitutionViewModel = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService = null)
            => new DynamicTextViewModel(
                eventRegistry ?? Create.Service.LiteEventRegistry(),
                substitutionViewModel ?? Create.ViewModel.SubstitutionViewModel(
                    interviewRepository: interviewRepository,
                    questionnaireRepository: questionnaireRepository,
                    rosterTitleSubstitutionService: rosterTitleSubstitutionService));

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
                substitutionService ?? Mock.Of<ISubstitutionService>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>(),
                mvxMainThreadDispatcher ?? Stub.MvxMainThreadDispatcher(),
                Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: eventRegistry,
                    questionnaireRepository: questionnaireRepository,
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
                    questionnaireRepository: questionnaireRepository,
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
                Create.Service.AnswerToStringService(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>());

        public SubstitutionViewModel SubstitutionViewModel(
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService = null)
            => new SubstitutionViewModel(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionnaireRepository ?? Stub<IQuestionnaireStorage>.WithNotEmptyValues,
                Create.Service.SubstitutionService(),
                Create.Service.AnswerToStringService(),
                Create.Service.VariableToUIStringService(),
                rosterTitleSubstitutionService ?? Create.Fake.RosterTitleSubstitutionService());

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

        public QuestionInstructionViewModel QuestionInstructionViewModel()
        {
            return Mock.Of<QuestionInstructionViewModel>();
        }
    }
}
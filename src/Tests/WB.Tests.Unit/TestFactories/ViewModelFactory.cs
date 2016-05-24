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
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.TestFactories
{
    internal class ViewModelFactory
    {
        public ValidityViewModel ValidityViewModel(ILiteEventRegistry eventRegistry = null,
            IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaire questionnaire = null,
            Identity entityIdentity = null)
        {
            var result = new ValidityViewModel(eventRegistry ?? Create.Service.LiteEventRegistry(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IPlainQuestionnaireRepository>(
                    x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                Stub.MvxMainThreadDispatcher());

            return result;
        }

        public StaticTextStateViewModel StaticTextStateViewModel(
            IStatefulInterviewRepository interviewRepository = null,
            ILiteEventRegistry eventRegistry = null)
        {
            return new StaticTextStateViewModel(
                Create.ViewModel.EnablementViewModel(interviewRepository, eventRegistry),
                Create.ViewModel.ValidityViewModel(interviewRepository: interviewRepository));
        }

        public EnablementViewModel EnablementViewModel(IStatefulInterviewRepository interviewRepository = null,
            ILiteEventRegistry eventRegistry = null)
        {
            return new EnablementViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                eventRegistry ?? Create.Service.LiteEventRegistry());
        }

        public EnumerationStageViewModel EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ISubstitutionService substitutionService = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMessenger messenger = null,
            IUserInterfaceStateService userInterfaceStateService = null,
            IMvxMainThreadDispatcher mvxMainThreadDispatcher = null)
            => new EnumerationStageViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                questionnaireRepository ?? Stub<IPlainQuestionnaireRepository>.WithNotEmptyValues,
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                substitutionService ?? Mock.Of<ISubstitutionService>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>(),
                mvxMainThreadDispatcher ?? Stub.MvxMainThreadDispatcher());

        public SingleOptionLinkedQuestionViewModel SingleOptionLinkedQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid());
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>();
            interview = interview ?? Mock.Of<IStatefulInterview>();

            return new SingleOptionLinkedQuestionViewModel(
                Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity),
                Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                Mock.Of<IStatefulInterviewRepository>(_ => _.Get(It.IsAny<string>()) == interview),
                Create.Service.AnswerToStringService(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>());
        }

        public AttachmentViewModel AttachmentViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAttachmentContentStorage attachmentContentStorage)
        {
            return new AttachmentViewModel(
                questionnaireRepository: questionnaireRepository,
                interviewRepository: interviewRepository,
                attachmentContentStorage: attachmentContentStorage);
        }
    }
}
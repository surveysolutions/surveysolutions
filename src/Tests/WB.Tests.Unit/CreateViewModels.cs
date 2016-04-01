using System;
using Moq;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        internal static class ViewModels
        {
            public static ValidityViewModel ValidityViewModel(ILiteEventRegistry eventRegistry = null,
                                                            IStatefulInterviewRepository interviewRepository = null,
                                                            IQuestionnaire questionnaire = null,
                                                            Identity entityIdentity = null)
            {
                var result = new ValidityViewModel(eventRegistry ?? Create.LiteEventRegistry(),
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                    Stub.MvxMainThreadDispatcher());
                result.Init("interviewid", entityIdentity, Create.NavigationState(interviewRepository));

                Mvx.RegisterSingleton(Stub.MvxMainThreadDispatcher());

                return result;
            }
        }

        public static SynchronizationViewModel SynchronizationViewModel(IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IAsyncPlainStorage<InterviewFileView> interviewFileViewStorage = null,
            ISynchronizationService synchronizationService = null,
            ILogger logger = null,
            IUserInteractionService userInteractionService = null,
            IPasswordHasher passwordHasher = null,
            IPrincipal principal = null,
            IMvxMessenger messenger = null,
            IInterviewerQuestionnaireAccessor questionnaireFactory = null,
            IInterviewerInterviewAccessor interviewFactory = null,
            IAttachmentContentStorage attachmentContentStorage = null)
        {
            return new SynchronizationViewModel(
                interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                interviewersPlainStorage ?? Mock.Of<IAsyncPlainStorage<InterviewerIdentity>>(),
                interviewMultimediaViewStorage ?? Mock.Of<IAsyncPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewStorage ?? Mock.Of<IAsyncPlainStorage<InterviewFileView>>(),
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                logger ?? Mock.Of<ILogger>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                principal ?? Mock.Of<IPrincipal>(),
                messenger ?? Mock.Of<IMvxMessenger>(),
                questionnaireFactory ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>());
        }
    }
}
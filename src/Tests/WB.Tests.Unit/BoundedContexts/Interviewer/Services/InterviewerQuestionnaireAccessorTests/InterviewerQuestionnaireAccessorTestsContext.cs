﻿using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class InterviewerQuestionnaireAccessorTestsContext
    {
        public static InterviewerQuestionnaireAccessor CreateInterviewerQuestionnaireAccessor(
            ISynchronizationSerializer synchronizationSerializer = null,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor = null,
            IInterviewerInterviewAccessor interviewFactory = null)
        {
            return new InterviewerQuestionnaireAccessor(
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<ISynchronizationSerializer>(),
                questionnaireViewRepository: questionnaireViewRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireView>>(),
                plainQuestionnaireRepository: plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                interviewViewRepository: interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                questionnaireAssemblyFileAccessor: questionnaireAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                interviewFactory: interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>());
        }
    }
}

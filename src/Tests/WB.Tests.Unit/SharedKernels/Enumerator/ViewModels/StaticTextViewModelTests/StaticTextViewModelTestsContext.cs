﻿using Machine.Specifications;
using Moq;
using MvvmCross.Platform.Core;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [Subject(typeof(StaticTextViewModel))]
    internal class StaticTextViewModelTestsContext
    {
        public static StaticTextViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            ILiteEventRegistry registry = null,
            AttachmentViewModel attachmentViewModel = null,
            StaticTextStateViewModel questionState = null)
        {
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var plainQuestionnaireRepository = questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>();

            var liteEventRegistry = registry ?? Create.Service.LiteEventRegistry();

            return new StaticTextViewModel(
                questionState: questionState ??
                               new StaticTextStateViewModel(
                                   new EnablementViewModel(statefulInterviewRepository, liteEventRegistry, plainQuestionnaireRepository),
                                   new ValidityViewModel(liteEventRegistry, statefulInterviewRepository, Create.Fake.MvxMainThreadDispatcher(), Create.ViewModel.ErrorMessagesViewModel())),
                attachmentViewModel: attachmentViewModel ?? new AttachmentViewModel(plainQuestionnaireRepository, statefulInterviewRepository, Mock.Of<IAttachmentContentStorage>()),
                dynamicTextViewModel: Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: liteEventRegistry,
                    interviewRepository: statefulInterviewRepository));
        }
    }
}
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [NUnit.Framework.TestOf(typeof(StaticTextViewModel))]
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
                                   new ValidityViewModel(liteEventRegistry, statefulInterviewRepository, Create.Fake.MvxMainThreadDispatcher(), Create.ViewModel.ErrorMessagesViewModel()),
                                   new WarningsViewModel(liteEventRegistry, statefulInterviewRepository, Create.Fake.MvxMainThreadDispatcher(), Create.ViewModel.ErrorMessagesViewModel())),
                attachmentViewModel: attachmentViewModel ?? new AttachmentViewModel(plainQuestionnaireRepository, statefulInterviewRepository, Mock.Of<IAttachmentContentStorage>(), Mock.Of<IEnumeratorSettings>(), Mock.Of<IExternalAppLauncher>()),
                dynamicTextViewModel: Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: liteEventRegistry,
                    interviewRepository: statefulInterviewRepository));
        }
    }
}

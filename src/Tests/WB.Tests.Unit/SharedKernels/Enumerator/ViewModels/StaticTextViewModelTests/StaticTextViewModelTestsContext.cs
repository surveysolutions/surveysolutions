using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [NUnit.Framework.TestOf(typeof(StaticTextViewModel))]
    internal class StaticTextViewModelTestsContext: MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            base.Setup();
            
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
    
        public static StaticTextViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            IViewModelEventRegistry registry = null,
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
                                   new ValidityViewModel(liteEventRegistry, statefulInterviewRepository, Create.ViewModel.ErrorMessagesViewModel()),
                                   new WarningsViewModel(liteEventRegistry, statefulInterviewRepository, Create.ViewModel.ErrorMessagesViewModel())),
                attachmentViewModel: attachmentViewModel ??
                            Create.ViewModel.AttachmentViewModel(plainQuestionnaireRepository, statefulInterviewRepository),
                dynamicTextViewModel: Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: liteEventRegistry,
                    interviewRepository: statefulInterviewRepository,
                    questionnaireStorage: plainQuestionnaireRepository));
        }
    }
}

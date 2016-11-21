using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnablementViewModelTests
{
    [Subject(typeof(EnablementViewModel))]
    internal class EnablementViewModelTestsContext
    {
        public static EnablementViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ILiteEventRegistry registry = null)
        {
            return new EnablementViewModel(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(), 
                registry ?? Create.Service.LiteEventRegistry(), 
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>());
        }
    }
}
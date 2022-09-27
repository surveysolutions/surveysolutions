using Moq;
using MvvmCross.Tests;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnablementViewModelTests
{
    [NUnit.Framework.TestOf(typeof(EnablementViewModel))]
    internal class EnablementViewModelTestsContext : MvxIoCSupportingTest
    {
        public static EnablementViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IViewModelEventRegistry registry = null)
        {
            return new EnablementViewModel(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(), 
                registry ?? Create.Service.LiteEventRegistry(), 
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>());
        }
    }
}

using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionLinkedToListQuestionViewModelTests
{
    [TestOf(typeof(SingleOptionLinkedToListQuestionViewModel))]
    public class SingleOptionLinkedToListQuestionViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            base.Setup();
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
        }
        
    }
}

using MvvmCross.Base;
using MvvmCross.Tests;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    public class BaseMvvmCrossTest : MvxIoCSupportingTest
    {
        public BaseMvvmCrossTest()
        {
            base.Setup();
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(Stub.MvxMainThreadAsyncDispatcher());
        }
   
    }
}

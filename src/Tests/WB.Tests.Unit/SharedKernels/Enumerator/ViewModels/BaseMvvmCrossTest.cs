using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
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
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
   
    }
}

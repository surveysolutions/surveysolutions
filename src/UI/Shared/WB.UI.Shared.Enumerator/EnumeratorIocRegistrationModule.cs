using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.WebBrowser;
using MvvmCross.Views;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Enumerator
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class EnumeratorIocRegistrationModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindToMethod<IMvxMessenger>(() => Mvx.IoCProvider.Resolve<IMvxMessenger>());
            registry.BindToMethod<IMvxNavigationService>(() => Mvx.IoCProvider.Resolve<IMvxNavigationService>());
            registry.BindToMethod<IMvxMainThreadAsyncDispatcher>(() => Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>());
            registry.BindToMethod<IMvxWebBrowserTask>(() => Mvx.IoCProvider.Resolve<IMvxWebBrowserTask>());
            registry.BindToMethod<IMvxAndroidCurrentTopActivity>(() => Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>());
            registry.BindToMethod<IMvxViewsContainer>(() => Mvx.IoCProvider.Resolve<IMvxViewsContainer>());
        }
    }
}

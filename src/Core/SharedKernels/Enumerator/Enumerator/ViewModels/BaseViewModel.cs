using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        protected readonly IPrincipal principal;
        protected readonly IViewModelNavigationService viewModelNavigationService;

        protected BaseViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public virtual bool IsAuthenticationRequired => true;

        public override void Prepare()
        {
            base.Prepare();
            BaseViewModelSetupMethods.Prepare(this.IsAuthenticationRequired, this.principal, this.viewModelNavigationService);
        }

        protected override void ReloadFromBundle(IMvxBundle parameters)
        {
            base.ReloadFromBundle(parameters);
            BaseViewModelSetupMethods.ReloadStateFromBundle(this.principal, parameters);
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            BaseViewModelSetupMethods.SaveStateToBundle(this.principal, bundle);
        }

        // it's much more performant, as original extension call new Action<...> on every call
        protected void RaiseAndSetIfChanged<TReturn>(ref TReturn backingField, TReturn newValue, 
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<TReturn>.Default.Equals(backingField, newValue)) return;
            backingField = newValue;
            this.RaisePropertyChanged(propertyName);
        }
    }
}

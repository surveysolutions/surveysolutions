using System;
using System.Runtime.CompilerServices;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel, IDisposable
    {
        // it's much more performant, as original extension call new Action<...> on every call
        protected void RaiseAndSetIfChanged<TReturn>(ref TReturn backingField, TReturn newValue, 
            [CallerMemberName] string propertyName = null)
        {
            SetProperty(ref backingField, newValue, propertyName);
        }

        public virtual void Dispose()
        {
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            if(viewFinishing)
                Dispose();
            
            base.ViewDestroy(viewFinishing);
        }
    }
}

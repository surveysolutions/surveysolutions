using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class ConnectedDeviceSynchronizationViewModel : SynchronizationViewModelBase
    {
        public override bool  IsSynchronizationInfoShowed => true;

        public override bool HasUserAnotherDevice => false;

        public override bool CanBeManaged => false;

        protected override void OnSyncCompleted()
        {
        }
    }
}

using System;
using WB.UI.Capi.Implementations.TabletInformation;

namespace WB.UI.Capi.Services
{
    public interface ITabletInformationSender {
        event EventHandler<InformationPackageEventArgs> InformationPackageCreated;
        event EventHandler ProcessCanceled;
        event EventHandler ProcessFinished;
        void Run();
        void Cancel();
    }
}
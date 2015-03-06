using System;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender
{
    public interface ITabletInformationSender {
        event EventHandler<InformationPackageEventArgs> InformationPackageCreated;
        event EventHandler<InformationPackageCancellationEventArgs> ProcessCanceled;
        event EventHandler ProcessFinished;
        void Run();
        void Cancel();
    }
}
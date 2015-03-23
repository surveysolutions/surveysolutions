using System;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender
{
    public interface ITabletInformationSender {
        event EventHandler<InformationPackageEventArgs> InformationPackageCreated;
        event EventHandler<InformationPackageCancellationEventArgs> ProcessCanceled;
        event EventHandler ProcessFinished;
        Task Run();
        void Cancel();
    }
}
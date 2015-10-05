using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.TabletInformationSender
{
    public interface ITabletInformationSender {
        event EventHandler<InformationPackageEventArgs> InformationPackageCreated;
        event EventHandler<InformationPackageCancellationEventArgs> ProcessCanceled;
        event System.EventHandler ProcessFinished;
        Task Run();
        void Cancel();
    }
}
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

public class RemoteTabletSettingsResponse : ICommunicationMessage
{
    public RemoteTabletSettingsApiView Settings { get; set; }
}
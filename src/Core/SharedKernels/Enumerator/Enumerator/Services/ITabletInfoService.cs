using System;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ITabletInfoService
    {
        void Store(DeviceInfoApiView deviceInfo);
        void Remove(int? recordId);

        DeviceInfoStorageView GetTopRecordForSync();
    }
}

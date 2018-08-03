using System;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class TabletInfoService : ITabletInfoService
    {
        private readonly IPlainStorage<DeviceInfoView, int?> deviceInfoViewRepository;
        private readonly ISerializer serializer;

        public TabletInfoService(IPlainStorage<DeviceInfoView, int?> deviceInfoViewRepository, 
            ISerializer serializer)
        {
            this.deviceInfoViewRepository = deviceInfoViewRepository;
            this.serializer = serializer;
        }

        
        public class DeviceInfoView : IPlainStorageEntity<int?>
        {
            [PrimaryKey, Unique, AutoIncrement]
            public int? Id { get; set; }

            public string Json { get; set; }
        }

        public void Store(DeviceInfoApiView deviceInfo)
        {
            var deviceInfoStorageView = new DeviceInfoStorageView()
            {
                DeviceInfo = deviceInfo
            };

            var json = serializer.Serialize(deviceInfoStorageView);
            deviceInfoViewRepository.Store(new DeviceInfoView { Json = json });
        }

        public void Remove(int? recordId)
        {
            deviceInfoViewRepository.Remove(recordId);
        }

        public DeviceInfoStorageView GetTopRecordForSync()
        {
            var item = deviceInfoViewRepository.FirstOrDefault();
            if (item == null)
            {
                return null;
            }
            var entity = serializer.Deserialize<DeviceInfoStorageView>(item.Json);
            entity.Id = item.Id;
            return entity;
        }
    }

    public class DeviceInfoStorageView
    {
        public int? Id { get; set; }

        public DeviceInfoApiView DeviceInfo { set; get; }
    }
}

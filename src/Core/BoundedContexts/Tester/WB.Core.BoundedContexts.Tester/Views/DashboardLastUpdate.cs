using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Tester.Views
{
    public class DashboardLastUpdate : IPlainStorageEntity
    {
        public string Id { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
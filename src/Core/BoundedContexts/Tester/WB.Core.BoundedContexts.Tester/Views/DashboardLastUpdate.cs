using System;

using WB.Core.BoundedContexts.Tester.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Views
{
    public class DashboardLastUpdate : IPlainStorageEntity
    {
        public string Id { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
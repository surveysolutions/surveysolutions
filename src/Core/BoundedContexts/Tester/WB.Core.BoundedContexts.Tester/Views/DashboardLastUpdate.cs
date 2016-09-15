using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Tester.Views
{
    public class DashboardLastUpdate : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
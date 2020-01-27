using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class SystemLogViewFactory : ISystemLogViewFactory
    {
        private readonly IPlainStorageAccessor<SystemLogEntry> plainStorageAccessor;

        public SystemLogViewFactory(IPlainStorageAccessor<SystemLogEntry> plainStorageAccessor)
        {
            this.plainStorageAccessor = plainStorageAccessor;
        }

        public SystemLog GetLog(SystemLogFilter filter)
        {
            return this.plainStorageAccessor.Query(queryable =>
            {
                IQueryable<SystemLogEntry> query = queryable;

                if (filter.SortOrder == null)
                {
                    filter.SortOrder = new[] { new OrderRequestItem() { Field = "LogDate", Direction = OrderDirection.Desc} };
                }

                var totalCount = query.Count();

                var items = query.OrderUsingSortExpression(filter.SortOrder.GetOrderRequestString())
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new SystemLog() {Items = items.Select(ToViewModel), TotalCount = totalCount};
            });
        }

        private SystemLogItem ToViewModel(SystemLogEntry entry)
        {
            return new SystemLogItem()
            {
                Id = entry.Id,
                UserId = entry.UserId,
                UserName = entry.UserName,
                LogDate = entry.LogDate,
                Log = entry.Log,
                Type = Enum.GetName(typeof(LogEntryType), entry.Type) 
            };
        }
    }
}

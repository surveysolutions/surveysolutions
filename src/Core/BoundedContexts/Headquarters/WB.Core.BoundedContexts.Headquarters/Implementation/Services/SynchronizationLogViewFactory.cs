using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class SynchronizationLogViewFactory : ISynchronizationLogViewFactory
    {
        private readonly IPlainStorageAccessor<SynchronizationLogItem> plainStorageAccessor;

        public SynchronizationLogViewFactory(IPlainStorageAccessor<SynchronizationLogItem> plainStorageAccessor)
        {
            this.plainStorageAccessor = plainStorageAccessor;
        }

        public SynchronizationLog GetLog(SynchronizationLogFilter filter)
        {
            return this.plainStorageAccessor.Query(queryable =>
            {
                IQueryable<SynchronizationLogItem> query = queryable;

                if (!string.IsNullOrEmpty(filter.DeviceId))
                {
                    query = query.Where(x => x.DeviceId == filter.DeviceId);
                }

                if (!string.IsNullOrEmpty(filter.InterviewerName))
                {
                    query = query.Where(x => x.InterviewerName == filter.InterviewerName);
                }

                if (filter.ToDateTime.HasValue)
                {
                    query = query.Where(x => x.LogDate <= filter.ToDateTime.Value.AddDays(1));
                }

                if (filter.FromDateTime.HasValue)
                {
                    query = query.Where(x => x.LogDate >= filter.FromDateTime);
                }

                if (filter.Type.HasValue)
                {
                    query = query.Where(x => x.Type == filter.Type);
                }

                if (filter.SortOrder == null)
                {
                    filter.SortOrder = new[] { new OrderRequestItem() { Field = "LogDate", Direction = OrderDirection.Desc} };
                }

                var totalCount = query.LongCount();

                var items = query.OrderUsingSortExpression(filter.SortOrder.GetOrderRequestString())
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new SynchronizationLog() {Items = items, TotalCount = totalCount};
            });
        }

        public UsersView GetInterviewers(int pageSize, string searchBy)
        {
            return this.plainStorageAccessor.Query(queryable =>
            {
                var query = queryable;

                if (!string.IsNullOrEmpty(searchBy))
                    query = queryable.Where(x => x.InterviewerName.ToLower().Contains(searchBy.ToLower()));

                var interviewersByQuery = query.GroupBy(x => new { x.InterviewerId, x.InterviewerName })
                                                       .Where(x => x.Count() > 0)
                                                       .Select(x => new UsersViewItem { UserId = x.Key.InterviewerId, UserName = x.Key.InterviewerName })
                                                       .OrderBy(x => x.UserName).ToList();

                return new UsersView()
                {
                    Users = interviewersByQuery.Take(pageSize),
                    TotalCountByQuery = interviewersByQuery.Count()
                };
            });
        }

        public SynchronizationLogDevicesView GetDevices(int pageSize, string searchBy)
        {
            return this.plainStorageAccessor.Query(queryable =>
            {
                var query = queryable;

                if (!string.IsNullOrEmpty(searchBy))
                    query = queryable.Where(x => x.DeviceId.ToLower().Contains(searchBy.ToLower()));

                var devicesByQuery = query.GroupBy(x => x.DeviceId).Where(x => x.Count() > 0).Select(x => x.Key).ToList();

                return new SynchronizationLogDevicesView
                {
                    Devices = devicesByQuery.Take(pageSize),
                    TotalCountByQuery = devicesByQuery.Count()
                };
            });
        }
    }
}

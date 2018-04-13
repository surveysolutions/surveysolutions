using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public static class ReportViewHelpers
    {
        public static string AsColumnName(this Answer a)
        {
            // to be possible to use as unique id
            return "col_" + ((int)a.GetParsedValue()).ToString().Replace("-", "_");
        }

        public static ReportView ApplyOrderAndPaging(this ReportView report,
            IEnumerable<OrderRequestItem> orders, int page, int pageSize)
        {
            var result = new ReportView
            {
                Columns = report.Columns,
                Headers = report.Headers,
                TotalCount = report.Data.Length,
                Totals = report.Totals
            };

            bool thenBy = false;
            var data = report.Data.OrderBy(d => true);

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    var field = order.Field.Trim();

                    var index = Array.IndexOf(report.Columns, field);
                    if (index >= 0)
                    {
                        data = OrderBy(d => d[index]);
                    }

                    IOrderedEnumerable<object[]> OrderBy<T>(Func<object[], T> a)
                    {
                        if (thenBy)
                        {
                            return order.Direction == OrderDirection.Asc
                                ? data.ThenBy(a)
                                : data.ThenByDescending(a);
                        }

                        thenBy = true;
                        return order.Direction == OrderDirection.Asc
                            ? data.OrderBy(a)
                            : data.OrderByDescending(a);
                    }
                }
            }

            result.Data = data.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
            return result;
        }
    }
}

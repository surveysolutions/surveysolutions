using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Utility
{
    public static class StringUtil
    {
        public static List<OrderRequestItem> ParseOrderRequestString(string value)
        {
            var result = new List<OrderRequestItem>();
            if (String.IsNullOrWhiteSpace(value)) return result;

            var list = value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in list)
            {
                var ori = new OrderRequestItem {Field = s, Direction = OrderDirection.Asc};
                if (s.EndsWith("Desc"))
                {
                    ori.Direction = OrderDirection.Desc;
                    ori.Field = s.Substring(0, s.Length - 4);
                }
                result.Add(ori);
            }
            return result;
        }

        public static string GetOrderRequestString(List<OrderRequestItem> orders)
        {
            return String.Join(",", orders.Select(o => o.Field + (o.Direction == OrderDirection.Asc ? "" : "Desc")));
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.UI.Headquarters.API
{
    public static class ResponseHelpers
    {
        [SuppressMessage("ReSharper", "LocalizableElement")]
        public static JObject AsDataTablesJson(this ReportView report, int? draw)
        {
            void FillItemData(JObject obj, object[] item)
            {
                for (var index = 0; index < report.Columns.Length; index++)
                {
                    var header = report.Columns[index];
                    obj[header] = item?[index]?.ToString();
                }
            }

            var response = new JObject
            {
                {"recordsTotal", report.TotalCount},

                // ReSharper disable once CoVariantArrayConversion
                {"headers", new JArray(report.Headers)},
                {"recordsFiltered", report.TotalCount}
            };

            if (draw.HasValue)
            {
                response.Add("draw", draw.Value);
            }

            var array = new JArray();

            foreach (var item in report.Data)
            {
                var obj = new JObject();
                FillItemData(obj, item);
                array.Add(obj);
            }

            response.Add("data", array);

            if (report.Totals != null)
            {
                var obj = new JObject();
                FillItemData(obj, report.Totals);
                response.Add("totalRow", obj);
            }

            return response;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class KindOfDataItem
    {
        public KindOfDataItem(string code, string title)
        {
            Code = code;
            Title = title;
        }

        public string Code { get; set; }
        public string Title { get; set; }
    }

    public static class KindOfDataProvider
    {
        public static List<KindOfDataItem> GetKindOfDataItems()
        {
            var codes = GetKindOfDataCodes();
            return codes.Select(code =>
            {
                var item = new KindOfDataItem(code, GetKindOfDataTitleByCode(code) ?? "Unknown");
                return item;
            }).ToList();
        }

        public static string? GetKindOfDataTitleByCode(string code)
        {
            return Resources.KindOfData.ResourceManager.GetString(code);
        }

        public static List<string> GetKindOfDataCodes()
        {
            return new List<string>()
            {
                "adm",
                "agg",
                "cen",
                "cli",
                "evn",
                "obs",
                "pro",
                "ssd",
                "tbd",
            };
        }
    }
}

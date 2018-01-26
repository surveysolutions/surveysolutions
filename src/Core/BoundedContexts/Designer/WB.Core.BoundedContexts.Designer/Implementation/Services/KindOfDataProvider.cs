using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class KindOfDataItem
    {
        public string Code { get; set; }
        public string Title { get; set; }
    }

    public static class KindOfDataProvider
    {
        public static List<KindOfDataItem> GetKindOfDataItems()
        {
            var codes = GetKindOfDataCodes();
            return codes.Select(code => new KindOfDataItem()
            {
                Code = code,
                Title = GetKindOfDataTitleByCode(code)
            }).ToList();
        }

        public static string GetKindOfDataTitleByCode(string code)
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
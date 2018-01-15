using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class KindOfDataItem
    {
        public KindOfData Code { get; set; }
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

        public static string GetKindOfDataTitleByCode(KindOfData code)
        {
            return Resources.KindOfData.ResourceManager.GetString(code.ToString());
        }

        public static List<KindOfData> GetKindOfDataCodes()
        {
            return new List<KindOfData>()
            {
                KindOfData.SampleSurveyData,
                KindOfData.CensusEnumerationData,
                KindOfData.AdministrativeRecordsData,
                KindOfData.AggregateData,
                KindOfData.ClinicalData,
                KindOfData.EventTransactionData,
                KindOfData.ObservationDataRatings,
                KindOfData.ProcessProducedData,
                KindOfData.TimeBudgetDiaries
            };
        }
    }
}
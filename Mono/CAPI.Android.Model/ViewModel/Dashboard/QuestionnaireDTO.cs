using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model.ProjectionStorage;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class QuestionnaireDTO : DenormalizerRow
    {
        public QuestionnaireDTO(Guid id, Guid responsible, Guid survey, SurveyStatus status, IList<FeaturedItem> properties)
        {
            Id = id.ToString();
            Status = status.PublicId.ToString();
            Properties = GetJsonData(properties);
            Responsible = responsible.ToString();
            Survey = survey.ToString();
        }

        public QuestionnaireDTO()
        {
        }

        public string Status { get; set; }
        public string Responsible { get; set; }
        public string Survey { get; set; }
        public string Properties { get; set; }

        public DashboardQuestionnaireItem GetDashboardItem()
        {
            return new DashboardQuestionnaireItem(
                Guid.Parse(Id), GetTypedStatus(),
                GetProperties());
        }

        private SurveyStatus GetTypedStatus()
        {
            return SurveyStatus.GetStatusByIdOrDefault(Guid.Parse(Status));
        }


        private string GetJsonData(IList<FeaturedItem> payload)
        {
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                   {
                                                       TypeNameHandling = TypeNameHandling.Objects,
                                                       ContractResolver = new CriteriaContractResolver()/*,
                                                           Converters =
                                                               new List<JsonConverter>() {new ItemPublicKeyConverter()}*/
                                                   });
            Console.WriteLine(data);
            return data;
        }

        private IList<FeaturedItem> GetProperties()
        {
            if (string.IsNullOrEmpty(Properties))
                return new List<FeaturedItem>();
            return JsonConvert.DeserializeObject<IList<FeaturedItem>>(Properties,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    ContractResolver = new CriteriaContractResolver()/*,
                    Converters =
                        new List<JsonConverter>() { new ItemPublicKeyConverter() }*/
                });
        }
    }
}
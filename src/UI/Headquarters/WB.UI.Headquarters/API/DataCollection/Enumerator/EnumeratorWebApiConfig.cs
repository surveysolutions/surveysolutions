using System.ComponentModel;
using System.Web.Http;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.DataCollection.Enumerator;
using WB.UI.Headquarters.API.DataCollection.Enumerator.v1;

namespace WB.UI.Headquarters.API.DataCollection.Enumerator
{
    [Localizable(false)]
    public class EnumeratorWebApiConfig
    {
#pragma warning disable 4014
        public static void Register(HttpConfiguration config)
        {
            config.TypedRoute("api/enumerator/logs", c => c.Action<LogsApiController>(x => x.Post()));

            config.TypedRoute("api/enumerator/v1/categories/{id}", c => c.Action<ReusableCategoriesApiV1Controller>(x => x.Get(Param.Any<string>())));
        }
#pragma warning restore 4014
    }
}

using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class ReusableCategoriesApiV1Controller : ReusableCategoriesControllerBase
    {
        public ReusableCategoriesApiV1Controller(IReusableCategoriesStorage reusableCategoriesStorage, IQuestionnaireStorage questionnaireStorage)
            : base(reusableCategoriesStorage, questionnaireStorage)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetReusableCategories)]
        public override HttpResponseMessage Get(string id) => base.Get(id);

    }
}

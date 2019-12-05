using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class ReusableCategoriesApiV2Controller : ReusableCategoriesControllerBase
    {
        public ReusableCategoriesApiV2Controller(IReusableCategoriesStorage reusableCategoriesStorage, IQuestionnaireStorage questionnaireStorage) 
            : base(reusableCategoriesStorage, questionnaireStorage)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetReusableCategories)]
        public override HttpResponseMessage Get(string id) => base.Get(id);

    }
}

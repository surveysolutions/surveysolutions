using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class UsersApiV2Controller : UsersControllerBase
    {
        public UsersApiV2Controller(
            IGlobalInfoProvider globalInfoProvider,
            IUserViewFactory userViewFactory,
            IUserWebViewFactory userInfoViewFactory) : base(
                globalInfoProvider: globalInfoProvider,
                userViewFactory: userViewFactory,
                userInfoViewFactory: userInfoViewFactory)
        {
        }

        [HttpGet]
        public override InterviewerApiView Current() => base.Current();

        [HttpGet]
        public override bool HasDevice() => base.HasDevice();
    }
}
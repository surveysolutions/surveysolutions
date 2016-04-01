using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    [ProtobufJsonSerializer]
    [Obsolete("Since v. 5.7")]
    public class UsersApiV1Controller : UsersControllerBase
    {
        public UsersApiV1Controller(
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
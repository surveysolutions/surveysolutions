using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.UI.Headquarters.Code;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class UsersApiV2Controller : UsersControllerBase
    {
        public UsersApiV2Controller(
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory) : base(
                authorizedUser: authorizedUser,
                userViewFactory: userViewFactory)
        {
        }

        [HttpGet]
        public override InterviewerApiView Current() => base.Current();

        [HttpGet]
        public override bool HasDevice() => base.HasDevice();
    }
}
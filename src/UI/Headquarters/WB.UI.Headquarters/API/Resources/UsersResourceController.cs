using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Resources
{
    [TokenValidationAuthorization]
    [RoutePrefix("api/resources/users/v1")]
    [HeadquarterFeatureOnly]
    public class UsersResourceController : ApiController
    {
        private readonly IUserWebViewFactory viewFactory;

        public UsersResourceController(IUserWebViewFactory viewFactory)
        {
            this.viewFactory = viewFactory;
        }

        [Route("{id}", Name = "api.userDetails")]
        [HttpGet]
        public UserWebView Details(string id)
        {
            UserWebView userView = viewFactory.Load(new UserWebViewInputModel(Guid.Parse(id)));
            return userView;
        }

        [Route("validateSupervisor")]
        [HttpGet]
        public HttpResponseMessage ValidateSupervisor(string login, string passwordHash)
        {
            var isValid = Membership.ValidateUser(login, passwordHash);

            if (!isValid)
            {
                return this.Request.CreateResponse(new SupervisorValidationResult
                {
                    isValid = false
                });
            }

            UserWebView userDocument = this.viewFactory.Load(new UserWebViewInputModel(login, passwordHash));
            string detailsUrl = this.Url.Route("api.userDetails", new { id = userDocument.PublicKey.FormatGuid()});
            var result = new SupervisorValidationResult
            {
                isValid = isValid && userDocument.Roles.Contains(UserRoles.Supervisor),
                userId =  userDocument.PublicKey.FormatGuid(),
                userDetailsUrl = new Uri(this.Request.RequestUri, detailsUrl).ToString()
            };

            return this.Request.CreateResponse(result);
        }
    }
}
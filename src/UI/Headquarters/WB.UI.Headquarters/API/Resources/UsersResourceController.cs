using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;
using WB.UI.Headquarters.API.Attributes;

namespace WB.UI.Headquarters.API.Resources
{
    [TokenValidationAuthorizationAttribute]
    [RoutePrefix("api/resources/users/v1")]
    [HeadquarterFeatureOnly]
    public class UsersResourceController : ApiController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;

        public UsersResourceController(IViewFactory<UserViewInputModel, UserView> viewFactory)
        {
            this.viewFactory = viewFactory;
        }

        [Route("{id}", Name = "api.userDetails")]
        [HttpGet]
        public UserView Details(string id)
        {
            UserView userView = viewFactory.Load(new UserViewInputModel(Guid.Parse(id)));
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

            UserView userDocument = this.viewFactory.Load(new UserViewInputModel(login, passwordHash));
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
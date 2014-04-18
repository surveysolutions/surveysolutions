using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.UI.Headquarters.API.Resources
{
    [RoutePrefix("api/resources/users/v1")]
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
                isValid = isValid,
                userId =  userDocument.PublicKey.FormatGuid(),
                userDetailsUrl = new Uri(this.Request.RequestUri, detailsUrl).ToString()
            };

            return this.Request.CreateResponse(result);
        }
    }
}
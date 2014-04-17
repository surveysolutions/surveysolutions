using System;
using System.Linq;
using System.Web.Http;
using System.Web.Security;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.UI.Headquarters.API.Resources
{
    [RoutePrefix("api/resources/users/v1")]
    public class UsersResourceController : ApiController
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public UsersResourceController(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }

        [Route("{id}", Name = "api.userDetails")]
        [HttpGet]
        public UserDocument Details(string id)
        {
            return users.GetById(id);
        }

        [Route("validateSupervisor")]
        [HttpGet]
        public SupervisorValidationResult ValidateSupervisor(string login, string passwordHash)
        {
            var isValid = Membership.ValidateUser(login, passwordHash);

            if (!isValid)
            {
                return new SupervisorValidationResult
                {
                    isValid = false
                };
            }

            var userDocument = this.users.Query(_ => _.First(x => x.UserName == login));
            string detailsUrl = this.Url.Route("api.userDetails", new { id = userDocument.PublicKey.FormatGuid()});
            var result = new SupervisorValidationResult
            {
                isValid = !userDocument.IsLockedByHQ && !userDocument.IsDeleted && userDocument.Roles.Contains(UserRoles.Supervisor),
                userId =  userDocument.PublicKey.FormatGuid(),
                userDetailsUrl = new Uri(this.Request.RequestUri, detailsUrl).ToString()
            };

            return result;
        }
    }
}
using System.Linq;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
            UserDocument userDocument = users.Query(_ => _.Where(user => user.UserName == login && user.Password == passwordHash)).FirstOrDefault();

            if (userDocument == null)
            {
                return new SupervisorValidationResult
                {
                    isValid = false
                };
            }

            var result = new SupervisorValidationResult
            {
                isValid = !userDocument.IsLocked && !userDocument.IsDeleted && userDocument.Roles.Contains(UserRoles.Supervisor),
                userId =  userDocument.PublicKey.FormatGuid()
            };

            return result;
        }
    }
}
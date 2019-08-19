using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API.PublicApi
{
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true)]
    [RoutePrefix("api/v1")]
    public class UsersController : BaseApiServiceController
    {
        private readonly IUserViewFactory usersFactory;
        private readonly HqUserManager userManager;
        private readonly IAuditLogService auditLogService;

        public UsersController(ILogger logger,
            IUserViewFactory usersFactory,
            HqUserManager userManager,
            IAuditLogService auditLogService)
            : base(logger)
        {
            this.usersFactory = usersFactory;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
        }

        /// <summary>
        /// Gets list of supervisors
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        [HttpGet]
        [Route("supervisors")]
        public UserApiView Supervisors(int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetUsersByRole(offset, limit, null, null, false, UserRoles.Supervisor));

        /// <summary>
        /// Gets list of interviewers in the specific supervisor team
        /// </summary>
        /// <param name="supervisorId">Id of supervisor</param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        [HttpGet]
        [Route("supervisors/{supervisorId:guid}/interviewers")]
        public UserApiView Interviewers(Guid supervisorId, int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetInterviewers(offset, limit, null, null, false, null, supervisorId));

        /// <summary>
        /// Gets detailed info about single user
        /// </summary>
        /// <param name="id">User id</param>
        [HttpGet]
        [Route("supervisors/{id:guid}")]
        [Route("users/{id:guid}")]
        public UserApiDetails Details(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new UserApiDetails(user);
        }


        /// <summary>
        /// Gets detailed info about single interviewer
        /// </summary>
        /// <param name="id">User id</param>
        /// <response code="200"></response>
        /// <response code="404">Interviewer was not found</response>
        [HttpGet]
        [Route("interviewers/{id:guid}")]
        public InterviewerUserApiDetails InterviewerDetails(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null || !user.Roles.Contains(UserRoles.Interviewer))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new InterviewerUserApiDetails(user);
        }

        /// <summary>
        /// Archives interviewer or supervisor with all his interviewers
        /// </summary>
        /// <param name="id">User id</param>
        /// <response code="200">User archived</response>
        /// <response code="400">User id cannot be parsed</response>
        /// <response code="404">User with provided id does not exist</response>
        /// <response code="406">User is not an interviewer or supervisor</response>
        [HttpPatch]
        [Route("users/{id}/archive")]
        public async Task<IHttpActionResult> Archive(string id)
        {
            Guid userGuid;
            if (!Guid.TryParse(id, out userGuid))
            {
                return this.BadRequest();
            }

            var user = this.usersFactory.GetUser(new UserViewInputModel(userGuid));
            if (user == null)
            {
                return this.NotFound();
            }
            if (!(user.Roles.Contains(UserRoles.Interviewer) || user.Roles.Contains(UserRoles.Supervisor)))
            {
                return this.StatusCode(HttpStatusCode.NotAcceptable);

            }

            if (user.IsSupervisor())
            {
                await this.userManager.ArchiveSupervisorAndDependentInterviewersAsync(userGuid);
            }
            else
            {
                await this.userManager.ArchiveUsersAsync(new[] { userGuid });
            }
            return this.Ok();
        }

        /// <summary>
        /// Unarchives single user
        /// </summary>
        /// <param name="id">User id</param>
        /// <response code="200">User unarchived</response>
        /// <response code="400">User id cannot be parsed</response>
        /// <response code="404">User with provided id does not exist</response>
        /// <response code="406">User is not an interviewer or supervisor</response>
        [HttpPatch]
        [Route("users/{id}/unarchive")]
        public async Task<IHttpActionResult> UnArchive(string id)
        {
            Guid userGuid;
            if (!Guid.TryParse(id, out userGuid))
            {
                return this.BadRequest();
            }

            var user = this.usersFactory.GetUser(new UserViewInputModel(userGuid));
            if (user == null)
            {
                return this.NotFound();
            }

            if (!(user.Roles.Contains(UserRoles.Interviewer) || user.Roles.Contains(UserRoles.Supervisor)))
            {
                return this.StatusCode(HttpStatusCode.NotAcceptable);
            }

            await this.userManager.UnarchiveUsersAsync(new[] { userGuid });
            return this.Ok();
        }

        /// <summary>
        /// Returns audit log records for interviewer.
        /// You can specify "start" and "end" parameters in query string to get range results.
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="start" >Start datetime. If isn't specified then return data for last 7 days.</param>
        /// <param name="end">End datetime. If isn't specified then get data for 7 days from start data.</param>
        [HttpGet]
        [Route("interviewers/{id:guid}/actions-log")]
        public AuditLogRecordApiView[] ActionsLog(Guid id, DateTime? start = null, DateTime? end = null)
        {
            DateTime startDate = start ?? DateTime.UtcNow.AddDays(-7);
            DateTime endDate = end ?? startDate.AddDays(7);

            var records = auditLogService.GetRecords(id, startDate, endDate, showErrorMessage: false);
            return records.Select(record => new AuditLogRecordApiView()
            {
                Time = record.Time,
                Message = record.Message,
            }).ToArray();
        }
    }
}

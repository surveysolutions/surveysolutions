using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Authorize(Roles = "ApiUser, Administrator")]
    [Route("api/v1")]
    [PublicApiJson]
    public class UsersController : ControllerBase
    {
        private readonly IUserViewFactory usersFactory;
        private readonly IUserArchiveService userManager;
        private readonly IAuditLogService auditLogService;

        public UsersController(IUserViewFactory usersFactory,
            IUserArchiveService userManager,
            IAuditLogService auditLogService)
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
        public ActionResult<UserApiDetails> Details(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null)
            {
                return NotFound();
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
        public ActionResult<InterviewerUserApiDetails> InterviewerDetails(Guid id)
        {
            var user = this.usersFactory.GetUser(new UserViewInputModel(id));

            if (user == null || !user.Roles.Contains(UserRoles.Interviewer))
            {
                return NotFound();
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
        public async Task<ActionResult> Archive(string id)
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
                return StatusCode((int) HttpStatusCode.NotAcceptable);
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
        public async Task<ActionResult> UnArchive(string id)
        {
            if (!Guid.TryParse(id, out var userGuid))
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
                return StatusCode((int) HttpStatusCode.NotAcceptable);
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

#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Headquarter)]
    [Route("api/v1")]
    [PublicApiJson]
    public class UsersController : ControllerBase
    {
        private readonly IUserViewFactory usersFactory;
        private readonly IUserArchiveService archiveService;
        private readonly IAuditLogService auditLogService;
        private readonly UserManager<HqUser> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly ISystemLog systemLog;

        public UsersController(IUserViewFactory usersFactory,
            IUserArchiveService archiveService,
            IAuditLogService auditLogService,
            UserManager<HqUser> userManager, IUnitOfWork unitOfWork,
            ISystemLog systemLog)
        {
            this.usersFactory = usersFactory;
            this.archiveService = archiveService;
            this.auditLogService = auditLogService;
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.systemLog = systemLog;
        }

        /// <summary>
        /// Gets list of supervisors
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        [HttpGet]
        [Route("supervisors")]
        public UserApiView Supervisors(int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetUsersByRole(offset, limit, string.Empty, string.Empty, false, UserRoles.Supervisor));

        /// <summary>
        /// Gets list of interviewers in the specific supervisor team
        /// </summary>
        /// <param name="supervisorId">Id of supervisor</param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        [HttpGet]
        [Route("supervisors/{supervisorId:guid}/interviewers")]
        public UserApiView Interviewers(Guid supervisorId, int limit = 10, int offset = 1)
            => new UserApiView(this.usersFactory.GetInterviewers(offset, limit, string.Empty, string.Empty, false, null, supervisorId));

        /// <summary>
        /// Gets detailed info about single user
        /// </summary>
        /// <param name="id">User id or user name or user email</param>
        [HttpGet]
        [Route("supervisors/{id:guid}")]
        [Route("users/{id}")]
        public ActionResult<UserApiDetails> Details(string id)
        {
            var userViewInputModel = new UserViewInputModel();
            
            if (Guid.TryParse(id, out Guid userId))
            {
                userViewInputModel.PublicKey = userId;
            }
            else
            {
                userViewInputModel.UserName = id;
            }
            
            var user = this.usersFactory.GetUser(userViewInputModel);

            
            if (user == null)
            {
                user = this.usersFactory.GetUser(new UserViewInputModel {UserEmail = id});
                if (user == null)
                {
                    return NotFound();
                }
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
        [ObservingNotAllowed]
        public async Task<ActionResult> Archive(string id)
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
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            if (user.IsSupervisor())
            {
                await this.archiveService.ArchiveSupervisorAndDependentInterviewersAsync(userGuid);
            }
            else
            {
                await this.archiveService.ArchiveUsersAsync(new[] { userGuid });
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
        /// <response code="409">User cannot be unarchived</response>
        [HttpPatch]
        [Route("users/{id}/unarchive")]
        [ObservingNotAllowed]
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
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            try
            {
                await this.archiveService.UnarchiveUsersAsync(new[] { userGuid });
            }
            catch (UserArchiveException e)
            {
                return StatusCode(StatusCodes.Status409Conflict, e.Message);
            }

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

            var records = auditLogService.GetRecords(id, startDate, endDate);
            return records.Select(record => new AuditLogRecordApiView()
            {
                Time = record.Time,
                Message = record.Message,
            }).ToArray();
        }

        /// <summary>
        /// Creates new user with specified role.
        /// </summary>
        /// <param name="model"></param>
        /// <response code="400">User cannot be created.</response>
        /// <response code="200">Created user id.</response>
        [HttpPost]
        [Route("users")]
        [ObservingNotAllowed]
        public async Task<ActionResult<UserCreationResult>> Register([FromBody, BindRequired]RegisterUserModel model)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    $@"Invalid parameter or property: {string.Join(',',ModelState.Keys.ToList())}");
            
            if (!Enum.IsDefined(typeof(UserRoles), (UserRoles)model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Trying to create user with unknown role");
            }
            
            var result = new UserCreationResult();
            var createdUserRole = model.GetDomainRole();

            if (createdUserRole == UserRoles.Administrator)
            {
                ModelState.AddModelError(nameof(model.Role), "Administrator user can not be created with api");
            }

            if (ModelState.IsValid)
            {
                if (createdUserRole == UserRoles.Interviewer && string.IsNullOrWhiteSpace(model.Supervisor))
                {
                    ModelState.AddModelError(nameof(model.Supervisor), "Supervisor name is required for interviewer creation");
                    return BadRequest(ModelState);
                }

                var createdUser = new HqUser
                {
                    UserName = model.UserName.Trim(),
                    NormalizedUserName = model.UserName.Trim().ToUpper(),
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    NormalizedEmail = model.Email?.Trim().ToUpper()
                };
                var creationResult = await this.userManager.CreateAsync(createdUser, model.Password);

                if (creationResult.Succeeded)
                {
                    if (createdUserRole == UserRoles.Interviewer)
                    {
                        var supervisorId = (await userManager.FindByNameAsync(model.Supervisor)).Id;
                        createdUser.Workspaces.Single().ChangeSupervisorId(supervisorId);
                    }

                    var addResult = await userManager.AddToRoleAsync(createdUser, model.Role.ToString());
                    if (addResult.Succeeded)
                    {
                        this.systemLog.UserCreated(createdUserRole, model.UserName);
                        result.UserId = createdUser.Id.FormatGuid();
                        return Ok(result);
                    }
                    else
                    {
                        unitOfWork.DiscardChanges();
                        foreach (var creationResultError in addResult.Errors)
                        {
                            ModelState.AddModelError(creationResultError.Code, creationResultError.Description);
                        }
                    }
                }
                else
                {
                    unitOfWork.DiscardChanges();
                    foreach (var creationResultError in creationResult.Errors)
                    {
                        ModelState.AddModelError(creationResultError.Code, creationResultError.Description);
                    }
                }
            }

            foreach (var modelState in ModelState.Values) {
                foreach (ModelError error in modelState.Errors) {
                    result.Errors.Add(error.ErrorMessage);
                }
            }

            return BadRequest(result);
        }
    }
}

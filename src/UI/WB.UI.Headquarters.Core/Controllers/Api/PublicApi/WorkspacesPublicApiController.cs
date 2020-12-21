#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.Persistence.Headquarters.Migrations.Workspace;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/workspaces")]
    [Localizable(false)]
    [PublicApiJson]
    public class WorkspacesPublicApiController : ControllerBase
    {
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IMapper mapper;
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspacesCache workspacesCache;
        private readonly IUserRepository users;
        private readonly ILogger<WorkspacesPublicApiController> logger;
        private readonly IAuthorizedUser authorizedUser;

        public WorkspacesPublicApiController(IPlainStorageAccessor<Workspace> workspaces,
            IMapper mapper,
            IWorkspacesService workspacesService,
            IWorkspacesCache workspacesCache,
            IUserRepository users,
            ILogger<WorkspacesPublicApiController> logger,
            IAuthorizedUser authorizedUser)
        {
            this.workspaces = workspaces;
            this.mapper = mapper;
            this.workspacesService = workspacesService;
            this.workspacesCache = workspacesCache;
            this.users = users;
            this.logger = logger;
            this.authorizedUser = authorizedUser;
        }

        /// <summary>
        /// List existing workspaces
        /// </summary>
        [HttpGet]
        [SwaggerResponse(200, Type = typeof(WorkspaceApiView))]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Interviewer)]
        public WorkspacesApiView Index([FromQuery] WorkspacesListFilter filter)
        {
            IEnumerable<WorkspaceApiView> result =
                this.workspaces.Query(_ =>
                        Filter(filter, _)
                            .Skip(filter.Start)
                            .Take(filter.Length)
                            .ToList())
                    .Select(x => mapper.Map<Workspace, WorkspaceApiView>(x));
            int totalCount = this.workspaces.Query(_ => Filter(filter, _).Count());

            return new WorkspacesApiView
            (
                filter.Start,
                filter.Length,
                totalCount,
                result
            );
        }

        private IQueryable<Workspace> Filter(WorkspacesListFilter filter, IQueryable<Workspace> source)
        {
            IQueryable<Workspace> result = source.OrderBy(x => x.Name);

            if (!this.authorizedUser.IsAdministrator)
            {
                var userWorkspaces = this.authorizedUser.Workspaces.ToList();
                result = result.Where(x => userWorkspaces.Contains(x.Name));
            }

            if (filter.UserId.HasValue)
            {
                result = result.Where(x => x.Users.Any(u => u.User.Id == filter.UserId));
            }

            if(!filter.IncludeDisabled)
            {
                result = result.Where(x => x.DisabledAtUtc == null);
            }

            return result;
        }

        /// <summary>
        /// Get single workspace details
        /// </summary>
        [Route("{name}")]
        [SwaggerResponse(404, "Workspace not found")]
        [SwaggerResponse(200, Type = typeof(WorkspaceApiView))]
        [HttpGet]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult<WorkspaceApiView> Details([FromQuery, BindRequired]string name)
        {
            var workspace = this.workspaces.GetById(name);
            if (workspace == null || !this.authorizedUser.Workspaces.Contains(name))
            {
                return NotFound();
            }

            return mapper.Map<WorkspaceApiView>(workspace);
        }

        /// <summary>
        /// Creates new workspace. Accessible only to administrator 
        /// </summary>
        [SwaggerResponse(StatusCodes.Status201Created, "Workspace created", typeof(WorkspaceApiView))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [HttpPost]
        [AuthorizeByRole(UserRoles.Administrator)]
        [ObservingNotAllowed]
        public async Task<ActionResult> Create([FromBody] WorkspaceCreateApiView request)
        {
            if (ModelState.IsValid)
            {
                var workspace = new Workspace(request.Name, request.DisplayName);

                this.workspaces.Store(workspace, null);
                await this.workspacesService.Generate(workspace.Name,
                    DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());
                this.workspacesCache.InvalidateCache();

                return CreatedAtAction("Details", routeValues: new {name = workspace.Name},
                    value: this.mapper.Map<WorkspaceApiView>(workspace));
            }

            return ValidationProblem();
        }

        /// <summary>
        /// Updates new workspace 
        /// </summary>
        /// <response code="204">Workspace updated</response>
        /// <response code="404">Workspace not found</response>
        /// <response code="400">Validation failed</response>
        [HttpPatch]
        [Route("{name}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspace updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult Update([FromQuery, BindRequired]string id, [FromBody] WorkspaceUpdateApiView request)
        {
            if (ModelState.IsValid)
            {
                var existing = this.workspaces.GetById(id);

                if (existing == null || !this.authorizedUser.Workspaces.Contains(id))
                {
                    return NotFound();
                }

                existing.DisplayName = request.DisplayName!;
                this.workspacesCache.InvalidateCache();
                return NoContent();
            }

            return ValidationProblem();
        }

        /// <summary>
        /// Disables specified workspace
        /// </summary>
        /// <response code="204">Workspace disabled</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Workspace not found</response>
        [HttpPost]
        [Route("{name}/disable")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator)]
        public ActionResult Disable([FromQuery, BindRequired]string name)
        {
            var workspace = this.workspaces.GetById(name);
            if (workspace == null)
            {
                return NotFound();
            }

            if (workspace.DisabledAtUtc != null)
            {
                ModelState.AddModelError(nameof(workspace.DisabledAtUtc), $"Workspace {name} is already disabled");
            }
            
            if (name == Workspace.Default.Name)
            {
                ModelState.AddModelError(nameof(name), $"Workspace {name} can not be disabled");
            }
            
            if (ModelState.IsValid)
            {
                workspace.Disable();
                this.logger.LogInformation("Workspace {name} was disabled by {user}", name, this.authorizedUser.UserName);
                this.workspacesCache.InvalidateCache();
                return NoContent();
            }

            return ValidationProblem();
        }
        
        /// <summary>
        /// Enables specified workspace
        /// </summary>
        /// <response code="204">Workspace enabled</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Workspace not found</response>
        [HttpPost]
        [Route("{name}/enable")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator)]
        public ActionResult Enable([FromQuery, BindRequired]string name)
        {
            var workspace = this.workspaces.GetById(name);
            if (workspace == null)
            {
                return NotFound();
            }

            if (workspace.DisabledAtUtc == null)
            {
                ModelState.AddModelError(nameof(workspace.DisabledAtUtc), $"Workspace {name} is already enabled");
            }
            
            if (ModelState.IsValid)
            {
                workspace.Enable();
                this.logger.LogInformation("Workspace {name} was enabled by {user}", name, this.authorizedUser.UserName);
                this.workspacesCache.InvalidateCache();
                return NoContent();
            }

            return ValidationProblem();
        }


        /// <summary>
        /// Assigns workspaces to user.
        /// </summary>
        /// <response code="204">Workspaces list updated</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        [Route("assign")]
        [AuthorizeByRole(UserRoles.Administrator)]
        public async Task<ActionResult> AssignWorkspaces([FromBody] AssignWorkspacesToUserModel model)
        {
            List<Workspace> dbWorkspaces = new();
            foreach (var modelWorkspace in model.Workspaces)
            {
                var workspace = this.workspaces.GetById(modelWorkspace);
                if (workspace == null)
                {
                    ModelState.AddModelError(nameof(model.Workspaces), $"Workspace {modelWorkspace} not found");
                }
                else if (workspace.IsDisabled())
                {
                    ModelState.AddModelError(nameof(model.Workspaces), $"Workspace {modelWorkspace} disabled");
                }
                else
                {
                    dbWorkspaces.Add(workspace);
                }
            }

            var user = await users.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Workspaces), "User not found");
            }

            if (user?.IsArchivedOrLocked == true)
            {
                ModelState.AddModelError(nameof(model.Workspaces), "User is locked");
            }

            if (user != null)
            {
                if (!user.IsInRole(UserRoles.Headquarter) && !user.IsInRole(UserRoles.ApiUser))
                {
                    ModelState.AddModelError(nameof(model.Workspaces),
                        "Only headquarter or api user workspaces can be edited");
                }
                
                if (ModelState.IsValid)
                {
                    workspacesService.AssignWorkspaces(user, dbWorkspaces);
                    return NoContent();
                }
            }

            return ValidationProblem();
        }
    }
}

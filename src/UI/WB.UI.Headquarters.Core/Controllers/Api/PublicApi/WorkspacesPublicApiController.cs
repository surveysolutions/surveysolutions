#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IAuthorizedUser authorizedUser;

        public WorkspacesPublicApiController(IPlainStorageAccessor<Workspace> workspaces,
            IMapper mapper,
            IWorkspacesService workspacesService,
            IWorkspacesCache workspacesCache,
            IUserRepository users,
            IAuthorizedUser authorizedUser)
        {
            this.workspaces = workspaces;
            this.mapper = mapper;
            this.workspacesService = workspacesService;
            this.workspacesCache = workspacesCache;
            this.users = users;
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
                            .Skip(filter.Offset)
                            .Take(filter.Limit)
                            .ToList())
                    .Select(x => mapper.Map<Workspace, WorkspaceApiView>(x));
            int totalCount = this.workspaces.Query(_ => Filter(filter, _).Count());

            return new WorkspacesApiView
            (
                filter.Offset,
                filter.Limit,
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

            return result;
        }

        /// <summary>
        /// Get single workspace details
        /// </summary>
        [Route("{id}")]
        [SwaggerResponse(404, "Workspace not found")]
        [SwaggerResponse(200, Type = typeof(WorkspaceApiView))]
        [HttpGet]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult<WorkspaceApiView> Details(string id)
        {
            var workspace = this.workspaces.GetById(id);
            if (workspace == null || !this.authorizedUser.Workspaces.Contains(id))
            {
                return NotFound();
            }

            return mapper.Map<WorkspaceApiView>(workspace);
        }

        /// <summary>
        /// Creates new workspace. Accessible only to administrator 
        /// </summary>
        /// <response code="201">Workspace created</response>
        /// <response code="400">Validation failed</response>
        [SwaggerResponse(StatusCodes.Status201Created, "Workspace created", typeof(WorkspaceApiView))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [HttpPost]
        [AuthorizeByRole(UserRoles.Administrator)]
        [ObservingNotAllowed]
        public async Task<ActionResult> Create([FromBody] WorkspaceApiView request)
        {
            if (ModelState.IsValid)
            {
                var workspace = new Workspace(request.Name!, request.DisplayName!);

                this.workspaces.Store(workspace, null);
                await this.workspacesService.Generate(workspace.Name,
                    DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());
                this.workspacesCache.InvalidateCache();

                return CreatedAtAction("Details", routeValues: new {id = workspace.Name},
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
        [Route("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspace updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult Update(string id, [FromBody] WorkspaceUpdateApiView request)
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
        /// Updates new workspace. Changing list of workspaces currently is allowed only for Headquarters or Api users.
        /// </summary>
        /// <response code="204">Workspaces updated</response>
        /// <response code="400">Validation failed</response>
        [HttpPut]
        [Route("assign")]
        [AuthorizeByRole(UserRoles.Administrator)]
        public async Task<ActionResult> AssignWorkspaces([FromBody] AssignWorkspacesToUserModel model)
        {
            List<Workspace> dbWorkspaces = new List<Workspace>();
            foreach (var modelWorkspace in model.Workspaces)
            {
                var workspace = this.workspaces.GetById(modelWorkspace);
                if (workspace == null)
                {
                    ModelState.AddModelError(nameof(model.Workspaces), $"Workspace {modelWorkspace} not found");
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

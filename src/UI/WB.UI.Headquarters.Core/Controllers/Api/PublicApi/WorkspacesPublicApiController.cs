#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.Persistence.Headquarters.Migrations.Workspace;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/workspaces")]
    [Localizable(false)]
    [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
    public class WorkspacesPublicApiController : ControllerBase
    {
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IMapper mapper;
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspacesCache workspacesCache;
        private readonly IAuthorizedUser authorizedUser;

        public WorkspacesPublicApiController(IPlainStorageAccessor<Workspace> workspaces,
            IMapper mapper,
            IWorkspacesService workspacesService,
            IWorkspacesCache workspacesCache, 
            IAuthorizedUser authorizedUser)
        {
            this.workspaces = workspaces;
            this.mapper = mapper;
            this.workspacesService = workspacesService;
            this.workspacesCache = workspacesCache;
            this.authorizedUser = authorizedUser;
        }

        /// <summary>
        /// List created workspaces
        /// </summary>
        [HttpGet]
        [SwaggerResponse(200, Type = typeof(WorkspaceApiView))]
        public WorkspacesApiView Index([FromQuery] WorkspacesListFilter filter)
        {
            var userWorkspaces = this.authorizedUser.Workspaces.ToList();
            IEnumerable<WorkspaceApiView> result =
                this.workspaces.Query(_ => _
                    .OrderBy(x => x.Name)
                    .Where(x => userWorkspaces.Contains(x.Name))
                    .Skip(filter.Offset)
                    .Take(filter.Limit)
                    .ToList())
                    .Select(x => mapper.Map<Workspace, WorkspaceApiView>(x));
            int totalCount = this.workspaces.Query(x => x.Count(x => userWorkspaces.Contains(x.Name)));
            
            return new WorkspacesApiView
            (
                filter.Offset,
                filter.Limit,
                totalCount,
                result
            );
        }

        /// <summary>
        /// Get single workspace details
        /// </summary>
        [Route("{id}")]
        [SwaggerResponse(404, "Workspace not found")]
        [SwaggerResponse(200, Type = typeof(WorkspaceApiView))]
        [HttpGet]
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
        /// Creates or updates new workspace 
        /// </summary>
        /// <response code="201">Workspace created</response>
        /// <response code="400">Validation failed</response>
        [SwaggerResponse(StatusCodes.Status201Created, "Workspace created", typeof(WorkspaceApiView))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [HttpPost]
        [AuthorizeByRole(UserRoles.Administrator)]
        public async Task<ActionResult> Create([FromBody]WorkspaceApiView request)
        {
            if (ModelState.IsValid)
            {
                var workspace = new Workspace(request.Name!, request.DisplayName!);
                
                this.workspaces.Store(workspace, null);
                await this.workspacesService.Generate(workspace.Name, DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());
                this.workspacesCache.InvalidateCache();

                return CreatedAtAction("Details", routeValues: new {id = workspace.Name}, value: this.mapper.Map<WorkspaceApiView>(workspace));
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
        public ActionResult Update(string id, [FromBody]WorkspaceUpdateApiView request)
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
    }
}

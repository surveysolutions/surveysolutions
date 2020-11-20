#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/workspaces")]
    [Localizable(false)]
    [Authorize(Roles = "ApiUser, Administrator")]
    public class WorkspacesPublicApiController : ControllerBase
    {
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IMapper mapper;

        public WorkspacesPublicApiController(IPlainStorageAccessor<Workspace> workspaces,
            IMapper mapper)
        {
            this.workspaces = workspaces;
            this.mapper = mapper;
        }

        /// <summary>
        /// List created workspaces
        /// </summary>
        [HttpGet]
        [SwaggerResponse(200, Type = typeof(WorkspaceApiView))]
        public WorkspacesApiView Index([FromQuery] WorkspacesListFilter filter)
        {
            IEnumerable<WorkspaceApiView> workspaces =
                this.workspaces.Query(_ => _
                    .OrderBy(x => x.Name)
                    .Skip(filter.Offset)
                    .Take(filter.Limit)
                    .ToList())
                    .Select(x => mapper.Map<Workspace, WorkspaceApiView>(x));
            int totalCount = this.workspaces.Query(x => x.Count());
            
            return new WorkspacesApiView
            (
                filter.Offset,
                filter.Limit,
                totalCount,
                workspaces
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
            if (workspace == null)
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
        public ActionResult Create([FromBody]WorkspaceApiView request)
        {
            if (ModelState.IsValid)
            {
                var workspace = new Workspace(request.Name!, request.DisplayName!);
                
                this.workspaces.Store(workspace, null);
                return CreatedAtAction("Details", routeValues: new {id = workspace.Name}, value: this.mapper.Map<WorkspaceApiView>(workspace));
            }

            return ValidationProblem();
        }
        
        /// <summary>
        /// Updates new workspace 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspace updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        public ActionResult Update(string id, [FromBody]WorkspaceUpdateApiView request)
        {
            if (ModelState.IsValid)
            {

                var existing = this.workspaces.GetById(id);
                
                existing.DisplayName = request.DisplayName!;
                return NoContent();
            }

            return ValidationProblem();
        }
    }
}

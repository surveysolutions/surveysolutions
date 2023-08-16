#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.Persistence.Headquarters.Migrations.Workspace;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/workspaces")]
    [Localizable(false)]
    [PublicApiJson]
    public class WorkspacesPublicApiController : ControllerBase
    {
        private readonly IWorkspacesStorage workspaces;
        private readonly IMapper mapper;
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspacesCache workspacesCache;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator mediator;
        private readonly ILogger<WorkspacesPublicApiController> logger;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ISystemLog systemLog;
        private readonly IInScopeExecutor<IWebInterviewInvoker> webInterviewNotification;

        public WorkspacesPublicApiController(IWorkspacesStorage workspaces,
            IMapper mapper,
            IWorkspacesService workspacesService,
            IWorkspacesCache workspacesCache,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<WorkspacesPublicApiController> logger,
            IAuthorizedUser authorizedUser, ISystemLog systemLog, 
            IInScopeExecutor<IWebInterviewInvoker> webInterviewNotification)
        {
            this.workspaces = workspaces;
            this.mapper = mapper;
            this.workspacesService = workspacesService;
            this.workspacesCache = workspacesCache;
            this.unitOfWork = unitOfWork;
            this.mediator = mediator;
            this.logger = logger;
            this.authorizedUser = authorizedUser;
            this.systemLog = systemLog;
            this.webInterviewNotification = webInterviewNotification;
        }

        /// <summary>
        /// List existing workspaces
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(WorkspaceApiView))]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer)]
        public WorkspacesApiView Index([FromQuery] WorkspacesListFilter filter)
        {
            var result = workspaces.FilterWorkspaces(new WorkspacesFilter()
            {
                UserId = filter.UserId,
                IncludeDisabled = filter.IncludeDisabled,
                Offset = filter.Start,
                Limit = filter.Length,
                Query = filter.Search?.Value,
                SortOrder = filter.GetSortOrder(),
            });

            return new WorkspacesApiView
            (
                filter.Start,
                filter.Length,
                result.TotalCount,
                result.Workspaces.Select(w => new WorkspaceApiView()
                {
                    Name = w.Name,
                    DisplayName = w.DisplayName,
                    DisabledAtUtc = w.DisabledAtUtc,
                    CreatedAtUtc = w.CreatedAtUtc,
                })
            );
        }


        /// <summary>
        /// Get single workspace details
        /// </summary>
        [Route("{name}")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Workspace not found")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(WorkspaceApiView))]
        [Produces(MediaTypeNames.Application.Json)]
        [HttpGet]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult<WorkspaceApiView> Details(string name)
        {
            var workspace = this.workspaces.GetById(name);
            if (workspace == null || workspace.RemovedAtUtc != null || !this.authorizedUser.Workspaces.Contains(name))
            {
                return NotFound();
            }

            return mapper.Map<WorkspaceApiView>(workspace);
        }

        /// <summary>
        /// Creates new workspace. Accessible only to administrator 
        /// </summary>
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, "Workspace created", typeof(WorkspaceApiView))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed", Type = typeof(ValidationProblemDetails))]
        [HttpPost]
        [AuthorizeByRole(UserRoles.Administrator)]
        [ObservingNotAllowed]
        public async Task<ActionResult> Create([FromBody] WorkspaceCreateApiView request)
        {
            if (ModelState.IsValid)
            {
                var workspace = new Workspace(request.Name, request.DisplayName, DateTime.UtcNow);

                this.workspaces.Store(workspace);
                await this.workspacesService.Generate(workspace.Name,
                    DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());
                this.workspacesCache.InvalidateCache();
                
                this.systemLog.WorkspaceCreated(workspace.Name, workspace.DisplayName);

                return CreatedAtAction("Details", routeValues: new { name = workspace.Name },
                    value: this.mapper.Map<WorkspaceApiView>(workspace));
            }

            unitOfWork.DiscardChanges();
            return ValidationProblem();
        }

        /// <summary>
        /// Updates workspace 
        /// </summary>
        /// <response code="404">Workspace not found</response>
        /// <response code="403">User is not authorized to make changes to workspace</response>
        [HttpPatch]
        [Route("{name}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspace updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult Update([FromRoute] string name, [FromBody] WorkspaceUpdateApiView request)
        {
            if (ModelState.IsValid)
            {
                var existing = this.workspaces.GetById(name);

                if (existing == null || existing.RemovedAtUtc != null)
                {
                    return NotFound();
                }

                if (!this.authorizedUser.HasAccessToWorkspace(name))
                {
                    return Forbid();
                }
                var oldName = existing.DisplayName;

                existing.DisplayName = request.DisplayName!;
                this.workspacesCache.InvalidateCache();
                this.systemLog.WorkspaceUpdated(name, oldName, existing.DisplayName);
                return NoContent();
            }

            unitOfWork.DiscardChanges();
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
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspace disabled")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Workspace not found")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator)]
        public ActionResult Disable(string name)
        {
            var workspace = this.workspaces.GetById(name);
            if (workspace == null || workspace.RemovedAtUtc != null)
            {
                return NotFound();
            }

            if (workspace.DisabledAtUtc != null)
            {
                ModelState.AddModelError(nameof(workspace.DisabledAtUtc), $"Workspace {name} is already disabled");
            }

            if (name == Workspace.Default.Name)
            {
                ModelState.AddModelError(nameof(name), $"Workspace {name} cannot be disabled");
            }

            if (ModelState.IsValid)
            {
                workspace.Disable();
                this.logger.LogInformation("Workspace {name} was disabled by {user}", name, this.authorizedUser.UserName);
                this.workspacesCache.InvalidateCache();
                this.systemLog.WorkspaceDisabled(name);
                this.webInterviewNotification.Execute(_ => _.ShutDownAllWebInterviews());
                return NoContent();
            }

            unitOfWork.DiscardChanges();
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
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspace enabled")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Workspace not found")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator)]
        public ActionResult Enable(string name)
        {
            var workspace = this.workspaces.GetById(name);
            if (workspace == null || workspace.RemovedAtUtc != null)
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
                this.systemLog.WorkspaceEnabled(name);
                return NoContent();
            }

            unitOfWork.DiscardChanges();
            return ValidationProblem();
        }

        /// <summary>
        /// Assigns workspaces to user.
        /// </summary>
        /// <response code="204">Workspaces list updated</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        [Route("assign")]
        [Consumes(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Workspaces list updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.ApiUser)]
        public async Task<ActionResult> AssignWorkspaces([FromBody] AssignWorkspacesToUserModel model, CancellationToken cancellationToken)
        {
            if(model == null)
                return BadRequest();

            await this.mediator.Send(new AssignWorkspacesToUserModelRequest(ModelState, model), cancellationToken);

            if (ModelState.IsValid)
            {
                return NoContent();
            }

            unitOfWork.DiscardChanges();
            return ValidationProblem();
        }

        /// <summary>
        /// Request server for information about workspace status and it's ability to delete
        /// </summary>
        /// <param name="name">Workspace name</param>
        /// <returns>Workspace status information</returns>
        [HttpGet]
        [Route("status/{name}")]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.Administrator)]
        public async Task<WorkspaceStatusInformation> Status(string name)
        {
            var response = await this.mediator.Send(new GetWorkspaceStatusInformation(name));
            return response;
        }
        
        /// <summary>
        /// Delete workspace
        /// </summary>
        /// <param name="name">Workspace name</param>
        [HttpDelete]
        [Route("{name}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator)]
        public async Task<DeleteWorkspaceResponse> Delete(string name)
        {
            var response = await this.mediator.Send(new DeleteWorkspaceRequest(name));
            return response;
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/assignments")]
    public class AssignmentsHistoryController: ControllerBase
    {
        private readonly IAssignmentsService assignmentsService;
        private readonly IAuthorizedUser user;
        private readonly IUserViewFactory userViewFactory;
        private readonly IAssignmentViewFactory viewFactory;

        public AssignmentsHistoryController(
            IAssignmentsService assignmentsService,
            IAuthorizedUser user,
            IUserViewFactory userViewFactory, 
            IAssignmentViewFactory viewFactory)
        {
            this.assignmentsService = assignmentsService;
            this.user = user;
            this.userViewFactory = userViewFactory;
            this.viewFactory = viewFactory;
        }


        /// <summary>
        /// Gets history of the assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="start"></param>
        /// <param name="length">Limit of events to return</param>
        /// <response code="200">Assignment history</response>
        /// <response code="403">Assignment cannot accessed by logged in user</response>
        /// <response code="404">Assignment cannot be found</response>
        [HttpGet]
        [SwaggerOperation(Tags = new[] { "Assignments" })]
        [Route("{id:int}/history")]
        [Authorize(Roles = "ApiUser, Supervisor, Headquarter, Administrator")]
        public async Task<ActionResult<AssignmentHistory>> History(int id, [FromQuery]int start = 0, [FromQuery]int length = 30)
        {
            var assignment = this.assignmentsService.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound();
            }

            if (this.user.IsSupervisor && assignment.ResponsibleId != this.user.Id)
            {
                var responsible = this.userViewFactory.GetUser(assignment.ResponsibleId);
                if (!responsible.IsInterviewer())
                    return Forbid();
                if(responsible.Supervisor.Id != this.user.Id)
                    return Forbid();
            }

            AssignmentHistory result = await this.viewFactory.LoadHistoryAsync(assignment.PublicKey, start, length);
            return result;
        }
    }
}

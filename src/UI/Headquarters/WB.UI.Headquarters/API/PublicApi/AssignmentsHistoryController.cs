using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Main.Core.Entities.SubEntities;
using Swashbuckle.Swagger.Annotations;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.API.PublicApi
{
    [RoutePrefix("api/v1/assignments")]
    [CamelCase]
    public class AssignmentsHistoryController: BaseApiServiceController
    {
        private readonly IAssignmentsService assignmentsService;
        private readonly IAuthorizedUser user;
        private readonly IUserViewFactory userViewFactory;
        private readonly IAssignmentViewFactory viewFactory;

        public AssignmentsHistoryController(
            IAssignmentsService assignmentsService,
            IAuthorizedUser user,
            ILogger logger, 
            IUserViewFactory userViewFactory, 
            IAssignmentViewFactory viewFactory) : base(logger)
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
        [ResponseType(typeof(AssignmentHistory))]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Supervisor, UserRoles.Headquarter, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        public async Task<HttpResponseMessage> History(int id, [FromUri]int start = 0, [FromUri]int length = 30)
        {
            var assignment = this.assignmentsService.GetAssignment(id);
            if (assignment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (this.user.IsSupervisor && assignment.ResponsibleId != this.user.Id)
            {
                var responsible = this.userViewFactory.GetUser(assignment.ResponsibleId);
                if (!responsible.IsInterviewer())
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                if(responsible.Supervisor.Id != this.user.Id)
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            AssignmentHistory result = await this.viewFactory.LoadHistoryAsync(assignment.PublicKey, start, length);
            return Request.CreateResponse(result);
        }
    }
}

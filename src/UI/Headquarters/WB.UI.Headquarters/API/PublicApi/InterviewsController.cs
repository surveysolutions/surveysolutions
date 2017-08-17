using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    [RoutePrefix("api/v1/interviews")]
    [ApiBasicAuth(new [] {UserRoles.ApiUser, UserRoles.Administrator }, TreatPasswordAsPlain = true)]
    public class InterviewsController : BaseApiServiceController
    {
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IUserViewFactory userViewFactory;
        private readonly IReadSideKeyValueStorage<InterviewReferences> interviewReferences;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainStorageAccessor<Assignment> assignmentStorageAccessor;

        public InterviewsController(ILogger logger,
            IAllInterviewsFactory allInterviewsViewFactory,
            IInterviewDetailsViewFactory interviewDetailsViewFactory, 
            IInterviewHistoryFactory interviewHistoryViewFactory,
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferences, 
            IInterviewUniqueKeyGenerator keyGenerator,
            IPlainStorageAccessor<Assignment> assignmentStorageAccessor)
            : base(logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.interviewReferences = interviewReferences;
            this.keyGenerator = keyGenerator;
            this.assignmentStorageAccessor = assignmentStorageAccessor;
        }

        [HttpGet]
        [Route("")] //?{templateId}&{templateVersion}&{status}&{interviewId}&{limit=10}&{offset=1}
        public InterviewApiView InterviewsFiltered(Guid? templateId = null, long? templateVersion = null,
            InterviewStatus? status = null, Guid? interviewId = null, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                QuestionnaireId = templateId,
                QuestionnaireVersion = templateVersion,
                Statuses = status.HasValue ? new[] { status.Value } : null,
                InterviewId = interviewId
            };

            var interviews = this.allInterviewsViewFactory.Load(input);

            return new InterviewApiView(interviews);
        }

        [HttpGet]
        [Route("{id:guid}/details")]
        public InterviewApiDetails InterviewDetails(Guid id)
        {
            var interview = this.interviewDetailsViewFactory.GetInterviewDetails(interviewId: id, questionsTypes: InterviewDetailsFilter.All);
            if (interview == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var interviewDetails = new InterviewApiDetails(interview.InterviewDetails);

            return interviewDetails;
        }

        [HttpGet]
        [Route("{id:guid}/history")]
        public InterviewHistoryView InterviewHistory(Guid id)
        {
            var interview = this.interviewHistoryViewFactory.Load(id);

            if (interview == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return interview;
        }

#region POST
        /// <summary>
        /// Assigns interview to interviewer
        /// </summary>
        /// <response code="200">Interview was reassigned</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target responsible was not found or it is not an interviewer</response>
        [HttpPost]
        [Route("assign")]
        public HttpResponseMessage PostAssign(AssignChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var userInfo = this.userViewFactory.GetUser(request.ResponsibleId.HasValue ? new UserViewInputModel(request.ResponsibleId.Value): new UserViewInputModel(request.ResponsibleName, null));

            if(userInfo == null)
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "User was not found.");

            if(!userInfo.Roles.Contains(UserRoles.Interviewer))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "User is not an interviewer.");
            
            return this.TryExecuteCommand(new AssignInterviewerCommand(request.Id, this.authorizedUser.Id, userInfo.PublicKey, DateTime.UtcNow));
        }

        [HttpPost]
        [Route("approve")]
        public HttpResponseMessage Approve(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);
            
            return this.TryExecuteCommand(new ApproveInterviewCommand(request.Id, this.authorizedUser.Id, request.Comment, DateTime.UtcNow));
        }

        [HttpPost]
        [Route("reject")]
        public HttpResponseMessage Reject(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);
            
            return this.TryExecuteCommand(new RejectInterviewCommand(request.Id, this.authorizedUser.Id, request.Comment, DateTime.UtcNow));
        }

        [HttpPost]
        [Route("hqapprove")]
        public HttpResponseMessage HQApprove(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);
            
            return this.TryExecuteCommand(new HqApproveInterviewCommand(request.Id, this.authorizedUser.Id, request.Comment));
        }


        [HttpPost]
        [Route("hqreject")]
        public HttpResponseMessage HQReject(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);
            
            return this.TryExecuteCommand(new HqRejectInterviewCommand(request.Id, this.authorizedUser.Id, request.Comment));
        }


        [HttpPost]
        [Route("hqunapprove")]
        public HttpResponseMessage HQUnapprove(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);
            
            return this.TryExecuteCommand(new UnapproveByHeadquartersCommand(request.Id, this.authorizedUser.Id, request.Comment));
        }

        [HttpPost]
        [Route("delete")]
        public HttpResponseMessage Delete(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);
            
            return this.TryExecuteCommand(new DeleteInterviewCommand(request.Id, this.authorizedUser.Id));
        }

        [HttpPost]
        [Route("assignsupervisor")]
        public HttpResponseMessage PostAssignSupervisor(AssignChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var userInfo = this.userViewFactory.GetUser(request.ResponsibleId.HasValue ? new UserViewInputModel(request.ResponsibleId.Value) : new UserViewInputModel(request.ResponsibleName, null));

            if (userInfo == null)
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "User was not found.");

            if (!userInfo.Roles.Contains(UserRoles.Supervisor))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "User is not a supervisor.");
            
            return this.TryExecuteCommand(new AssignSupervisorCommand(request.Id, this.authorizedUser.Id, userInfo.PublicKey));
        }

        #endregion

        /// <summary>
        /// Create interview for assignment
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Information about created interview</returns>
        [HttpPost]
        [Route("create")]
        public CreateInterviewResult CreateInterview(CreateInterviewRequest request)
        {
            var assignment = this.assignmentStorageAccessor.GetById(request.AssignmentId)
                ?? throw new InvalidOperationException($"Cannot find assignment with id: {request.AssignmentId}");

            if (assignment.InterviewsNeeded.HasValue && assignment.InterviewsNeeded <= 0)
            {
                throw new InvalidOperationException($"Cannot create more interviews from this assignmentId: {request.AssignmentId}");
            }

            var interviewer = this.userViewFactory.GetUser(new UserViewInputModel(assignment.ResponsibleId));

            if (!interviewer.IsInterviewer())
                throw new InvalidOperationException($"Assignment {assignment.Id} has responsible that is not an interviewer. Interview cannot be created");

            var interviewId = Guid.NewGuid();

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey,
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                DateTime.UtcNow,
                interviewer.Supervisor.Id,
                interviewer.PublicKey,
                this.keyGenerator.Get(),
                assignment.Id);

            this.commandService.Execute(createInterviewCommand);

            return new CreateInterviewResult
            {
                AssignmentId = assignment.Id,
                InterviewId = createInterviewCommand.InterviewerId.Value,
                InterviewKey = createInterviewCommand.InterviewKey.ToString(),
                AssignedTo = interviewer.UserName,
                AssignedToId = interviewer.PublicKey
            };
        }

        private HttpResponseMessage TryExecuteCommand(ICommand command)
        {
            try
            {
                this.commandService.Execute(command);
            }
            catch (InterviewException interviewExc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, interviewExc.Message);
            }
            catch (Exception exc)
            {
                this.Logger.Error("Error occurred on Api request", exc);
                throw new HttpResponseException(HttpStatusCode.ServiceUnavailable);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private void ThrowIfInterviewDoesnotExist(Guid id)
        {
            var interviewRefs = this.interviewReferences.GetById(id);
            if (interviewRefs == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
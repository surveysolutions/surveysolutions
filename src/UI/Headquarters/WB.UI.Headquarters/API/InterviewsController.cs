using Main.Core.Entities.SubEntities;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
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

        private readonly ICommandService commandService;
        private readonly IGlobalInfoProvider globalInfoProvider;

        public InterviewsController(ILogger logger,
            IAllInterviewsFactory allInterviewsViewFactory,
            IInterviewDetailsViewFactory interviewDetailsViewFactory, 
            IInterviewHistoryFactory interviewHistoryViewFactory,
            ICommandService commandService,
            IGlobalInfoProvider globalInfoProvider,
            IUserViewFactory userViewFactory,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferences)
            : base(logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.commandService = commandService;
            this.globalInfoProvider = globalInfoProvider;
            this.userViewFactory = userViewFactory;
            this.interviewReferences = interviewReferences;
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
                Status = status,
                InterviewId = interviewId
            };

            var interviews = this.allInterviewsViewFactory.Load(input);

            return new InterviewApiView(interviews);
        }

        [HttpGet]
        [Route("{id:guid}/details")]
        public InterviewApiDetails InterviewDetails(Guid id)
        {
            InterviewDetailsFilter filter = new InterviewDetailsFilter();
            var interview = this.interviewDetailsViewFactory.GetInterviewDetails(interviewId: id, filter:filter);
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
        [HttpPost]
        [Route("assign")]
        public HttpResponseMessage PostAssign(AssignChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var userInfo = this.userViewFactory.Load(request.ResponsibleId.HasValue ? new UserViewInputModel(request.ResponsibleId.Value): new UserViewInputModel(request.ResponsibleName, null));

            if(userInfo == null)
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "User was not found.");

            if(!userInfo.Roles.Contains(UserRoles.Operator))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "User is not an interviewer.");

            var executor = this.globalInfoProvider.GetCurrentUser();
            return TryExecuteCommand(new AssignInterviewerCommand(request.Id, executor.Id, userInfo.PublicKey, DateTime.UtcNow));
        }

        [HttpPost]
        [Route("approve")]
        public HttpResponseMessage Approve(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var executor = this.globalInfoProvider.GetCurrentUser();
            return TryExecuteCommand(new ApproveInterviewCommand(request.Id, executor.Id, request.Comment, DateTime.UtcNow));
        }

        [HttpPost]
        [Route("reject")]
        public HttpResponseMessage Reject(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var executor = this.globalInfoProvider.GetCurrentUser();
            return TryExecuteCommand(new RejectInterviewCommand(request.Id, executor.Id, request.Comment, DateTime.UtcNow));
        }

        [HttpPost]
        [Route("hqapprove")]
        public HttpResponseMessage HQApprove(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var executor = this.globalInfoProvider.GetCurrentUser();
            return TryExecuteCommand(new HqApproveInterviewCommand(request.Id, executor.Id, request.Comment));
        }


        [HttpPost]
        [Route("hqreject")]
        public HttpResponseMessage HQReject(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var executor = this.globalInfoProvider.GetCurrentUser();
            return TryExecuteCommand(new HqRejectInterviewCommand(request.Id, executor.Id, request.Comment));
        }


        [HttpPost]
        [Route("hqunapprove")]
        public HttpResponseMessage HQUnapprove(StatusChangeApiModel request)
        {
            this.ThrowIfInterviewDoesnotExist(request.Id);

            var executor = this.globalInfoProvider.GetCurrentUser();
            return TryExecuteCommand(new UnapproveByHeadquartersCommand(request.Id, executor.Id, request.Comment));
        }
        #endregion

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

            return Request.CreateResponse(HttpStatusCode.OK);
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
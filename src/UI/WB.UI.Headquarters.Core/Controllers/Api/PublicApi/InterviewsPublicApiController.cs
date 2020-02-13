﻿using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/interviews")]
    [Authorize(Roles = "ApiUser, Administrator")]
    [PublicApiJson]
    public class InterviewsPublicApiController : ControllerBase
    {
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IUserViewFactory userViewFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferences;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ILogger<InterviewsPublicApiController> logger;
        private readonly IStatefullInterviewSearcher statefullInterviewSearcher;
        private readonly IInterviewDiagnosticsFactory diagnosticsFactory;

        public InterviewsPublicApiController(
            IAllInterviewsFactory allInterviewsViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory,
            IUserViewFactory userViewFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferences,
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger<InterviewsPublicApiController> logger,
            IStatefullInterviewSearcher statefullInterviewSearcher,
            IInterviewDiagnosticsFactory diagnosticsFactory)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.userViewFactory = userViewFactory;
            this.interviewReferences = interviewReferences;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.logger = logger;
            this.statefullInterviewSearcher = statefullInterviewSearcher;
            this.diagnosticsFactory = diagnosticsFactory;
        }


        /// <summary>
        /// Gets list of interviews existing in the system
        /// </summary>
        /// <param name="questionnaireId">Questionnaire id if filtering by this field is required</param>
        /// <param name="questionnaireVersion">Questionnaire id if filtering by this field is required</param>
        /// <param name="status">Filtering by interview status</param>
        /// <param name="interviewId">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="page">Page number (starting from 1)</param>
        /// <returns></returns>
        [HttpGet]
        public InterviewApiView InterviewsFiltered(Guid? questionnaireId = null, long? questionnaireVersion = null,
            InterviewStatus? status = null, Guid? interviewId = null, int pageSize = 10, int page = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = page.CheckAndRestrictOffset(),
                PageSize = pageSize.CheckAndRestrictLimit(),
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Statuses = status.HasValue ? new[] { status.Value } : null,
                InterviewId = interviewId
            };

            var interviews = this.allInterviewsViewFactory.Load(input);

            return new InterviewApiView(interviews);
        }

        /// <summary>
        /// Gets all the answers for given interview
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        [HttpGet]
        [Route("{id:guid}")]
        public ActionResult<InterviewApiDetails> Get(Guid id)
        {
            var interview = this.statefulInterviewRepository.Get(id.ToString());
            if (interview == null)
                return NotFound();

            return new InterviewApiDetails(interview);
        }

        /// <summary>
        /// Get statistics by interview
        /// </summary>
        /// <param name="id">Interview id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}/stats")]
        public ActionResult<InterviewApiStatistics> Stats(Guid id)
        {
            var interview = this.statefulInterviewRepository.Get(id.ToString());
            if (interview == null)
            {
                return NotFound();
            }

            var statistics = this.statefullInterviewSearcher.GetStatistics(interview);
            var diagnosticsInfo = diagnosticsFactory.GetById(id);
            var interviewSummary = this.allInterviewsViewFactory.Load(new AllInterviewsInputModel
            {
                InterviewId = id
            });

            return new InterviewApiStatistics
            {
                Answered = statistics[FilterOption.Answered],
                NotAnswered = statistics[FilterOption.NotAnswered],
                Valid = statistics[FilterOption.Valid],
                Invalid = statistics[FilterOption.Invalid],
                Flagged = statistics[FilterOption.Flagged],
                NotFlagged = statistics[FilterOption.NotFlagged],
                WithComments = statistics[FilterOption.WithComments],
                ForInterviewer = statistics[FilterOption.ForInterviewer],
                ForSupervisor = statistics[FilterOption.ForSupervisor],

                InterviewId = diagnosticsInfo.InterviewId,
                InterviewKey = diagnosticsInfo.InterviewKey,
                AssignmentId = interview.GetAssignmentId(),
                Status = diagnosticsInfo.Status.ToString(),
                ResponsibleId = diagnosticsInfo.ResponsibleId,
                ResponsibleName = diagnosticsInfo.ResponsibleName,
                NumberOfInterviewers = diagnosticsInfo.NumberOfInterviewers,
                NumberRejectionsBySupervisor = diagnosticsInfo.NumberRejectionsBySupervisor,
                NumberRejectionsByHq = diagnosticsInfo.NumberRejectionsByHq,
                InterviewDuration = diagnosticsInfo.InterviewDuration != null ? new TimeSpan(diagnosticsInfo.InterviewDuration.Value) : (TimeSpan?)null,
                UpdatedAtUtc = interviewSummary.Items.First().LastEntryDateUtc
            };
        }

        [HttpGet]
        [Route("{id:guid}/history")]
        public ActionResult<InterviewHistoryView> InterviewHistory(Guid id)
        {
            var interview = this.interviewHistoryViewFactory.Load(id);

            if (interview == null)
            {
                return NotFound();
            }

            return interview;
        }

        /// <summary>
        /// Leave a comment on a question using questionnaire variable name and roster vector
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="variable">Variable name. This is the variable name for a question in Designer or in an export file.</param>
        /// <param name="rosterVector">Roster row. In simple rosters, the row code. In nested rosters, an array of row codes: first, the row code of the parent(s); followed by the row code of the target child roster (e.g., a question in a second-level roster needs 2 row codes, a question in a first-level roster only 1). For variables not in rosters, this parameter may be left blank.</param>
        /// <param name="comment">Comment. Comment to be posted to the chosen question </param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:guid}/comment-by-variable/{variable}")]
        [Authorize(Roles = "Supervisor, Interviewer, ApiUser, Administrator")]
        public ActionResult CommentByVariable(Guid id, string variable, RosterVector rosterVector, string comment)
        {
            var questionnaireIdentity = this.GetQuestionnaireIdForInterview(id);

            var questionnaire = questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var question = questionnaire.GetQuestionByVariable(variable);

            if (question == null)
                return StatusCode(StatusCodes.Status406NotAcceptable,
                    "Question was not found.");

            return this.CommentAnswer(id, Identity.Create(question.PublicKey, rosterVector), comment);
        }

        /// <summary>
        /// Leave a comment on a question
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="questionId">Question Id. Identifier of the question constructed as follows. First, take the question GUID from the JSON version of the questionnaire. Then, remove all dashes. If the question is not in a roster, use this as the question Id. If the question is in a roster, append its address to the question Id using the following pattern : [questionId]_#-#-#, where [questionId] is the question GUID without dashes, # represents the row code of each roster from the top level of the questionnaire to the current question, and only the needed number of row codes is used (e.g., a question in a second-level roster needs 2 row codes, a question in a first-level roster only 1).</param>
        /// <param name="comment">Comment. Comment to be posted to the chosen question </param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:guid}/comment/{questionId}")]
        [Authorize(Roles = "Supervisor, Interviewer, ApiUser, Administrator")]
        public ActionResult CommentByIdentity(Guid id, string questionId, string comment)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();

            if(!Identity.TryParse(questionId, out var questionIdentity))
                return StatusCode(StatusCodes.Status400BadRequest, $@"bad {nameof(questionId)} format");

            return CommentAnswer(id, questionIdentity, comment);
        }

        private ActionResult CommentAnswer(Guid id, Identity questionIdentity, string comment)
        {
            var command = new CommentAnswerCommand(id, this.authorizedUser.Id, questionIdentity.Id,
                questionIdentity.RosterVector, comment);

            return this.TryExecuteCommand(command);
        }

        #region POST

        /// <summary>
        /// Assigns interview to interviewer
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="request">Responsible id or responsible name</param>
        /// <response code="200">Interview was reassigned</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target responsible was not found or it is not an interviewer</response>
        [HttpPatch]
        [Route("{id:guid}/assign")]
        public ActionResult Assign(Guid id, AssignChangeApiModel request)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();

            var userInfo = this.userViewFactory.GetUser(request.ResponsibleId.HasValue ? new UserViewInputModel(request.ResponsibleId.Value): new UserViewInputModel(request.ResponsibleName, null));

            if(userInfo == null)
                return StatusCode(StatusCodes.Status406NotAcceptable, "User was not found.");

            if(!userInfo.Roles.Contains(UserRoles.Interviewer))
                return StatusCode(StatusCodes.Status406NotAcceptable, "User is not an interviewer.");
            
            return this.TryExecuteCommand(new AssignResponsibleCommand(id, this.authorizedUser.Id, userInfo.PublicKey, userInfo.Supervisor.Id));
        }

        /// <summary>
        /// Approves interview 
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="comment">Approval comment</param>
        /// <response code="200">Interview was approved</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be approved</response>
        [HttpPatch]
        [Route("{id:guid}/approve")]
        public ActionResult Approve(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new ApproveInterviewCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Rejects interview as supervisor
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="comment">Rejection comment</param>
        /// <response code="200">Interview was rejected</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be rejected</response>
        [HttpPatch]
        [Route("{id:guid}/reject")]
        public ActionResult Reject(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new RejectInterviewCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Approves interview as headquarters
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="comment">Rejection comment</param>
        /// <response code="200">Interview was approved</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be approved</response>
        [HttpPatch]
        [Route("{id:guid}/hqapprove")]
        public ActionResult HQApprove(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new HqApproveInterviewCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Rejects interview as headquarters
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="comment">Rejection comment</param>
        /// <response code="200">Interview was rejected</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be rejected</response>
        [HttpPatch]
        [Route("{id:guid}/hqreject")]
        public ActionResult HQReject(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new HqRejectInterviewCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Rejects interview from Approved by headquarters status
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="comment">Rejection comment</param>
        /// <response code="200">Interview was rejected</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be rejected</response>
        [HttpPatch]
        [Route("{id:guid}/hqunapprove")]
        public ActionResult HQUnapprove(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new UnapproveByHeadquartersCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Assigns supervisor
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <param name="request"></param>
        /// <response code="200">Interview was deleted</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Interview cannot be reassigned. Check response for error description</response>
        [HttpPatch]
        [Route("{id:guid}/assignsupervisor")]
        public ActionResult PostAssignSupervisor(Guid id, AssignChangeApiModel request)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();

            var userInfo = this.userViewFactory.GetUser(request.ResponsibleId.HasValue ? new UserViewInputModel(request.ResponsibleId.Value) : new UserViewInputModel(request.ResponsibleName, null));

            if (userInfo == null)
                return StatusCode(StatusCodes.Status406NotAcceptable, "User was not found.");

            if (!userInfo.Roles.Contains(UserRoles.Supervisor))
                return StatusCode(StatusCodes.Status406NotAcceptable, "User is not a supervisor.");
            
            return this.TryExecuteCommand(new AssignSupervisorCommand(id, this.authorizedUser.Id, userInfo.PublicKey));
        }

        /// <summary>
        /// Deletes interview 
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests.</param>
        /// <response code="200">Interview was deleted</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be deleted</response>
        [HttpDelete]
        [Route("{id:guid}")]
        public ActionResult Delete(Guid id)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new DeleteInterviewCommand(id, this.authorizedUser.Id));
        }
        
        #endregion

        private ActionResult TryExecuteCommand(InterviewCommand command)
        {
            try
            {
                this.commandService.Execute(command);
            }
            catch (InterviewException interviewExc)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, interviewExc.Message);
            }
            catch (Exception exc)
            {
                this.logger.LogError($"Error during execution of {command.GetType()}", new Exception(null, exc)
                {
                    Data =
                    {
                        {"CommandInterviewId", command.InterviewId},
                        {"CommandUserId", command.UserId}
                    }
                });

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        private QuestionnaireIdentity GetQuestionnaireIdForInterview(Guid id)
        {
            var interviewRefs = this.interviewReferences.GetQuestionnaireIdentity(id);
            return interviewRefs;
        }
    }
}

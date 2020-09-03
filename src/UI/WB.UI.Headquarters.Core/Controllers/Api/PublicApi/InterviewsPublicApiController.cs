﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
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
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.WebInterview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/interviews")]
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
        private readonly IPdfInterviewGenerator pdfInterviewGenerator;

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
            IInterviewDiagnosticsFactory diagnosticsFactory,
            IPdfInterviewGenerator pdfInterviewGenerator)
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
            this.pdfInterviewGenerator = pdfInterviewGenerator;
        }


        /// <summary>
        /// Gets list of interviews existing in the system
        /// </summary>
        /// <param name="questionnaireId">Questionnaire id if filtering by this field is required</param>
        /// <param name="questionnaireVersion">Questionnaire id if filtering by this field is required</param>
        /// <param name="status">Filtering by interview status</param>
        /// <param name="interviewId">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="page">Page number (starting from 1)</param>
        /// <returns></returns>
        [HttpGet]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        [Obsolete("Use /graphql endpoint instead")]
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
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        [HttpGet]
        [Route("{id:guid}")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
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
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
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
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
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
        /// Get interview transcript in pdf file
        /// </summary>
        /// <param name="id">Interview id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}/pdf")]
        [AllowAnonymous]
        public IActionResult Pdf(Guid id)
        {
            var interview = statefulInterviewRepository.Get(id.ToString());
            if (interview == null)
                return NotFound(id);

            if (!this.authorizedUser.IsAuthenticated)
            {
                var hasAccess = Request.HasAccessToWebInterviewAfterComplete(interview);
                if (!hasAccess)
                    return Forbid();
            }

            if (this.authorizedUser.IsInterviewer && interview.CurrentResponsibleId != this.authorizedUser.Id)
                return Forbid();

            if (this.authorizedUser.IsSupervisor && interview.SupervisorId != this.authorizedUser.Id)
                return Forbid();

            var content = pdfInterviewGenerator.Generate(id);
            if (content == null)
                return NotFound(id);

            return this.File(content, 
                "application/pdf", 
                fileDownloadName: interview.GetInterviewKey() + ".pdf");
        }

        /// <summary>
        /// Leave a comment on a question using questionnaire variable name and roster vector
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="variable">Variable name. This is the variable name for a question in Designer or in an export file</param>
        /// <param name="rosterVector">Roster row. In simple rosters, the row code. In nested rosters, an array of row codes: first, the row code of the parent(s); followed by the row code of the target child roster (e.g., a question in a second-level roster needs 2 row codes, a question in a first-level roster only 1). For variables not in rosters, this parameter may be left blank</param>
        /// <param name="comment">Comment. Comment to be posted to the chosen question </param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:guid}/comment-by-variable/{variable}")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Interviewer, UserRoles.Supervisor)]
        public ActionResult CommentByVariable(Guid id, [Required]string variable, int[] rosterVector, [Required]string comment)
        {
            var questionnaireIdentity = this.GetQuestionnaireIdForInterview(id);

            var questionnaire = questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var questionId = questionnaire.GetQuestionIdByVariable(variable);

            if (questionId == null)
                return StatusCode(StatusCodes.Status406NotAcceptable, "Question was not found.");

            return this.CommentAnswer(id, Identity.Create(questionId.Value, rosterVector), comment);
        }

        /// <summary>
        /// Leave a comment on a question
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="questionId">Question Id. Identifier of the question constructed as follows. First, take the question GUID from the JSON version of the questionnaire. Then, remove all dashes. If the question is not in a roster, use this as the question Id. If the question is in a roster, append its address to the question Id using the following pattern : [questionId]_#-#-#, where [questionId] is the question GUID without dashes, # represents the row code of each roster from the top level of the questionnaire to the current question, and only the needed number of row codes is used (e.g., a question in a second-level roster needs 2 row codes, a question in a first-level roster only 1)</param>
        /// <param name="comment">Comment. Comment to be posted to the chosen question </param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:guid}/comment/{questionId}")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Interviewer, UserRoles.Supervisor)]
        public ActionResult CommentByIdentity(Guid id, [Required]string questionId, [Required]string comment)
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
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="request">Responsible id or responsible name</param>
        /// <response code="200">Interview was reassigned</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Interview cannot be reassigned. Check response for error description</response>
        [HttpPatch]
        [Route("{id:guid}/assign")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult Assign(Guid id, [FromBody] AssignChangeApiModel request)
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
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="comment">Approval comment</param>
        /// <response code="200">Interview was approved</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be approved</response>
        [HttpPatch]
        [Route("{id:guid}/approve")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult Approve(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new ApproveInterviewCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Rejects interview as supervisor
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="comment">Rejection comment</param>
        /// <param name="responsibleId">New responsible id</param>
        /// <response code="200">Interview was rejected</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be rejected or selected responsible cannot be assigned</response>

        [HttpPatch]
        [Route("{id:guid}/reject")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult Reject(Guid id, string comment = null, Guid? responsibleId = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();

            if (!responsibleId.HasValue)
                return this.TryExecuteCommand(new RejectInterviewCommand(id, this.authorizedUser.Id, comment));
            
            var userInfo = this.userViewFactory.GetUser(responsibleId.Value);

            if(userInfo == null)
                return StatusCode(StatusCodes.Status406NotAcceptable, "User was not found.");

            if(!userInfo.Roles.Contains(UserRoles.Interviewer))
                return StatusCode(StatusCodes.Status406NotAcceptable, "User is not an interviewer.");

            return this.TryExecuteCommand(new RejectInterviewToInterviewerCommand(this.authorizedUser.Id, id, userInfo.PublicKey, comment));
        }

        /// <summary>
        /// Approves interview as headquarters
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="comment">Rejection comment</param>
        /// <response code="200">Interview was approved</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be approved</response>
        [HttpPatch]
        [Route("{id:guid}/hqapprove")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult HQApprove(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new HqApproveInterviewCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Rejects interview as headquarters
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="comment">Rejection comment</param>
        /// <param name="responsibleId">New responsible id</param>
        /// <response code="200">Interview was rejected</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be rejected or selected responsible cannot be assigned</response>

        [HttpPatch]
        [Route("{id:guid}/hqreject")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult HQReject(Guid id, string comment = null, Guid? responsibleId = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            if (!responsibleId.HasValue)
                return this.TryExecuteCommand(new HqRejectInterviewCommand(id, this.authorizedUser.Id, comment));
           
            var userInfo = this.userViewFactory.GetUser(responsibleId.Value);

            if(userInfo == null)
                return StatusCode(StatusCodes.Status406NotAcceptable, "User was not found.");

            if(userInfo.Roles.Contains(UserRoles.Interviewer))
                return this.TryExecuteCommand(new HqRejectInterviewToInterviewerCommand(id, this.authorizedUser.Id, userInfo.PublicKey, userInfo.Supervisor.Id, comment));

            if(userInfo.Roles.Contains(UserRoles.Supervisor))
                return this.TryExecuteCommand(new HqRejectInterviewToSupervisorCommand(id, this.authorizedUser.Id, userInfo.PublicKey, comment));
            
            return StatusCode(StatusCodes.Status406NotAcceptable, "Responsible is not an interviewer or supervisor.");
        }

        /// <summary>
        /// Rejects interview from Approved by headquarters status
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="comment">Rejection comment</param>
        /// <response code="200">Interview was rejected</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be rejected</response>
        [HttpPatch]
        [Route("{id:guid}/hqunapprove")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult HQUnapprove(Guid id, string comment = null)
        {
            var q = this.GetQuestionnaireIdForInterview(id);
            if (q == null) return NotFound();
            
            return this.TryExecuteCommand(new UnapproveByHeadquartersCommand(id, this.authorizedUser.Id, comment));
        }

        /// <summary>
        /// Assigns supervisor
        /// </summary>
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <param name="request"></param>
        /// <response code="200">Interview was assigned to supervisor</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Interview cannot be reassigned. Check response for error description</response>
        [HttpPatch]
        [Route("{id:guid}/assignsupervisor")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult PostAssignSupervisor(Guid id, [FromBody]  AssignChangeApiModel request)
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
        /// <param name="id">Interview Id. This corresponds to the interview__id variable in data export files or the interview Id obtained through other API requests</param>
        /// <response code="200">Interview was deleted</response>
        /// <response code="404">Interview was not found</response>
        /// <response code="406">Target interview was in status that was not ready to be deleted</response>
        [HttpDelete]
        [Route("{id:guid}")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
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

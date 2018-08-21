using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Interviewer")]
    public class InterviewerHqController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory usersRepository;
        private readonly IPlainStorageAccessor<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;

        public InterviewerHqController(
            ICommandService commandService,
            ILogger logger,
            IAuthorizedUser authorizedUser,
            IWebInterviewConfigProvider configProvider,
            IUserViewFactory usersRepository,
            IPlainStorageAccessor<InterviewSummary> interviewSummaryReader,
            IPlainStorageAccessor<Assignment> assignments,
            IInterviewUniqueKeyGenerator keyGenerator) : base(commandService, logger)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.usersRepository = usersRepository;
            this.interviewSummaryReader = interviewSummaryReader;
            this.assignments = assignments;
            this.keyGenerator = keyGenerator;
        }

        [ActivePage(MenuItem.CreateNew)]
        public ActionResult CreateNew()
        {
            return View("Index", NewModel(MenuItem.CreateNew));
        }

        [ActivePage(MenuItem.Started)]
        public ActionResult Started()
        {
            return View("Interviews", NewModel(MenuItem.Started, InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted));
        }

        [ActivePage(MenuItem.Rejected)]
        public ActionResult Rejected()
        {
            return View("Interviews", NewModel(MenuItem.Rejected, InterviewStatus.RejectedBySupervisor));
        }

        [ActivePage(MenuItem.Completed)]
        public ActionResult Completed()
        {
            return View("Interviews", NewModel(MenuItem.Completed, InterviewStatus.Completed));
        }

        private InterviewerHqModel NewModel(MenuItem title, params InterviewStatus[] statuses)
        {
            ViewBag.ActivePage = title;
            return new InterviewerHqModel
            {
                Title = title.ToUiString(),
                AllInterviews = Url.Content(@"~/api/InterviewApi/GetInterviews"),
                InterviewerHqEndpoint = Url.Content(@"~/InterviewerHq"),
                Statuses = statuses.Select(s => s.ToString()).ToArray(),
                Questionnaires = this.GetQuestionnaires(statuses)
            };
        }
        
        private string CreateInterview(Assignment assignment)
        {
            var interviewer = this.usersRepository.GetUser(new UserViewInputModel(assignment.ResponsibleId));
            if (!interviewer.IsInterviewer())
                throw new InvalidOperationException($"Assignment {assignment.Id} has responsible that is not an interviewer. Interview cannot be created");

            var interviewId = Guid.NewGuid();

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey,
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                assignment.ProtectedVariables,
                interviewer.Supervisor.Id,
                interviewer.PublicKey,
                this.keyGenerator.Get(),
                assignment.Id);

            this.commandService.Execute(createInterviewCommand);
            return interviewId.FormatGuid();
        }

        [HttpPost]
        public ActionResult StartNewInterview(int id)
        {
            var assignment = this.assignments.GetById(id);

            var interviewId = CreateInterview(assignment);
            TempData[WebInterviewController.LastCreatedInterviewIdKey] = interviewId;

            return Content(Url.Content(GenerateUrl(@"Cover", interviewId)));
        }

        [HttpGet]
        public ActionResult OpenInterview(Guid id)
        {
            return RedirectToAction("Cover", "WebInterview", new { id = id.FormatGuid() });
        }

        [HttpDelete]
        public ActionResult DiscardInterview(Guid id)
        {
            var deleteInterview = new DeleteInterviewCommand(id, this.authorizedUser.Id);
            this.commandService.Execute(deleteInterview);
            return this.Content("ok");
        }

        [HttpPost]
        public ActionResult RestartInterview(Guid id, string comment)
        {
            var restartCommand = new RestartInterviewCommand(id, this.authorizedUser.Id, comment, DateTime.UtcNow);

            this.commandService.Execute(restartCommand);

            return Content(Url.Content(GenerateUrl(@"Cover", id.FormatGuid())));
        }
        
        private ComboboxOptionModel[] GetQuestionnaires(InterviewStatus[] interviewStatuses)
        {
            var queryResult = this.interviewSummaryReader.Query(_ =>
            {
                var filter = _.Where(summary => summary.ResponsibleId == this.authorizedUser.Id);

                if (interviewStatuses != null)
                {
                    filter = filter.Where(summary => interviewStatuses.Contains(summary.Status));
                }

                return filter
                    .OrderBy(s => s.QuestionnaireTitle).ThenBy(s => s.QuestionnaireVersion)
                    .Select(s => new
                    {
                        s.QuestionnaireTitle,
                        s.QuestionnaireId,
                        s.QuestionnaireVersion
                    })
                    .Distinct().ToList();
            });

            return queryResult.Select(s => new ComboboxOptionModel(
                new QuestionnaireIdentity(s.QuestionnaireId, s.QuestionnaireVersion).ToString(),
                $@"(ver. {s.QuestionnaireVersion.ToString()}) {s.QuestionnaireTitle}")).ToArray();
        }

        private string GenerateUrl(string action, string interviewId, string sectionId = null) =>
            $@"~/WebInterview/{interviewId}/{action}" + (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
    }
}

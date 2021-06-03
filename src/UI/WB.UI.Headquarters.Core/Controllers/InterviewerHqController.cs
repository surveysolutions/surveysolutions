using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Interviewer")]
    public class InterviewerHqController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory usersRepository;
        private readonly IPlainStorageAccessor<InterviewSummary> interviewSummaryReader;
        private readonly IAssignmentsService assignments;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;
        private readonly ICalendarEventService calendarEventService;

        
        public InterviewerHqController(
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            IUserViewFactory usersRepository,
            IPlainStorageAccessor<InterviewSummary> interviewSummaryReader,
            IAssignmentsService assignments,
            IInterviewUniqueKeyGenerator keyGenerator,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ICalendarEventService calendarEventService)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.usersRepository = usersRepository;
            this.interviewSummaryReader = interviewSummaryReader;
            this.assignments = assignments;
            this.keyGenerator = keyGenerator;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.calendarEventService = calendarEventService;
        }

        [ActivePage(MenuItem.CreateNew)]
        public IActionResult CreateNew()
        {
            return View("Index", NewModel(MenuItem.CreateNew));
        }

        [ActivePage(MenuItem.Started)]
        public IActionResult Started()
        {
            return View("Interviews", NewModel(MenuItem.Started, InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted));
        }

        [ActivePage(MenuItem.Rejected)]
        public IActionResult Rejected()
        {
            return View("Interviews", NewModel(MenuItem.Rejected, InterviewStatus.RejectedBySupervisor));
        }

        [ActivePage(MenuItem.Completed)]
        public IActionResult Completed()
        {
            return View("Interviews", NewModel(MenuItem.Completed, InterviewStatus.Completed));
        }

        private InterviewerHqModel NewModel(MenuItem title, params InterviewStatus[] statuses)
        {
            ViewBag.ActivePage = title;
            return new InterviewerHqModel
            {
                Title = title.ToUiString(),
                InterviewerHqEndpoint = Url.Content(@"~/InterviewerHq"),
                Statuses = statuses.Select(s => s.ToString().ToUpper()).ToArray(),
                Questionnaires = this.GetQuestionnaires(statuses)
            };
        }
        
        private string CreateInterview(Assignment assignment)
        {
            var interviewer = this.usersRepository.GetUser(new UserViewInputModel(assignment.ResponsibleId));
            if (!interviewer.IsInterviewer())
                throw new InvalidOperationException($"Assignment {assignment.Id} has responsible that is not an interviewer. Interview cannot be created");

            var interviewId = Guid.NewGuid();
            var interviewKey = this.keyGenerator.Get();

            var createInterviewCommand = new CreateInterview(
                interviewId,
                interviewer.PublicKey,
                assignment.QuestionnaireId,
                assignment.Answers.ToList(),
                assignment.ProtectedVariables,
                interviewer.Supervisor.Id,
                interviewer.PublicKey,
                interviewKey,
                assignment.Id,
                assignment.AudioRecording,
                InterviewMode.CAPI);

            this.commandService.Execute(createInterviewCommand);
            
            var calendarEvent = calendarEventService.GetActiveCalendarEventForAssignmentId(assignment.Id);
            if (calendarEvent != null)
            {
                var createCalendarEvent = new CreateCalendarEventCommand(Guid.NewGuid(), 
                    interviewer.PublicKey,
                    calendarEvent.Start.ToDateTimeUtc(),
                    calendarEvent.Start.Zone.Id,
                    interviewId,
                    interviewKey.ToString(),
                    assignment.Id,
                    calendarEvent.Comment);
                commandService.Execute(createCalendarEvent);
            }
            
            return interviewId.FormatGuid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartNewInterview(int id)
        {
            var assignment = this.assignments.GetAssignment(id);

            var interviewId = CreateInterview(assignment);
            TempData["lastCreatedInterviewId"] = interviewId; // todo replace with lastCreatedInterviewId from webinterview controller when its migrated

            return Json(new { InterviewId = interviewId});
        }

        [HttpGet]
        public IActionResult OpenInterview(Guid id)
        {
            return RedirectToAction("Cover", "WebInterview", new { id = id.FormatGuid() });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult DiscardInterview(Guid id)
        {
            var deleteInterview = new DeleteInterviewCommand(id, this.authorizedUser.Id);
            this.commandService.Execute(deleteInterview);
            return this.Content("ok");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RestartInterview(Guid id, string comment)
        {
            var restartCommand = new RestartInterviewCommand(id, this.authorizedUser.Id, comment, DateTime.UtcNow);

            this.commandService.Execute(restartCommand);

            return Content(Url.Content(GenerateUrl(@"Cover", id.FormatGuid())));
        }

        private List<QuestionnaireVersionsComboboxViewItem> GetQuestionnaires(InterviewStatus[] interviewStatuses)
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
            }).Select(q => new QuestionnaireIdentity(q.QuestionnaireId, q.QuestionnaireVersion)).ToArray();

            var questionnaires = questionnaireBrowseViewFactory.GetByIds(queryResult);

            return questionnaires.GetQuestionnaireComboboxViewItems();
        }

        private string GenerateUrl(string action, string interviewId, string sectionId = null) =>
            $@"~/WebInterview/{interviewId}/{action}" + (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
    }
}

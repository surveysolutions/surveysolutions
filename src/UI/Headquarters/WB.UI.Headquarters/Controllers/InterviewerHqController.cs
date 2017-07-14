using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Interviewer")]
    public class InterviewerHqController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IWebInterviewConfigProvider configProvider;
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
            this.configProvider = configProvider;
            this.usersRepository = usersRepository;
            this.interviewSummaryReader = interviewSummaryReader;
            this.assignments = assignments;
            this.keyGenerator = keyGenerator;
        }

        public ActionResult CreateNew()
        {
            return View("Index");
        }

        public ActionResult Rejected()
        {
            return View("Interviews");
        }

        public ActionResult Completed()
        {
            return View("Interviews");
        }

        public ActionResult Started()
        {
            return View("Interviews");
        }
        
        private string CreateInterview(Assignment assignment)
        {
            var webInterviewConfig = this.configProvider.Get(assignment.QuestionnaireId);
            if (!webInterviewConfig.Started)
                throw new InvalidOperationException(@"Web interview is not started for this questionnaire");

            var interviewer = this.usersRepository.GetUser(new UserViewInputModel(assignment.ResponsibleId));
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
            return interviewId.FormatGuid();
        }

        [HttpPost]
        public ActionResult StartNewInterview(int id)
        {
            var assignment = this.assignments.GetById(id);

            var interviewId = CreateInterview(assignment);

            return Content(Url.Content(GenerateUrl(@"Cover", interviewId)));
        }

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        public ActionResult QuestionnairesCombobox(string query = "", string statuses = null, int pageSize = 10)
        {
            IEnumerable<InterviewStatus> ParseStatuses()
            {
                foreach (var statusString in statuses.Split(','))
                {
                    if (Enum.TryParse(statusString, out InterviewStatus status))
                    {
                        yield return status;
                    }
                }
            }

            var interviewStatuses = ParseStatuses().ToArray();

            var queryResult = this.interviewSummaryReader.Query(_ =>
            {
                var filter = _
                    .Where(summary =>
                        !summary.IsDeleted
                        && summary.ResponsibleId == this.authorizedUser.Id
                        && summary.QuestionnaireTitle.Contains(query)
                    );

                if (statuses != null)
                {
                    filter = filter.Where(summary => interviewStatuses.Contains(summary.Status));
                }
                
                return filter.Select(s => new
                    {
                        s.QuestionnaireTitle,
                        s.QuestionnaireId,
                        s.QuestionnaireVersion
                    })
                    .Distinct().Take(pageSize).ToList();
            });

            var result = new ComboboxModel(queryResult.Select(s => new ComboboxOptionModel(
                new QuestionnaireIdentity(s.QuestionnaireId, s.QuestionnaireVersion).ToString(), 
                $@"(ver. {s.QuestionnaireVersion.ToString()}) {s.QuestionnaireTitle}")).ToArray());
            
            return JsonCamelCase(result);
        }

        private string GenerateUrl(string action, string interviewId, string sectionId = null) => 
            $@"~/WebInterview/{interviewId}/{action}" + (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
    }
}
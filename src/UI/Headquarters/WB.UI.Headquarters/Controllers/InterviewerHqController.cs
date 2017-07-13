using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Interviewer")]
    public class InterviewerHqController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IWebInterviewConfigProvider configProvider;
        private readonly IUserViewFactory usersRepository;
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;

        public InterviewerHqController(ICommandService commandService, ILogger logger,
            IWebInterviewConfigProvider configProvider,
            IUserViewFactory usersRepository,
            IPlainStorageAccessor<Assignment> assignments,
            IInterviewUniqueKeyGenerator keyGenerator) : base(commandService, logger)
        {
            this.commandService = commandService;
            this.configProvider = configProvider;
            this.usersRepository = usersRepository;
            this.assignments = assignments;
            this.keyGenerator = keyGenerator;
        }

        public ActionResult CreateNew()
        {
            return View("Index");
        }

        public ActionResult Rejected()
        {
            return View("Index");
        }

        public ActionResult Completed()
        {
            return View("Index");
        }

        public ActionResult Started()
        {
            return View("Index");
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

        

        private string GenerateUrl(string action, string interviewId, string sectionId = null) => 
            $@"~/WebInterview/{interviewId}/{action}" + (string.IsNullOrWhiteSpace(sectionId) ? "" : $@"/{sectionId}");
    }
}
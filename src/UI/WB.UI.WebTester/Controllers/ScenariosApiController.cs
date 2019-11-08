using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.UI.Shared.Web.Filters;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class ScenariosApiController : ApiController
    {
        private readonly ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage;
        private readonly IScenarioService scenarioService;
        private readonly IScenarioSerializer serializer;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public ScenariosApiController(IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage,
            IScenarioService scenarioService,
            IScenarioSerializer serializer)
        {
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.executedCommandsStorage = executedCommandsStorage ?? throw new ArgumentNullException(nameof(executedCommandsStorage));
            this.scenarioService = scenarioService ?? throw new ArgumentNullException(nameof(scenarioService));
            this.serializer = serializer;
        }

        [ApiNoCache]
        public HttpResponseMessage Get(string id)
        {
            var interview = statefulInterviewRepository.Get(id);
            var commands = this.executedCommandsStorage.Get(interview.Id, interview.Id);
            if (commands == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);
            var readyToBeStoredCommands = this.scenarioService.ConvertFromInterview(questionnaire, commands);
            var scenario = new Scenario
            {
                Steps = readyToBeStoredCommands
            };

            string response = this.serializer.Serialize(scenario);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}

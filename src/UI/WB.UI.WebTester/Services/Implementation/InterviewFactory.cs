using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.WebTester.Infrastructure;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage;
        private readonly ICommandService commandService;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IEvictionNotifier evictionService;
        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly IDesignerWebTesterApi webTesterApi;
        private readonly IScenarioService scenarioService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IScenarioSerializer serializer;
        private readonly IAggregateRootCache aggregateRootCache;

        public InterviewFactory(ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage,
            ICommandService commandService,
            IImageFileStorage imageFileStorage,
            IEvictionNotifier evictionService,
            IQuestionnaireImportService questionnaireImportService,
            IDesignerWebTesterApi webTesterApi,
            IScenarioService scenarioService,
            IQuestionnaireStorage questionnaireStorage,
            IScenarioSerializer serializer,
            IAggregateRootCache aggregateRootCache)
        {
            this.executedCommandsStorage = executedCommandsStorage ?? throw new ArgumentNullException(nameof(executedCommandsStorage));
            this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.imageFileStorage = imageFileStorage;
            this.evictionService = evictionService ?? throw new ArgumentNullException(nameof(evictionService));
            this.questionnaireImportService = questionnaireImportService;
            this.webTesterApi = webTesterApi ?? throw new ArgumentNullException(nameof(webTesterApi));
            this.scenarioService = scenarioService ?? throw new ArgumentNullException(nameof(scenarioService));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.aggregateRootCache = aggregateRootCache ?? throw new ArgumentNullException(nameof(aggregateRootCache));
        }

        public async Task CreateInterview(Guid designerToken)
        {
            var questionnaire = await questionnaireImportService.ImportQuestionnaire(designerToken);

            var createInterview = new CreateInterview(
                interviewId: designerToken,
                userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                questionnaireId: questionnaire,
                answers: new List<InterviewAnswer>(),
                protectedVariables: new List<string>(),
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: new InterviewKey(new Random().Next(99999999)),
                assignmentId: null,
                isAudioRecordingEnabled: false);

            this.commandService.Execute(createInterview);
        }

        public async Task<CreationResult> CreateInterview(Guid designerToken, int scenarioId)
        {
            var questionnaire = await questionnaireImportService.ImportQuestionnaire(designerToken);

            var createInterview = new CreateInterview(
                interviewId: designerToken,
                userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                questionnaireId: questionnaire,
                answers: new List<InterviewAnswer>(),
                protectedVariables: new List<string>(),
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: new InterviewKey(new Random().Next(99999999)),
                assignmentId: null,
                isAudioRecordingEnabled: false);

            this.commandService.Execute(createInterview);

            var scenarioSerialized = await this.webTesterApi.GetScenario(designerToken.ToString(), scenarioId);
            var scenario = this.serializer.Deserialize(scenarioSerialized);

            if (scenario == null)
                throw new InvalidOperationException("Scenario must not be null.");

            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaire, null);

            try
            {
                var commands = this.scenarioService.ConvertFromScenario(questionnaireDocument, scenario.Steps);

                foreach (var existingInterviewCommand in commands)
                {
                    existingInterviewCommand.InterviewId = designerToken;
                    this.commandService.Execute(existingInterviewCommand);
                }

                return CreationResult.DataRestored;
            }
            catch
            {
                Evict(designerToken);
                await this.CreateInterview(designerToken);
                return CreationResult.EmptyCreated;
            }
        }

        public async Task<CreationResult> CreateInterview(Guid designerToken, Guid originalInterviewId)
        {
            try
            {
                var questionnaireId = await questionnaireImportService.ImportQuestionnaire(designerToken);

                var createInterview = new CreateInterview(
                    interviewId: designerToken,
                    userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                    questionnaireId: questionnaireId,
                    answers: new List<InterviewAnswer>(),
                    protectedVariables: new List<string>(),
                    supervisorId: Guid.NewGuid(),
                    interviewerId: Guid.NewGuid(),
                    interviewKey: new InterviewKey(new Random().Next(99999999)),
                    assignmentId: null,
                    isAudioRecordingEnabled: false);

                this.commandService.Execute(createInterview);

                var existingInterviewCommands = this.executedCommandsStorage.Get(originalInterviewId, originalInterviewId) ??
                                                new List<InterviewCommand>();
                var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);

                var scenario = this.scenarioService.ConvertFromInterview(questionnaireDocument,
                    existingInterviewCommands.Cast<InterviewCommand>());
                var commands = this.scenarioService.ConvertFromScenario(questionnaireDocument, scenario);

                foreach (var existingInterviewCommand in commands)
                {
                    existingInterviewCommand.InterviewId = designerToken;
                    this.commandService.Execute(existingInterviewCommand);
                }

                foreach (var image in await this.imageFileStorage.GetBinaryFilesForInterview(originalInterviewId))
                {
                    var imageBytes = await this.imageFileStorage.GetInterviewBinaryDataAsync(image.InterviewId, image.FileName);

                    this.imageFileStorage.StoreInterviewBinaryData(designerToken, image.FileName, imageBytes,
                        image.ContentType);

                    await this.imageFileStorage.RemoveInterviewBinaryData(originalInterviewId, image.FileName);
                }

                return CreationResult.DataRestored;
            }
            catch (Exception ex)
            {
                Evict(designerToken);
                await this.CreateInterview(designerToken);
                return CreationResult.EmptyCreated;
            }
        }

        private void Evict(Guid designerToken)
        {
            aggregateRootCache.Evict(designerToken);
            evictionService.Evict(designerToken);
        }
    }
}

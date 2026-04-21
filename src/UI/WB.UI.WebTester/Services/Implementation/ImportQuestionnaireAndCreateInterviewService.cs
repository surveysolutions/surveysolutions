using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Refit;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.WebTester.Infrastructure;

namespace WB.UI.WebTester.Services.Implementation
{
    public class ImportQuestionnaireAndCreateInterviewService : IImportQuestionnaireAndCreateInterviewService
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

        public ImportQuestionnaireAndCreateInterviewService(
            ICacheStorage<List<InterviewCommand>, Guid> executedCommandsStorage,
            ICommandService commandService,
            IImageFileStorage imageFileStorage,
            IEvictionNotifier evictionService,
            IQuestionnaireImportService questionnaireImportService,
            IDesignerWebTesterApi webTesterApi,
            IScenarioService scenarioService,
            IQuestionnaireStorage questionnaireStorage,
            IScenarioSerializer serializer,
            IAggregateRootCache aggregateRootCache,
            ICacheStorage<string, CreationResult> cacheStorage)
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

        // Keyed by interviewId (unique per run).
        private static readonly ConcurrentDictionary<Guid, CreationResult> statuses = new();

        public Guid StartImportQuestionnaireAndCreateInterview(
            Guid questionnaireId,
            Guid interviewId,
            Guid? originalInterviewId,
            int? scenarioId)
        {
            if (statuses.TryGetValue(interviewId, out _))
                return interviewId;

            statuses[interviewId] = CreationResult.Loading;

            var task = ImportAndCreate(questionnaireId, interviewId, originalInterviewId, scenarioId);
            task.ContinueWith(result => statuses[interviewId] = result.Result);

            return interviewId;
        }

        public CreationResult? GetStatus(Guid interviewId)
        {
            statuses.TryGetValue(interviewId, out var result);
            return result == default ? null : result;
        }

        public CreationResult? RemoveStatus(Guid interviewId)
        {
            statuses.TryRemove(interviewId, out var result);
            return result == default ? null : result;
        }

        private async Task<CreationResult> ImportAndCreate(
            Guid questionnaireId, Guid interviewId, Guid? originalInterviewId, int? scenarioId)
        {
            try
            {
                var questionnaire = await ImportQuestionnaireAndCreateInterview(questionnaireId, interviewId);

                if (scenarioId.HasValue)
                    return await ApplyScenario(questionnaire, questionnaireId, interviewId, scenarioId.Value);
                if (originalInterviewId.HasValue)
                    return await ApplyInterviewData(questionnaire, questionnaireId, interviewId, originalInterviewId.Value);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return CreationResult.Error;
            }
            catch (TaskCanceledException)
            {
                return CreationResult.Error;
            }

            return CreationResult.EmptyCreated;
        }

        private async Task<QuestionnaireIdentity> ImportQuestionnaireAndCreateInterview(
            Guid questionnaireId, Guid interviewId)
        {
            // questionnaireId → Designer API; interviewId → local interview aggregate
            var questionnaire = await questionnaireImportService.ImportQuestionnaire(questionnaireId, interviewId);

            var createInterview = new CreateInterview(
                interviewId: interviewId,
                userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                questionnaireId: questionnaire,
                answers: new List<InterviewAnswer>(),
                protectedVariables: new List<string>(),
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: new InterviewKey(new Random().Next(99999999)),
                assignmentId: null,
                isAudioRecordingEnabled: false,
                InterviewMode.CAPI);

            this.commandService.Execute(createInterview);

            return questionnaire;
        }

        public async Task<CreationResult> ApplyScenario(
            QuestionnaireIdentity questionnaireIdentity, Guid questionnaireId, Guid interviewId, int scenarioId)
        {
            // Scenario fetched using questionnaireId (Designer API path segment).
            var scenarioSerialized = await this.webTesterApi.GetScenario(questionnaireId.ToString(), scenarioId);
            if (scenarioSerialized.StatusCode == HttpStatusCode.NotFound || scenarioSerialized.Content == null)
                return CreationResult.EmptyCreated;

            var scenario = this.serializer.Deserialize(scenarioSerialized.Content);
            if (scenario == null)
                return CreationResult.EmptyCreated;

            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            try
            {
                var commands = this.scenarioService.ConvertFromScenario(questionnaireDocument, scenario.Steps);

                foreach (var cmd in commands)
                {
                    cmd.InterviewId = interviewId;
                    this.commandService.Execute(cmd);
                }

                return CreationResult.DataRestored;
            }
            catch (InterviewException)
            {
                return CreationResult.DataPartialRestored;
            }
            catch
            {
                Evict(interviewId);
                await ImportQuestionnaireAndCreateInterview(questionnaireId, interviewId);
                return CreationResult.DataRestoreError;
            }
        }

        private async Task<CreationResult> ApplyInterviewData(
            QuestionnaireIdentity questionnaireIdentity, Guid questionnaireId, Guid interviewId, Guid originalInterviewId)
        {
            List<InterviewCommand>? existingInterviewCommands = null;
            int lastCommandIndex = 0;

            try
            {
                existingInterviewCommands =
                    this.executedCommandsStorage.Get(originalInterviewId, originalInterviewId) ??
                    new List<InterviewCommand>();
                var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

                var scenario = this.scenarioService.ConvertFromInterview(questionnaireDocument,
                    existingInterviewCommands.Cast<InterviewCommand>());
                var commands = this.scenarioService.ConvertFromScenario(questionnaireDocument, scenario);

                foreach (var image in await this.imageFileStorage.GetBinaryFilesForInterview(originalInterviewId))
                {
                    var imageBytes = await this.imageFileStorage
                        .GetInterviewBinaryDataAsync(image.InterviewId, image.FileName);
                    this.imageFileStorage.StoreInterviewBinaryData(interviewId, image.FileName, imageBytes,
                        image.ContentType);
                    await this.imageFileStorage.RemoveInterviewBinaryData(originalInterviewId, image.FileName);
                }

                foreach (var cmd in commands)
                {
                    cmd.InterviewId = interviewId;
                    this.commandService.Execute(cmd);
                    lastCommandIndex++;
                }

                return CreationResult.DataRestored;
            }
            catch (InterviewException)
            {
                if (existingInterviewCommands != null && existingInterviewCommands.Count > 0 &&
                    lastCommandIndex > 0)
                {
                    int count = existingInterviewCommands.Count - lastCommandIndex;
                    existingInterviewCommands.RemoveRange(lastCommandIndex, count);
                    this.executedCommandsStorage.Store(existingInterviewCommands,
                        originalInterviewId, originalInterviewId);
                    return CreationResult.DataPartialRestored;
                }

                Evict(interviewId);
                await ImportQuestionnaireAndCreateInterview(questionnaireId, interviewId);
                return CreationResult.DataRestoreError;
            }
            catch (Exception)
            {
                Evict(interviewId);
                await ImportQuestionnaireAndCreateInterview(questionnaireId, interviewId);
                return CreationResult.DataRestoreError;
            }
        }

        private void Evict(Guid interviewId)
        {
            aggregateRootCache.Evict(interviewId);
            evictionService.Evict(interviewId);
        }
    }
}

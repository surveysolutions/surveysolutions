﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.WebTester.Services
{
    public enum CreationResult
    {
        DataRestored,
        EmptyCreated
    }

    public interface IInterviewFactory
    {
        Task CreateInterview(Guid designerToken);

        Task<CreationResult> CreateInterview(Guid designerToken, Guid originalInterviewId);

        Task<CreationResult> CreateInterview(Guid designerToken, int scenarioId);
    }

    public class InterviewFactory : IInterviewFactory
    {
        private readonly ICacheStorage<List<ICommand>, Guid> executedCommandsStorage;
        private readonly ICommandService commandService;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IEvictionNotifier evictionService;
        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly IDesignerWebTesterApi webTesterApi;
        private readonly IScenarioService scenarioService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISerializer serializer;

        public InterviewFactory(ICacheStorage<List<ICommand>, Guid> executedCommandsStorage,
            ICommandService commandService,
            IImageFileStorage imageFileStorage,
            IEvictionNotifier evictionService,
            IQuestionnaireImportService questionnaireImportService, 
            IDesignerWebTesterApi webTesterApi, 
            IScenarioService scenarioService, 
            IQuestionnaireStorage questionnaireStorage,
            ISerializer serializer)
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
                isAudioRecordingEnabled:false);

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
            var scenario = this.serializer.Deserialize<Scenario>(scenarioSerialized);

            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaire, null);
            var commands = this.scenarioService.ConvertFromScenario(questionnaireDocument, scenario.Steps);
            try
            {

                foreach (var existingInterviewCommand in commands)
                {
                    existingInterviewCommand.InterviewId = designerToken;
                    this.commandService.Execute(existingInterviewCommand);
                }

                return CreationResult.DataRestored;
            }
            catch
            {
                evictionService.Evict(designerToken);
                await this.CreateInterview(designerToken);
                return CreationResult.EmptyCreated;
            }
        }

        public async Task<CreationResult> CreateInterview(Guid designerToken, Guid originalInterviewId)
        {
            try
            {
                var questionnaireIdentity = await questionnaireImportService.ImportQuestionnaire(designerToken);

                var existingInterviewCommands = this.executedCommandsStorage.Get(originalInterviewId, originalInterviewId);
                foreach (var existingInterviewCommand in existingInterviewCommands.Cast<InterviewCommand>())
                {
                    if (existingInterviewCommand is CreateInterview createCommand)
                    {
                        createCommand.QuestionnaireId = questionnaireIdentity;
                    }
                    existingInterviewCommand.InterviewId = designerToken;
                    this.commandService.Execute(existingInterviewCommand);
                }

                foreach (var image in this.imageFileStorage.GetBinaryFilesForInterview(originalInterviewId))
                {
                    var imageBytes = this.imageFileStorage.GetInterviewBinaryData(image.InterviewId, image.FileName);

                    this.imageFileStorage.StoreInterviewBinaryData(designerToken, image.FileName, imageBytes,
                        image.ContentType);

                    this.imageFileStorage.RemoveInterviewBinaryData(originalInterviewId, image.FileName);
                }

                return CreationResult.DataRestored;
            }
            catch (Exception)
            {
                evictionService.Evict(designerToken);
                await this.CreateInterview(designerToken);
                return CreationResult.EmptyCreated;
            }
        }
    }
}

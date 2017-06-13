using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class AssignmentsSynchronizer : IAssignmentsSynchronizer
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IPlainStorage<AssignmentDocument> assignmentsRepository;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAnswerToStringConverter answerToStringConverter;

        public AssignmentsSynchronizer(ISynchronizationService synchronizationService, 
            IPlainStorage<AssignmentDocument> assignmentsRepository, 
            IQuestionnaireDownloader questionnaireDownloader, 
            IQuestionnaireStorage questionnaireStorage,
            IAnswerToStringConverter answerToStringConverter)
        {
            this.synchronizationService = synchronizationService;
            this.assignmentsRepository = assignmentsRepository;
            this.questionnaireDownloader = questionnaireDownloader;
            this.questionnaireStorage = questionnaireStorage;
            this.answerToStringConverter = answerToStringConverter;
        }

        public virtual async Task SynchronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress,
            SychronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_Assignments,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            var remoteAssignments = await this.synchronizationService.GetAssignmentsAsync(cancellationToken);
            var localAssignments = this.assignmentsRepository.LoadAll();

            // removing local assignments if needed
            var remoteIds = remoteAssignments.ToLookup(ra => ra.Id);

            foreach (var assignment in localAssignments)
            {
                if (remoteIds.Contains(assignment.Id)) continue;
                statistics.RemovedAssignmentsCount += 1;
                this.assignmentsRepository.Remove(assignment.Id);
            }

            // adding new, updating capacity for existing
            var localAssignmentsLookup = localAssignments.ToLookup(la => la.Id);

            foreach (var remote in remoteAssignments)
            {
                var local = localAssignmentsLookup[remote.Id].FirstOrDefault();
                if (local == null)
                {
                    await this.questionnaireDownloader.DownloadQuestionnaireAsync(remote.QuestionnaireId, cancellationToken, statistics);
                    IQuestionnaire questionnaire = this.questionnaireStorage.GetQuestionnaire(remote.QuestionnaireId, null);

                    local = new AssignmentDocument
                    {
                        Id = remote.Id,
                        QuestionnaireId = remote.QuestionnaireId.ToString(),
                        Title = questionnaire.Title,
                        ReceivedDateUtc = DateTime.UtcNow
                    };

                    var identifyingData = new List<AssignmentDocument.IdentifyingAnswer>();

                    foreach (var identifyingAnswer in remote.IdentifyingData)
                    {
                        var questionType = questionnaire.GetQuestionType(identifyingAnswer.QuestionId);

                        if (questionType == QuestionType.GpsCoordinates)
                        {
                            local.LocationQuestionId = identifyingAnswer.QuestionId;
                            var geoPositionAnswer = GeoPosition.FromString(identifyingAnswer.Answer);
                            if (geoPositionAnswer != null)
                            {
                                local.LocationLatitude = geoPositionAnswer.Latitude;
                                local.LocationLongitude = geoPositionAnswer.Longitude;
                            }
                        }

                        var stringAnswer = this.answerToStringConverter.Convert(identifyingAnswer.Answer, identifyingAnswer.QuestionId, questionnaire);
                        identifyingData.Add(new AssignmentDocument.IdentifyingAnswer
                        {
                            QuestionId = identifyingAnswer.QuestionId,
                            Answer = identifyingAnswer.Answer,
                            AnswerAsString = stringAnswer,
                            Question = questionnaire.GetQuestionTitle(identifyingAnswer.QuestionId)
                        });
                    }

                    local.IdentifyingData = identifyingData;
                    statistics.NewAssignmentsCount += 1;
                }

                local.Quantity = remote.Quantity;
                local.InterviewsCount = remote.InterviewsCount;

                this.assignmentsRepository.Store(local);
            }
        }

        private static AssignmentDocument.IdentifyingAnswer CreateIdentifyingAnswer(
            AssignmentApiView.IdentifyingAnswer identifyingAnswer, 
            object questionAnswer, 
            IQuestionnaire questionnaireDocument)
        {
            return new AssignmentDocument.IdentifyingAnswer
            {
                QuestionId = identifyingAnswer.QuestionId,
                Answer = identifyingAnswer.Answer,
                AnswerAsString = AnswerUtils.AnswerToString(questionAnswer),
                Question = questionnaireDocument.GetQuestionTitle(identifyingAnswer.QuestionId)
            };
        }
    }
}
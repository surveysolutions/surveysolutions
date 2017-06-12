using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class AssignmentsSynchronizer : IAssignmentsSynchronizer
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IPlainStorage<AssignmentDocument> assignmentsRepository;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentsSynchronizer(ISynchronizationService synchronizationService, 
            IPlainStorage<AssignmentDocument> assignmentsRepository, 
            IQuestionnaireDownloader questionnaireDownloader, 
            IQuestionnaireStorage questionnaireStorage)
        {
            this.synchronizationService = synchronizationService;
            this.assignmentsRepository = assignmentsRepository;
            this.questionnaireDownloader = questionnaireDownloader;
            this.questionnaireStorage = questionnaireStorage;
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
                    IQuestionnaire questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(remote.QuestionnaireId, null);

                    local = new AssignmentDocument
                    {
                        Id = remote.Id,
                        QuestionnaireId = remote.QuestionnaireId.ToString(),
                        Title = questionnaireDocument.Title,
                    };

                    var identifyingData = new List<AssignmentDocument.IdentifyingAnswer>();

                    foreach (var identifyingAnswer in remote.IdentifyingData)
                    {
                        var questionType = questionnaireDocument.GetQuestionType(identifyingAnswer.QuestionId);

                        switch (questionType)
                        {
                            case QuestionType.SingleOption:
                                var questionTextAnswer =
                                    questionnaireDocument.GetAnswerOptionTitle(identifyingAnswer.QuestionId, int.Parse(identifyingAnswer.Answer));
                                identifyingData.Add(new AssignmentDocument.IdentifyingAnswer
                                {
                                    QuestionId = identifyingAnswer.QuestionId,
                                    Answer = identifyingAnswer.Answer,
                                    AnswerAsString = questionTextAnswer,
                                    Question = questionnaireDocument.GetQuestionTitle(identifyingAnswer.QuestionId)
                                });
                                break;
                            case QuestionType.GpsCoordinates:
                                local.LocationQuestionId = identifyingAnswer.QuestionId;
                                var geoPositionAnswer = GeoPosition.FromString(identifyingAnswer.Answer);
                                if (geoPositionAnswer != null)
                                {
                                    local.LocationLatitude = geoPositionAnswer.Latitude;
                                    local.LocationLongitude = geoPositionAnswer.Longitude;
                                }
                                break;
                            default:
                                identifyingData.Add(new AssignmentDocument.IdentifyingAnswer
                                {
                                    QuestionId = identifyingAnswer.QuestionId,
                                    Answer = identifyingAnswer.Answer,
                                    AnswerAsString = identifyingAnswer.Answer,
                                    Question = questionnaireDocument.GetQuestionTitle(identifyingAnswer.QuestionId)
                                });
                                break;
                        }
                    }

                    local.IdentifyingData = identifyingData;
                    statistics.NewAssignmentsCount += 1;
                }

                local.Quantity = remote.Quantity;
                local.InterviewsCount = remote.InterviewsCount;

                this.assignmentsRepository.Store(local);
            }
        }
    }
}
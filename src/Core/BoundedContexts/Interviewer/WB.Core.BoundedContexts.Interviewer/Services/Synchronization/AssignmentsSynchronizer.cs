using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class AssignmentsSynchronizer : IAssignmentsSynchronizer
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IPlainStorage<AssignmentDocument, int> assignmentsRepository;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAnswerToStringConverter answerToStringConverter;

        public AssignmentsSynchronizer(ISynchronizationService synchronizationService,
            IPlainStorage<AssignmentDocument, int> assignmentsRepository,
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
                await this.questionnaireDownloader.DownloadQuestionnaireAsync(remote.QuestionnaireId, cancellationToken, statistics);
                IQuestionnaire questionnaire = this.questionnaireStorage.GetQuestionnaire(remote.QuestionnaireId, null);

                var local = localAssignmentsLookup[remote.Id].FirstOrDefault();
                if (local == null)
                {
                    local = new AssignmentDocument
                    {
                        Id = remote.Id,
                        QuestionnaireId = remote.QuestionnaireId.ToString(),
                        Title = questionnaire.Title,
                        ReceivedDateUtc = DateTime.UtcNow
                    };

                    var identifyingData = this.FillAnswers(remote, questionnaire, local);

                    local.IdentifyingData = identifyingData;
                    statistics.NewAssignmentsCount++;
                }
                else
                {
                    local.IdentifyingData = FillAnswers(remote, questionnaire, local);
                }

                local.Quantity = remote.Quantity;
                local.InterviewsCount = remote.InterviewsCount;

                this.assignmentsRepository.Store(local);
            }
        }

        private List<AssignmentDocument.IdentifyingAnswer> FillAnswers(AssignmentApiView remote, IQuestionnaire questionnaire, AssignmentDocument local)
        {
            var identifyingData = new List<AssignmentDocument.IdentifyingAnswer>();

            foreach (var identifyingAnswer in remote.IdentifyingData)
            {
                var questionType = questionnaire.GetQuestionType(identifyingAnswer.Identity.Id);

                if (questionType == QuestionType.GpsCoordinates)
                {
                    local.LocationQuestionId = identifyingAnswer.Identity.Id;
                    var geoPositionAnswer = GeoPosition.FromString(identifyingAnswer.Answer);
                    if (geoPositionAnswer != null)
                    {
                        local.LocationLatitude = geoPositionAnswer.Latitude;
                        local.LocationLongitude = geoPositionAnswer.Longitude;
                    }
                }

                try
                {
                    string stringAnswer = this.answerToStringConverter.Convert(identifyingAnswer.Answer,
                        identifyingAnswer.Identity.Id, questionnaire);

                    identifyingData.Add(new AssignmentDocument.IdentifyingAnswer
                    {
                        Identity = identifyingAnswer.Identity,
                        Answer = identifyingAnswer.Answer,
                        AnswerAsString = stringAnswer,
                        Question = questionnaire.GetQuestionTitle(identifyingAnswer.Identity.Id)
                    });
                }
                catch (Exception)
                {
                    //BUG: most of question types cannot be restored from current string representation
                    // list questions should be serialized with values, as well as multi option answer should be parsable "2, 3"
                }
            }
            return identifyingData;
        }
    }
}
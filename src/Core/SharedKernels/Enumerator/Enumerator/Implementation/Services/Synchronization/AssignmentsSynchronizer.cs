using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public class AssignmentsSynchronizer : IAssignmentsSynchronizer
    {
        private readonly IAssignmentSynchronizationApi synchronizationService;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAnswerToStringConverter answerToStringConverter;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        public AssignmentsSynchronizer(IAssignmentSynchronizationApi synchronizationService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IQuestionnaireDownloader questionnaireDownloader,
            IQuestionnaireStorage questionnaireStorage,
            IAnswerToStringConverter answerToStringConverter,
            IInterviewAnswerSerializer answerSerializer, 
            IPlainStorage<InterviewView> interviewViewRepository)
        {
            this.synchronizationService = synchronizationService;
            this.assignmentsRepository = assignmentsRepository;
            this.questionnaireDownloader = questionnaireDownloader;
            this.questionnaireStorage = questionnaireStorage;
            this.answerToStringConverter = answerToStringConverter;
            this.answerSerializer = answerSerializer;
            this.interviewViewRepository = interviewViewRepository;
        }

        public virtual async Task SynchronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            List<AssignmentApiView> remoteAssignments = await this.synchronizationService.GetAssignmentsAsync(cancellationToken);

            IReadOnlyCollection<AssignmentDocument> localAssignments = this.assignmentsRepository.LoadAll();

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(0, remoteAssignments.Count),
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            // removing local assignments if needed
            var remoteIds = remoteAssignments.ToLookup(ra => ra.Id);

            foreach (var assignment in localAssignments)
            {
                if (remoteIds.Contains(assignment.Id)) continue;

                statistics.RemovedAssignmentsCount += 1;
                this.assignmentsRepository.Remove(assignment.Id);
            }

            // adding new, updating quantity for existing
            var localAssignmentsLookup = this.assignmentsRepository.LoadAll().ToLookup(la => la.Id);
            var processedAssignmentsCount = 0;

            foreach (var remoteItem in remoteAssignments)
            {
                processedAssignmentsCount++;

                await this.questionnaireDownloader.DownloadQuestionnaireAsync(remoteItem.QuestionnaireId, cancellationToken, statistics);

                IQuestionnaire questionnaire = this.questionnaireStorage.GetQuestionnaire(remoteItem.QuestionnaireId, null);

                var local = localAssignmentsLookup[remoteItem.Id].FirstOrDefault();
                if (local == null)
                {
                    var remote = await this.synchronizationService.GetAssignmentAsync(remoteItem.Id, cancellationToken);

                    local = new AssignmentDocument
                    {
                        Id = remote.Id,
                        QuestionnaireId = remote.QuestionnaireId.ToString(),
                        Title = questionnaire.Title,
                        Quantity = remote.Quantity,
                        CreatedInterviewsCount = 0,
                        LocationQuestionId = remote.LocationQuestionId,
                        LocationLatitude = remote.LocationLatitude,
                        LocationLongitude = remote.LocationLongitude,
                        ResponsibleId = remote.ResponsibleId,
                        ReceivedDateUtc = remote.CreatedAtUtc,
                        ProtectedVariables = remote.ProtectedVariables?.Select(x => new AssignmentDocument.AssignmentProtectedVariable
                        {
                            Variable = x,
                            AssignmentId = remote.Id
                        }).ToList() ?? new List<AssignmentDocument.AssignmentProtectedVariable>()
                    };

                    this.FillAnswers(remote, questionnaire, local);

                    statistics.NewAssignmentsCount++;

                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(processedAssignmentsCount, remoteAssignments.Count),
                        Statistics = statistics,
                        Status = SynchronizationStatus.Download
                    });

                    this.assignmentsRepository.Store(local);
                }
                else
                {
                    local.Quantity = remoteItem.Quantity;
                    var interviewsCount = this.interviewViewRepository.Count(x => x.CanBeDeleted && x.Assignment == local.Id);
                    local.CreatedInterviewsCount = interviewsCount;
                    this.assignmentsRepository.Store(local);
                }
            }

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(processedAssignmentsCount, remoteAssignments.Count),
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });
        }

        private void FillAnswers(AssignmentApiDocument remote, IQuestionnaire questionnaire, AssignmentDocument local)
        {
            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

            local.LocationLatitude = remote.LocationLatitude;
            local.LocationLongitude = remote.LocationLongitude;
            local.LocationQuestionId = remote.LocationQuestionId;

            var answers = new List<AssignmentDocument.AssignmentAnswer>();
            var identiAnswers = new List<AssignmentDocument.AssignmentAnswer>();

            foreach (var answer in remote.Answers)
            {
                var isIdentifying = identifyingQuestionIds.Contains(answer.Identity.Id);


                var assignmentAnswer = new AssignmentDocument.AssignmentAnswer
                {
                    AssignmentId = remote.Id,
                    Identity = answer.Identity,
                    SerializedAnswer = answer.SerializedAnswer,
                    Question = isIdentifying ? questionnaire.GetQuestionTitle(answer.Identity.Id) : null,
                    IsIdentifying = isIdentifying
                };

                if (isIdentifying && questionnaire.GetQuestionType(answer.Identity.Id) != QuestionType.GpsCoordinates)
                {
                    var abstractAnswer = this.answerSerializer.Deserialize<AbstractAnswer>(answer.SerializedAnswer);
                    var answerAsString = this.answerToStringConverter.Convert(abstractAnswer?.ToString(), answer.Identity.Id, questionnaire);
                    assignmentAnswer.AnswerAsString = answerAsString;

                    identiAnswers.Add(assignmentAnswer);
                }

                answers.Add(assignmentAnswer);
            }

            local.Answers = answers;
            local.IdentifyingAnswers = identiAnswers;
        }
    }
}

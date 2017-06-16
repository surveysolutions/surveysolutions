using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class AssignmentsSynchronizer : IAssignmentsSynchronizer
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentsSynchronizer(ISynchronizationService synchronizationService,
            IAssignmentDocumentsStorage assignmentsRepository,
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

            foreach (AssignmentApiView remote in remoteAssignments)
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

                    statistics.NewAssignmentsCount++;
                }

                this.FillAnswers(remote, questionnaire, local);
                local.Quantity = remote.Quantity;
                local.InterviewsCount = remote.InterviewsCount;

                this.assignmentsRepository.Store(local);
            }
        }

        private void FillAnswers(AssignmentApiView remote, IQuestionnaire questionnaire, AssignmentDocument local)
        {
            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

            local.LocationLatitude = remote.LocationLatitude;
            local.LocationLongitude = remote.LocationLongitude;
            var answers = new List<AssignmentDocument.AssignmentAnswer>();
            var identiAnswers = new List<AssignmentDocument.AssignmentAnswer>();

            foreach (var answer in remote.Answers)
            {
                var isIdentifying = identifyingQuestionIds.Contains(answer.Identity.Id);

                if (isIdentifying)
                {
                    identiAnswers.Add(new AssignmentDocument.AssignmentAnswer
                    {
                        AssignmentId = remote.Id,
                        Identity = answer.Identity,
                        AnswerAsString = answer.AnswerAsString,
                        Question = isIdentifying ? questionnaire.GetQuestionTitle(answer.Identity.Id) : null,
                        IsIdentifying = isIdentifying
                    });
                }

                answers.Add(new AssignmentDocument.AssignmentAnswer
                {
                    AssignmentId = remote.Id,
                    Identity = answer.Identity,
                    SerializedAnswer = answer.SerializedAnswer,
                    AnswerAsString = answer.AnswerAsString,
                    Question = isIdentifying ? questionnaire.GetQuestionTitle(answer.Identity.Id) : null,
                    IsIdentifying = isIdentifying
                });
            }

            local.Answers = answers;
            local.IdentifyingAnswers = identiAnswers;
        }
    }
}
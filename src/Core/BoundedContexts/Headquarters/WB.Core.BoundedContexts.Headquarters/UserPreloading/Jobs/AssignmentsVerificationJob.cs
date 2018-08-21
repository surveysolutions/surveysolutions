using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsVerificationJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsVerificationJob>();
        
        private IAssignmentsImportService importAssignmentsService => ServiceLocator.Current
            .GetInstance<IAssignmentsImportService>();

        private IPreloadedDataVerifier importAssignmentsVerifier => ServiceLocator.Current
            .GetInstance<IPreloadedDataVerifier>();

        private IQuestionnaireStorage questionnaireStorage => ServiceLocator.Current
            .GetInstance<IQuestionnaireStorage>();

        private SampleImportSettings sampleImportSettings => ServiceLocator.Current
            .GetInstance<SampleImportSettings>();
        private AssignmentsImportTask assignmentsImportTask => ServiceLocator.Current
            .GetInstance<AssignmentsImportTask>();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var importProcess = this.importAssignmentsService.GetImportStatus();
                if (importProcess?.ProcessStatus != AssignmentsImportProcessStatus.Verification) return;

                var allAssignmentIds = this.importAssignmentsService.GetAllAssignmentIdsToVerify();

                this.logger.Debug("Assignments verification job: Started");

                var sw = new Stopwatch();
                sw.Start();

                Parallel.ForEach(allAssignmentIds,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    assignmentId =>
                    {
                        try
                        {
                            ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                            var assignmentToVerify = this.importAssignmentsService.GetAssignmentById(assignmentId);
                            if (assignmentToVerify == null) return;

                            var questionnaire =  this.questionnaireStorage.GetQuestionnaire(importProcess.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                this.importAssignmentsService.RemoveAssignmentToImport(assignmentToVerify.Id);
                                return;
                            }

                            var error = 
                                this.importAssignmentsVerifier.VerifyWithInterviewTree(
                                    assignmentToVerify.Answers,
                                    assignmentToVerify.Interviewer ?? assignmentToVerify.Supervisor,
                                    questionnaire);

                            this.importAssignmentsService.SetVerifiedToAssignment(assignmentToVerify.Id, error?.ErrorMessage);
                        }
                        finally
                        {
                            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                        }
                    });

                    this.importAssignmentsService.SetImportProcessStatus(AssignmentsImportProcessStatus.Import);

                assignmentsImportTask.Run();

                sw.Stop();
                this.logger.Debug($"Assignments verfication job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Assignments verification job: FAILED. Reason: {ex.Message} ", ex);
            }
        }
    }
}

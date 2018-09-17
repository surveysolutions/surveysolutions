using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsVerificationJob : IJob
    {
        private readonly IServiceLocator serviceLocator;

        public AssignmentsVerificationJob(IServiceLocator locator)
        {
            this.serviceLocator = locator;
        }

        private ILogger logger => serviceLocator.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsVerificationJob>();
        

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                AssignmentsImportStatus importProcess = null;
                int[] allAssignmentIds = null;
                SampleImportSettings sampleImportSettings = null;

                serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                {
                    var importAssignmentsService = serviceLocatorLocal.GetInstance<IAssignmentsImportService>();
                    sampleImportSettings = serviceLocatorLocal.GetInstance<SampleImportSettings>();
                    importProcess = importAssignmentsService.GetImportStatus();
                    if (importProcess?.ProcessStatus != AssignmentsImportProcessStatus.Verification) return;

                    allAssignmentIds = importAssignmentsService.GetAllAssignmentIdsToVerify();
                });

                    
                if (importProcess?.ProcessStatus != AssignmentsImportProcessStatus.Verification) return;

                this.logger.Debug("Assignments verification job: Started");

                var sw = new Stopwatch();
                sw.Start();

                Parallel.ForEach(allAssignmentIds,
                    new ParallelOptions { MaxDegreeOfParallelism = sampleImportSettings.InterviewsImportParallelTasksLimit },
                    assignmentId =>
                    {
                        serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                        {
                            var threadImportAssignmentsService =
                                serviceLocatorLocal.GetInstance<IAssignmentsImportService>();
                            IQuestionnaireStorage threadQuestionnaireStorage =
                                serviceLocatorLocal.GetInstance<IQuestionnaireStorage>();
                            IPreloadedDataVerifier threadImportAssignmentsVerifier =
                                serviceLocatorLocal.GetInstance<IPreloadedDataVerifier>();

                            var assignmentToVerify = threadImportAssignmentsService.GetAssignmentById(assignmentId);
                            if (assignmentToVerify == null) return;

                            var questionnaire =
                                threadQuestionnaireStorage.GetQuestionnaire(importProcess.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentToVerify.Id);
                                return;
                            }

                            var error = threadImportAssignmentsVerifier.VerifyWithInterviewTree(
                                    assignmentToVerify.Answers,
                                    assignmentToVerify.Interviewer ?? assignmentToVerify.Supervisor,
                                    questionnaire);

                            threadImportAssignmentsService.SetVerifiedToAssignment(assignmentToVerify.Id,
                                error?.ErrorMessage);
                        });
                    });

                serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                {
                    var importAssignmentsService = serviceLocatorLocal.GetInstance<IAssignmentsImportService>();
                    importAssignmentsService.SetImportProcessStatus(AssignmentsImportProcessStatus.Import);
                });

                serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                {
                     serviceLocatorLocal.GetInstance<AssignmentsImportTask>().Run();
                });

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

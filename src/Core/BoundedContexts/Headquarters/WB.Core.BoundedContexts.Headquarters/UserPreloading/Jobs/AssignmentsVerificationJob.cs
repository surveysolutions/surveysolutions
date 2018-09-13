using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Threading;
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
        
        private IAssignmentsImportService importAssignmentsService => serviceLocator
            .GetInstance<IAssignmentsImportService>();

        private SampleImportSettings sampleImportSettings => serviceLocator
            .GetInstance<SampleImportSettings>();
        private AssignmentsImportTask assignmentsImportTask => serviceLocator
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
                        using (var scope = serviceLocator.CreateChildContainer())
                        {
                            //preserve scope
                            var threadServiceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

                            var threadImportAssignmentsService = threadServiceLocator.GetInstance<IAssignmentsImportService>();
                            IQuestionnaireStorage threadQuestionnaireStorage = threadServiceLocator.GetInstance<IQuestionnaireStorage>();
                            IPreloadedDataVerifier threadImportAssignmentsVerifier = threadServiceLocator.GetInstance<IPreloadedDataVerifier>();

                            var assignmentToVerify = threadImportAssignmentsService.GetAssignmentById(assignmentId);
                            if (assignmentToVerify == null) return;

                            var questionnaire = threadQuestionnaireStorage.GetQuestionnaire(importProcess.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentToVerify.Id);
                                return;
                            }

                            var error =
                                threadImportAssignmentsVerifier.VerifyWithInterviewTree(
                                    assignmentToVerify.Answers,
                                    assignmentToVerify.Interviewer ?? assignmentToVerify.Supervisor,
                                    questionnaire);

                            threadImportAssignmentsService.SetVerifiedToAssignment(assignmentToVerify.Id, error?.ErrorMessage);

                            serviceLocator.GetInstance<IUnitOfWork>().AcceptChanges();
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

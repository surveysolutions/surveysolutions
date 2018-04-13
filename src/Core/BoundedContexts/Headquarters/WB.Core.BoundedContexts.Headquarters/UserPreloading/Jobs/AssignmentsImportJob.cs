using System;
using System.Diagnostics;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsImportJob>();
        
        private IAssignmentsImportService importAssignmentsService => ServiceLocator.Current
            .GetInstance<IAssignmentsImportService>();

        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current
            .GetInstance<IPlainTransactionManager>();

        private ITransactionManager transactionManager => ServiceLocator.Current
            .GetInstance<ITransactionManager>();

        public void Execute(IJobExecutionContext context)
        {
            this.logger.Info("Assignments import job: Started");

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                var importProcess = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                    this.importAssignmentsService.GetImportStatus());

                if (importProcess == null) return;

                if (importProcess.AssignedToInterviewersCount + importProcess.AssignedToSupervisorsCount == 0)
                    return;

                if (importProcess.VerifiedAssignments != importProcess.TotalAssignments)
                    return;

                AssignmentToImport assignmentToImport = null;
                do
                {
                    assignmentToImport = this.plainTransactionManager.ExecuteInPlainTransaction(()=>this.importAssignmentsService.GetAssignmentToImport());
                    if (assignmentToImport == null) return;

                    this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                        this.transactionManager.ExecuteInQueryTransaction(
                            () =>
                            {
                                this.importAssignmentsService.ImportAssignment(assignmentToImport,
                                    importProcess.QuestionnaireIdentity);
                            }));

                } while (assignmentToImport != null);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Assignments import job: FAILED. Reason: {ex.Message} ", ex);
            }

            sw.Stop();
            this.logger.Info($"Assignments import job: Finished. Elapsed time: {sw.Elapsed}");
        }
    }
}

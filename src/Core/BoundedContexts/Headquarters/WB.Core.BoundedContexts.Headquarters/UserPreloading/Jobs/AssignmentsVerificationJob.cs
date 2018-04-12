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
    internal class AssignmentsVerificationJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsVerificationJob>();
        
        private IAssignmentsImportService importAssignmentsService => ServiceLocator.Current
            .GetInstance<IAssignmentsImportService>();
        
        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current
            .GetInstance<IPlainTransactionManager>();

        private ITransactionManager transactionManager => ServiceLocator.Current
            .GetInstance<ITransactionManager>();

        public void Execute(IJobExecutionContext context)
        {
            this.logger.Info("Assignments verification job: Started");

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                var importProcess = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                        this.importAssignmentsService.GetImportStatus());
                if (importProcess == null) return;

                AssignmentToImport assignmentToVerify = null;
                do
                {
                    plainTransactionManager.BeginTransaction();
                    transactionManager.BeginCommandTransaction();

                    assignmentToVerify = this.importAssignmentsService.GetAssignmentToVerify();
                    if (assignmentToVerify == null) break;

                    this.importAssignmentsService.VerifyAssignment(assignmentToVerify,
                        importProcess.QuestionnaireIdentity);

                    plainTransactionManager.CommitTransaction();
                    transactionManager.CommitCommandTransaction();

                } while (assignmentToVerify != null);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Assignments verification job: FAILED. Reason: {ex.Message} ", ex);
            }

            sw.Stop();
            this.logger.Info($"Assignments verfication job: Finished. Elapsed time: {sw.Elapsed}");
        }
    }
}

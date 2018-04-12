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
        private IPlainTransactionManager transactionManager =>
            ServiceLocator.Current.GetInstance<IPlainTransactionManager>();

        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsImportJob>();
        
        private IAssignmentsImportService importAssignmentsService => ServiceLocator.Current
            .GetInstance<IAssignmentsImportService>();
        
        private T PlainTransaction<T>(Func<T> func) 
            => transactionManager.ExecuteInPlainTransaction(func);
        
        private void PlainTransaction(Action action)
            => transactionManager.ExecuteInPlainTransaction(action);

        private void QueryTransaction(Action action)
            => ServiceLocator.Current.GetInstance<ITransactionManager>().ExecuteInQueryTransaction(action);

        public void Execute(IJobExecutionContext context)
        {
            this.logger.Info("Assignments import job: Started");

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                var importProcess = this.PlainTransaction(() => this.importAssignmentsService.GetImportStatus());

                if (importProcess == null ||
                    importProcess.AssignedToInterviewersCount + importProcess.AssignedToSupervisorsCount == 0)
                    return;

                if (importProcess.VerifiedAssignments != importProcess.TotalAssignments)
                    return;
                
                AssignmentToImport assignmentToImport = null;
                do
                {
                    this.transactionManager.BeginTransaction();

                    assignmentToImport = this.importAssignmentsService.GetAssignmentToImport();
                    if (assignmentToImport == null) return;

                    this.QueryTransaction(() => this.importAssignmentsService.ImportAssignment(assignmentToImport,
                        importProcess.QuestionnaireIdentity));

                    this.transactionManager.CommitTransaction();
                    
                } while (assignmentToImport != null);

                this.PlainTransaction(() => this.importAssignmentsService.RemoveAllAssignmentsToImport());
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

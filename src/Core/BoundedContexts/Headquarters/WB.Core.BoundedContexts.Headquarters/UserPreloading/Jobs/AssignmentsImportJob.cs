using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Threading;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
        private IServiceLocator serviceLocator;

        public AssignmentsImportJob(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        private ILogger logger => serviceLocator.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsImportJob>();
        
        private IAssignmentsImportService importAssignmentsService => serviceLocator
            .GetInstance<IAssignmentsImportService>();

        private SampleImportSettings sampleImportSettings => serviceLocator
            .GetInstance<SampleImportSettings>();

        private IQuestionnaireStorage questionnaireStorage => serviceLocator
            .GetInstance<IQuestionnaireStorage>();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var importProcess = this.importAssignmentsService.GetImportStatus();
                if (importProcess?.ProcessStatus != AssignmentsImportProcessStatus.Import) return;

                var allAssignmentIds = this.importAssignmentsService.GetAllAssignmentIdsToImport();

                this.logger.Debug("Assignments import job: Started");
                var sw = new Stopwatch();
                sw.Start();

                Parallel.ForEach(allAssignmentIds,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    assignmentId =>
                    {

                        using (var scope = serviceLocator.CreateChildContainer())
                        {
                            //preserve scope
                            var serviceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));
                            var threadImportAssignmentsService = scope.Resolve<IAssignmentsImportService>();

                            var questionnaire = scope.Resolve<IQuestionnaireStorage>().GetQuestionnaire(importProcess.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                                return;
                            }

                            threadImportAssignmentsService.ImportAssignment(assignmentId, importProcess.AssignedTo, questionnaire);
                            threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);

                            serviceLocator.GetInstance<IUnitOfWork>().AcceptChanges();
                        }
                        
                    });

                this.importAssignmentsService.SetImportProcessStatus(AssignmentsImportProcessStatus.ImportCompleted);

                sw.Stop();
                this.logger.Debug($"Assignments import job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Assignments import job: FAILED. Reason: {ex.Message} ", ex);
            }
        }
    }
}

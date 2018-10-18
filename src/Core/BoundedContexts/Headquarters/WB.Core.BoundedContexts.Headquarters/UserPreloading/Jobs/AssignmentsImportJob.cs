using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;
        private readonly IAssignmentsImportService assignmentsImportService;

        public AssignmentsImportJob(IServiceLocator serviceLocator, ILogger logger, IAssignmentsImportService assignmentsImportService)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.assignmentsImportService = assignmentsImportService;
        }
        
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var sampleImportSettings = serviceLocator.GetInstance<SampleImportSettings>();

                AssignmentsImportStatus importProcessStatus = assignmentsImportService.GetImportStatus();
                if (importProcessStatus?.ProcessStatus != AssignmentsImportProcessStatus.Import)
                    return;

                var allAssignmentIds = assignmentsImportService.GetAllAssignmentIdsToImport();
                
                if (importProcessStatus?.ProcessStatus != AssignmentsImportProcessStatus.Import)
                    return;

                this.logger.Debug("Assignments import job: Started");
                var sw = new Stopwatch();
                sw.Start();

                Parallel.ForEach(allAssignmentIds,
                    new ParallelOptions { MaxDegreeOfParallelism = sampleImportSettings.InterviewsImportParallelTasksLimit },
                    assignmentId =>
                    {
                        serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                        {
                            var threadImportAssignmentsService = serviceLocatorLocal.GetInstance<IAssignmentsImportService>();

                            var questionnaire = serviceLocatorLocal.GetInstance<IQuestionnaireStorage>().GetQuestionnaire(importProcessStatus.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                                return;
                            }

                            threadImportAssignmentsService.ImportAssignment(assignmentId, importProcessStatus.AssignedTo, questionnaire);
                            threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                        });
                    });

                assignmentsImportService.SetImportProcessStatus(AssignmentsImportProcessStatus.ImportCompleted);
                
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

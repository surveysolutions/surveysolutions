﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly ISystemLog systemLog;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        public AssignmentsImportJob(IServiceLocator serviceLocator, ILogger logger,
            IAssignmentsImportService assignmentsImportService, ISystemLog systemLog,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.assignmentsImportService = assignmentsImportService;
            this.systemLog = systemLog;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        public async Task Execute(IJobExecutionContext context)
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

                Guid responsibleId;

                using (var userManager = serviceLocator.GetInstance<HqUserManager>())
                {
                    responsibleId = (await userManager
                        .FindByNameAsync(importProcessStatus.ResponsibleName)).Id;
                }

                this.logger.Debug("Assignments import job: Started");
                var sw = new Stopwatch();
                sw.Start();

                int? lastImportedAssignmentId = null;
                int? firstImportedAssignmentId = null;

                Parallel.ForEach(allAssignmentIds,
                    new ParallelOptions { MaxDegreeOfParallelism = sampleImportSettings.InterviewsImportParallelTasksLimit },
                    assignmentId =>
                    {
                        InScopeExecutor.Current.Execute((serviceLocatorLocal) =>
                        {
                            var threadImportAssignmentsService = serviceLocatorLocal.GetInstance<IAssignmentsImportService>();

                            var questionnaire = serviceLocatorLocal.GetInstance<IQuestionnaireStorage>().GetQuestionnaire(importProcessStatus.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                                return;
                            }

                            var newAssignmentId = threadImportAssignmentsService.ImportAssignment(assignmentId,
                                importProcessStatus.AssignedTo, questionnaire, responsibleId);

                            if (!firstImportedAssignmentId.HasValue)
                                firstImportedAssignmentId = newAssignmentId;
                            else
                                lastImportedAssignmentId = newAssignmentId;

                            threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                        });
                    });

                assignmentsImportService.SetImportProcessStatus(AssignmentsImportProcessStatus.ImportCompleted);

                var questionnaireTitle = this.questionnaireBrowseViewFactory.GetById(importProcessStatus.QuestionnaireIdentity).Title;
                var questionnaireVersion = importProcessStatus.QuestionnaireIdentity.Version;

                this.systemLog.AssignmentsImported(importProcessStatus.TotalCount, questionnaireTitle,
                    questionnaireVersion, firstImportedAssignmentId ?? 0, lastImportedAssignmentId ?? firstImportedAssignmentId ?? 0, importProcessStatus.ResponsibleName);
                
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

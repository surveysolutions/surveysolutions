using System;
using System.Diagnostics;
using System.Linq;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsImportJob>();
        
        private IAssignmentsImportService importAssignmentsService => ServiceLocator.Current
            .GetInstance<IAssignmentsImportService>();

        private IInterviewImportService interviewImportService => ServiceLocator.Current
            .GetInstance<IInterviewImportService>();

        private IPlainStorageAccessor<Assignment> assignmentsStorage => ServiceLocator.Current
            .GetInstance<IPlainStorageAccessor<Assignment>>();

        private IInterviewCreatorFromAssignment interviewCreatorFromAssignment => ServiceLocator.Current
            .GetInstance<IInterviewCreatorFromAssignment>();

        private IQuestionnaireStorage questionnaireStorage => ServiceLocator.Current
            .GetInstance<IQuestionnaireStorage>();

        private T PlainTransaction<T>(Func<T> func) 
            => ServiceLocator.Current.GetInstance<IPlainTransactionManager>().ExecuteInPlainTransaction(func);

        private void PlainTransaction(Action action)
            => ServiceLocator.Current.GetInstance<IPlainTransactionManager>().ExecuteInPlainTransaction(action);

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

                if (importProcess.IsInProgress && importProcess.TotalAssignmentsWithResponsible == 0)
                    return;

                var questionnaire = this.PlainTransaction(() => questionnaireStorage.GetQuestionnaire(importProcess.QuestionnaireIdentity, null));

                AssignmentImportData assignmentToImport = null;
                do
                {
                    assignmentToImport = this.PlainTransaction(() => this.importAssignmentsService.GetAssignmentToImport());

                    if (assignmentToImport == null) break;

                    var responsibleId = assignmentToImport.InterviewerId ?? assignmentToImport.SupervisorId.Value;
                    var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();

                    var verificationResult = interviewImportService.VerifyAssignment(
                        assignmentToImport.PreloadedData.Answers.GroupedByLevels(), questionnaire);

                    if (!verificationResult.Status)
                    {
                        //return Content(HttpStatusCode.Forbidden, verificationResult.ErrorMessage);
                        continue;
                    }

                    var assignment = new Assignment(importProcess.QuestionnaireIdentity, responsibleId, assignmentToImport.Quantity);
                    var identifyingAnswers = assignmentToImport.PreloadedData.Answers
                        .Where(x => identifyingQuestionIds.Contains(x.Identity.Id)).Select(a =>
                            IdentifyingAnswer.Create(assignment, questionnaire, a.Answer.ToString(), a.Identity))
                        .ToList();

                    assignment.SetIdentifyingData(identifyingAnswers);
                    assignment.SetAnswers(assignment.Answers);

                    this.PlainTransaction(() => this.assignmentsStorage.Store(assignment, Guid.NewGuid()));

                    this.PlainTransaction(() => this.QueryTransaction(() =>
                    this.interviewCreatorFromAssignment.CreateInterviewIfQuestionnaireIsOld(responsibleId,
                        importProcess.QuestionnaireIdentity, assignment.Id, assignmentToImport.PreloadedData.Answers)));

                    this.PlainTransaction(() => this.importAssignmentsService.RemoveImportedAssignment(assignmentToImport));

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

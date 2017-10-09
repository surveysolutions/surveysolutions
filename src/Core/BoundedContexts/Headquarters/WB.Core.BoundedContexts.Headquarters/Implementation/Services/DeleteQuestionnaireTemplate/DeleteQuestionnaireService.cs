using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class DeleteQuestionnaireService : IDeleteQuestionnaireService
    {
        private readonly Func<IInterviewsToDeleteFactory> interviewsToDeleteFactory;
        private IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader => 
            ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireBrowseItem>>();
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly ITranslationManagementService translations;

        private readonly IInterviewImportService importService;
        private readonly IAuditLog auditLog;

        private static readonly object DeleteInProcessLockObject = new object();
        private static readonly HashSet<string> DeleteInProcess = new HashSet<string>();

        public DeleteQuestionnaireService(Func<IInterviewsToDeleteFactory> interviewsToDeleteFactory, 
            ICommandService commandService,
            ILogger logger, 
            ITranslationManagementService translations,
            IInterviewImportService importService,
            IAuditLog auditLog)
        {
            this.interviewsToDeleteFactory = interviewsToDeleteFactory;
            this.commandService = commandService;
            this.logger = logger;
            this.translations = translations;
            this.importService = importService;
            this.auditLog = auditLog;
        }

        public Task DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            this.logger.Warn($"Questionnaire {questionnaireId}${questionnaireVersion} deletion was triggered by {userId} user");
            var questionnaire =
                this.questionnaireBrowseItemReader.GetById(new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString());

            if (questionnaire != null)
            {
                this.auditLog.Append($"Questionnaire \"{questionnaire.Title} ver. {questionnaire.Version}\" delete was triggered");

                if (!questionnaire.Disabled)
                {
                    this.commandService.Execute(new DisableQuestionnaire(questionnaireId, questionnaireVersion,
                        userId));
                }
            }

            return Task.Factory.StartNew(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                try
                {
                    this.DeleteInterviewsAndQuestionnaireAfter(questionnaireId, questionnaireVersion, userId);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            });
        }

        private void DeleteInterviewsAndQuestionnaireAfter(Guid questionnaireId,
            long questionnaireVersion, Guid? userId)
        {
            var questionnaireKey = ObjectExtensions.AsCompositeKey(questionnaireId.FormatGuid(), questionnaireVersion);

            lock (DeleteInProcessLockObject)
            {
                if (DeleteInProcess.Contains(questionnaireKey))
                    return;

                DeleteInProcess.Add(questionnaireKey);
            }
            try
            {
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

                this.DeleteInterviews(questionnaireId, questionnaireVersion, userId);
                this.DeleteTranslations(questionnaireId, questionnaireVersion);

                var isAssignmentImportIsGoing = importService.Status.IsInProgress &&
                                                importService.Status.QuestionnaireId.Equals(questionnaireIdentity);

                if (!isAssignmentImportIsGoing)
                {
                    this.DeleteAssignments(new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

                    IPlainTransactionManager plainTransactionManager = ServiceLocator.Current
                        .GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();
                    plainTransactionManager.ExecuteInPlainTransaction(() =>
                        this.commandService.Execute(new DeleteQuestionnaire(questionnaireId, questionnaireVersion,
                            userId)));
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e.Message, e);
            }

            lock (DeleteInProcessLockObject)
            {
                DeleteInProcess.Remove(questionnaireKey);
            }
        }

        private void DeleteAssignments(QuestionnaireIdentity questionnaireIdentity)
        {
            var sessionProvider = ServiceLocator.Current.GetInstance<IAssignmetnsDeletionService>();
            sessionProvider.Delete(questionnaireIdentity);
        }

        private void DeleteTranslations(Guid questionnaireId, long questionnaireVersion)
        {
            IPlainTransactionManager plainTransactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();
            plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.translations.Delete(new QuestionnaireIdentity(questionnaireId, questionnaireVersion));
            });
        }

        private void DeleteInterviews(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            var exceptionsDuringDelete = new List<Exception>();

            IInterviewsToDeleteFactory toDeleteFactory = this.interviewsToDeleteFactory.Invoke();
            ITransactionManager transactionManager = ServiceLocator.Current.GetInstance<ITransactionManager>();
            List<InterviewSummary> listOfInterviews = transactionManager.ExecuteInQueryTransaction(() => 
                                                            toDeleteFactory.Load(questionnaireId, questionnaireVersion));
            do
            {
                try
                {
                    transactionManager.BeginCommandTransaction();

                    foreach (var interviewSummary in listOfInterviews)
                    {
                        this.commandService.Execute(new HardDeleteInterview(interviewSummary.InterviewId,
                            userId ?? interviewSummary.ResponsibleId));
                    }

                    transactionManager.CommitCommandTransaction();
                }
                catch (Exception e)
                {
                    transactionManager.RollbackCommandTransaction();
                    this.logger.Error(e.Message, e);
                    exceptionsDuringDelete.Add(e);
                }

                listOfInterviews = transactionManager.ExecuteInQueryTransaction(() =>
                                                            toDeleteFactory.Load(questionnaireId, questionnaireVersion));

            } while (
                exceptionsDuringDelete.Count == 0 && 
                listOfInterviews.Any());

            if(exceptionsDuringDelete.Count>0)
                throw new AggregateException(
                    $"interview delete process failed for questionnaire {questionnaireId.FormatGuid()} v. {questionnaireVersion}", exceptionsDuringDelete);
        }
    }
}

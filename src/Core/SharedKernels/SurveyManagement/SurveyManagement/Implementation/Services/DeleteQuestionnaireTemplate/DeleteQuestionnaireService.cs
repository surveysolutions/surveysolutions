using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class DeleteQuestionnaireService : IDeleteQuestionnaireService
    {
        private readonly Func<IInterviewsToDeleteFactory> interviewsToDeleteFactory;
        private IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireBrowseItem>>();
        private readonly ICommandService commandService;
        private readonly ILogger logger;

        private static readonly object DeleteInProcessLockObject = new object();
        private static readonly HashSet<string> DeleteInProcess = new HashSet<string>();

        public DeleteQuestionnaireService(Func<IInterviewsToDeleteFactory> interviewsToDeleteFactory, 
            ICommandService commandService,
            ILogger logger)
        {
            this.interviewsToDeleteFactory = interviewsToDeleteFactory;
            this.commandService = commandService;
            this.logger = logger;
        }

        public Task DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            var questionnaire =
                questionnaireBrowseItemReader.GetById(new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString());

            if (questionnaire != null)
            {
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
                DeleteInterviews(questionnaireId, questionnaireVersion, userId);


                IPlainTransactionManager plainTransactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
                plainTransactionManager.ExecuteInPlainTransaction(() =>
                    this.commandService.Execute(new DeleteQuestionnaire(questionnaireId, questionnaireVersion,
                        userId)));
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

        private void DeleteInterviews(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            var exceptionsDuringDelete = new List<Exception>();

            IInterviewsToDeleteFactory toDeleteFactory = interviewsToDeleteFactory.Invoke();
            ITransactionManager cqrsTransactionManager = ServiceLocator.Current.GetInstance<ITransactionManager>();
            List<InterviewSummary> listOfInterviews = cqrsTransactionManager.ExecuteInQueryTransaction(() => 
                                                            toDeleteFactory.Load(questionnaireId, questionnaireVersion));
            do
            {
                foreach (var interviewSummary in listOfInterviews)
                {
                    try
                    {
                        this.commandService.Execute(new HardDeleteInterview(interviewSummary.InterviewId, userId ?? interviewSummary.ResponsibleId));
                    }
                    catch (Exception e)
                    {
                       this.logger.Error(e.Message, e);
                       exceptionsDuringDelete.Add(e);
                    }
                }
                listOfInterviews = cqrsTransactionManager.ExecuteInQueryTransaction(() =>
                                                            toDeleteFactory.Load(questionnaireId, questionnaireVersion));

            } while (listOfInterviews.Any());

            if(exceptionsDuringDelete.Count>0)
                throw new AggregateException(string.Format("interview delete process failed for questionnaire {0} v. {1}", questionnaireId.FormatGuid(),questionnaireVersion), exceptionsDuringDelete);
        }
    }
}

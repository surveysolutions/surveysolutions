using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class DeleteQuestionnaireService : IDeleteQuestionnaireService
    {
        private readonly Func<IInterviewsToDeleteFactory> interviewsToDeleteFactory;
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemReader;
        private readonly ICommandService commandService;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly ILogger logger;

        private static readonly object DeleteInProcessLockObject = new object();
        private static readonly HashSet<string> DeleteInProcess = new HashSet<string>();

        public DeleteQuestionnaireService(Func<IInterviewsToDeleteFactory> interviewsToDeleteFactory, 
            ICommandService commandService,
            ILogger logger, 
            IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemReader, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.interviewsToDeleteFactory = interviewsToDeleteFactory;
            this.commandService = commandService;
            this.logger = logger;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public Task DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            ITransactionManager cqrsTransactionManager = ServiceLocator.Current.GetInstance<ITransactionManager>();
            var questionnaire = cqrsTransactionManager.ExecuteInQueryTransaction(() => 
                questionnaireBrowseItemReader.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion));

            if (questionnaire == null)
                throw new ArgumentException(string.Format("questionnaire with id {0} and version {1} is absent", questionnaireId.FormatGuid(), questionnaireVersion));

            if (!questionnaire.Disabled)
            {
                this.commandService.Execute(new DisableQuestionnaire(questionnaireId, questionnaireVersion,
                    userId));

                this.plainQuestionnaireRepository.DeleteQuestionnaireDocument(questionnaireId, questionnaireVersion);
            }

            return Task.Factory.StartNew(() =>
            {
                IsolatedThreadManager.MarkCurrentThreadAsIsolated();
                try
                {
                    this.DeleteInterviewsAndQuestionnaireAfter(questionnaireId, questionnaireVersion, userId);
                }
                finally
                {
                    IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
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

                this.commandService.Execute(new DeleteQuestionnaire(questionnaireId, questionnaireVersion,
                    userId));
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

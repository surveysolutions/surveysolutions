using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class DeleteQuestionnaireService : IDeleteQuestionnaireService
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemReader;
        private readonly ICommandService commandService;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly ILogger logger;

        private static readonly object DeleteInProcessLockObject = new object();
        private static readonly HashSet<string> DeleteInProcess = new HashSet<string>();

        public DeleteQuestionnaireService(IReadSideRepositoryIndexAccessor indexAccessor, ICommandService commandService,
            ILogger logger, IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemReader, IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.indexAccessor = indexAccessor;
            this.commandService = commandService;
            this.logger = logger;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public void DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            var questionnaire = questionnaireBrowseItemReader.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException(string.Format("questionnaire with id {0} and version {1} is absent", questionnaireId.FormatGuid(), questionnaireVersion));

            if (!questionnaire.Disabled)
                commandService.Execute(new DisableQuestionnaire(questionnaireId, questionnaireVersion,
                    userId));

            this.plainQuestionnaireRepository.DeleteQuestionnaireDocument(questionnaireId, questionnaireVersion);

            new Task(() =>
            {
                DeleteInterviewsAndQuestionnaireAfter(questionnaireId, questionnaireVersion, userId);

            }).Start();
        }

        private void DeleteInterviewsAndQuestionnaireAfter(Guid questionnaireId,
            long questionnaireVersion, Guid? userId)
        {
            var questionnaireKey = CreateQuestionnaireWithVersionKey(questionnaireId, questionnaireVersion);

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

        private IList<InterviewSummary> CreateInterviewForDeleteQuery(Guid questionnaireId,
         long questionnaireVersion)
        {
            return
                indexAccessor.Query<SeachIndexContent>(typeof (InterviewsSearchIndex).Name)
                    .Where(interview => !interview.IsDeleted &&
                                        interview.QuestionnaireId == questionnaireId &&
                                        interview.QuestionnaireVersion == questionnaireVersion)
                    .ProjectFromIndexFieldsInto<InterviewSummary>().ToList();
        }

        private void DeleteInterviews(Guid questionnaireId, long questionnaireVersion, Guid? userId)
        {
            var exceptionsDuringDelete=new List<Exception>();
            while (true)
            {
                var chunk =
                    CreateInterviewForDeleteQuery(questionnaireId, questionnaireVersion);

                if (!chunk.Any())
                    break;

                foreach (var interviewSummary in chunk)
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
            }

            if(exceptionsDuringDelete.Count>0)
                throw new AggregateException(string.Format("interview delete process failed for questionnaire {0} v. {1}", questionnaireId.FormatGuid(),questionnaireVersion), exceptionsDuringDelete);
        }

        private string CreateQuestionnaireWithVersionKey(Guid questionnaireId, long questionnaireVersion)
        {
            return string.Format("{0}${1}", questionnaireId.FormatGuid(), questionnaireVersion);
        }
    }
}

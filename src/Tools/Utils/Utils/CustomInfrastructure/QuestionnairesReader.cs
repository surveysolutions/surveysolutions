using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage;

namespace Utils.CustomInfrastructure
{
    public class QuestionnairesReader
    {
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IPlainTransactionManagerProvider plainTransactionManager;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnairesBrowseFactory;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummaries;


        public QuestionnairesReader(
            IQuestionnaireBrowseViewFactory questionnairesBrowseFactory,
            ITransactionManagerProvider transactionManager,
            INativeReadSideStorage<InterviewSummary> interviewSummaries,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository,
            IQuestionnaireStorage questionnaireStorage, 
            IPlainTransactionManagerProvider plainTransactionManager)
        {
            this.questionnairesBrowseFactory = questionnairesBrowseFactory;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManager = plainTransactionManager;
            this.questionnaireRepository = questionnaireRepository;
        }

        private List<QuestionnaireBrowseItem> questionnairesList;

        public QuestionnairesReader LoadQuestionnairesList()
        {
            var questionnaireBrowseItems = this.plainTransactionManager.GetPlainTransactionManager()
                .ExecuteInQueryTransaction(() =>
                    questionnairesBrowseFactory.Load(new QuestionnaireBrowseInputModel {PageSize = 1000}));

            this.questionnairesList = questionnaireBrowseItems.Items.ToList();
            Console.WriteLine($"Found {this.questionnairesList.Count} questionnaires");

            return this;
        }

        public QuestionnairesReader WithInterviewsOnly()
        {
            questionnairesList = questionnairesList.Where(questionnaireBrowseItem =>
            {
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version);
                return HasInterviews(questionnaireIdentity);
            }).ToList();
            
            return this;
        }

        private bool HasInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireIdentityAsString = questionnaireIdentity.ToString();
            var interviewsCount = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                this.interviewSummaries.Query(_ => _.Count(x => x.QuestionnaireIdentity == questionnaireIdentityAsString && x.WasCompleted)));
            return interviewsCount > 0;
        }

        public QuestionnairesReader ForEachQuestionnaire(Action<QuestionnaireIdentity, QuestionnaireDocument> action)
        {
            foreach (var questionnaireBrowseItem in questionnairesList)
            {
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version);

                var questionnaireRepositoryId = $"{questionnaireBrowseItem.QuestionnaireId.FormatGuid()}${questionnaireBrowseItem.Version}";
                QuestionnaireDocument questionnaire = this.plainTransactionManager.GetPlainTransactionManager()
                    .ExecuteInQueryTransaction(() => questionnaireRepository.GetById(questionnaireRepositoryId));
                
                Console.WriteLine("============================================");
                Console.WriteLine($"Questionnaire: {questionnaireRepositoryId}");
                Console.WriteLine($"               {questionnaire.Title}");

                questionnaireStorage.StoreQuestionnaire(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version, questionnaire);

                action(questionnaireIdentity, questionnaire);

                questionnaireStorage.DeleteQuestionnaireDocument(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version);
            }

            return this;
        }
    }
}

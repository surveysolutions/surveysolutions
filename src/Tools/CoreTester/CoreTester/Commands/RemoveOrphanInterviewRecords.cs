using System;
using System.Collections.Generic;
using System.Linq;
using CoreTester.CustomInfrastructure;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using ReflectionMagic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace CoreTester.Commands
{
    public class RemoveOrphanInterviewRecords
    {
        private readonly ILogger logger;
        private ITransactionManager TransactionManager => transactionManagerProvider.GetTransactionManager();
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IInterviewFactory interviewFactory;
        
        private readonly IDomainRepository domainRepository;

        public RemoveOrphanInterviewRecords(ILogger logger, 
            IDomainRepository domainRepository, 
            ITransactionManagerProvider transactionManagerProvider,
            IInterviewFactory interviewFactory)
        {
            this.logger = logger;
            this.domainRepository = domainRepository;
            this.transactionManagerProvider = transactionManagerProvider;
            this.interviewFactory = interviewFactory;
        }

        public int Run(string serverName)
        {
            logger.Info($"Orphans remover for db {serverName}");
            var fileName = $"{serverName}.oresults.txt";

            QuestionnairesReader questionnaires = ServiceLocator.Current.GetInstance<QuestionnairesReader>();
            
            questionnaires
                .LoadQuestionnairesList()
                .WithInterviewsOnly()
                .ForEachQuestionnaire((questionnaireIdentity, questionnaire) =>
                {
                    var interviews = ServiceLocator.Current.GetInstance<InterviewsReader>();
                    interviews
                        .LoadInterviewsListForQuestionnaire(questionnaireIdentity, questionnaire)
                        .Initialize()
                        .ForEachInterview(ProcessInterview)
                        .DumpResultsInFile(fileName)
                        .Finish();
                });

            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Finished at {DateTime.Now}");
            return 0;
        }

        private void ProcessInterview(Guid interviewId, InterviewsReader process, List<CommittedEvent> events)
        {
            StatefulInterview interview = (StatefulInterview) domainRepository.Load(typeof(StatefulInterview), interviewId, null, events);
            InterviewTree tree = (InterviewTree)interview.AsDynamic().Tree;

            if (tree == null)
                return;

            List<InterviewEntity> interviewAnswers = null;
            this.TransactionManager.ExecuteInQueryTransaction(() => { interviewAnswers = interviewFactory.GetInterviewEntities(interviewId); });

            var interviewState = new InterviewState{ Id = interviewId };
            foreach (var interviewEntity in interviewAnswers)
            {
                if (tree.GetNodeByIdentity(interviewEntity.Identity)!=null)
                    continue;

                interviewState.Removed.Add(InterviewStateIdentity.Create(interviewEntity.Identity));
            }

            if (!interviewState.Removed.Any()) return;

            Console.WriteLine(interviewId);
            //process.AddInterviewWithError(interviewId);

            this.TransactionManager.BeginCommandTransaction();
            interviewFactory.Save(interviewState);
            this.TransactionManager.CommitCommandTransaction();
        }
    }
}

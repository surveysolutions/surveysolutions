#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Services.DynamicReporting;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DynamicReporting
{
    public class ExposedVariablesService : IExposedVariablesService
    {
        private static readonly object UpdateReportsInProcessLockObject = new object();
        private static readonly HashSet<string> UpdateReportsInProcess = new HashSet<string>();

        private readonly IHeadquartersEventStore hqEventStore;
        private readonly IUnitOfWork sessionFactory;
        private readonly ILogger<ExposedVariablesService> logger;
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireCompositeItem;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader;
        private readonly IScheduledTask<UpdateDynamicReportJob, UpdateDynamicReportRequest> updateDynamicReportTask;
        private readonly IQuestionnaireStorage questionnaireStorage;


        public ExposedVariablesService(
            IUnitOfWork sessionFactory,
            ILogger<ExposedVariablesService> logger, IScheduledTask<UpdateDynamicReportJob, 
            UpdateDynamicReportRequest> updateDynamicReportTask, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader, 
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireCompositeItem,
            IHeadquartersEventStore hqEventStore,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.updateDynamicReportTask = updateDynamicReportTask ?? throw new ArgumentNullException(nameof(updateDynamicReportTask));
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader ?? throw new ArgumentNullException(nameof(questionnaireBrowseItemReader));
            this.questionnaireCompositeItem = questionnaireCompositeItem ?? throw new ArgumentNullException(nameof(questionnaireCompositeItem));
            this.hqEventStore = hqEventStore ?? throw new ArgumentNullException(nameof(hqEventStore));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
        }

        public async Task UpdateExposedVariables(QuestionnaireIdentity questionnaireIdentity, int[] exposedVariables, Guid userId)
        {
            this.logger.LogWarning("Exposed variables change was called for {questionnaireIdentity}. {userId} user", questionnaireIdentity, userId);
            var questionnaire = this.questionnaireBrowseItemReader.GetById(questionnaireIdentity.ToString());

            if (questionnaire != null)
            {
                var variables = this.questionnaireCompositeItem.Query(q =>
                {
                    q = q.Where(i => i.QuestionnaireIdentity == questionnaireIdentity.ToString());
                    return q.ToList();
                });

                var top15Variables = exposedVariables.Take(15).ToHashSet();

                foreach (var questionnaireCompositeItem in variables)
                {
                    questionnaireCompositeItem.UsedInReporting = top15Variables.Contains(questionnaireCompositeItem.Id);
                }

                this.questionnaireCompositeItem.Store(variables);

                await this.updateDynamicReportTask.Schedule(new UpdateDynamicReportRequest()
                {
                    Identity = questionnaireIdentity
                });
            }
        }

        public async Task UpdateDynamicReportDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireKey = questionnaireIdentity.ToString();

            lock (UpdateReportsInProcessLockObject)
            {
                if (UpdateReportsInProcess.Contains(questionnaireKey))
                    return;

                UpdateReportsInProcess.Add(questionnaireKey);
            }

            var sw = Stopwatch.StartNew();

            try
            {
                //clean up old records
                await RemoveAllDynamicReportDataAsync(questionnaireIdentity);

                RebuildAllDynamicReportDataAsync(questionnaireIdentity);

            }
            catch(Exception e)
            {
                this.logger.LogError(e, e.Message);
            }
            finally
            {
                sw.Stop();
                this.logger.LogInformation("Dynamic reports rebuilt for {id} in {seconds:0.00}s",
                    questionnaireIdentity, sw.Elapsed.TotalSeconds);

                lock (UpdateReportsInProcessLockObject)
                {
                    UpdateReportsInProcess.Remove(questionnaireKey);
                }
            }
        }

        private void RebuildAllDynamicReportDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var variables = this.questionnaireCompositeItem.Query(q =>
            {
                q = q.Where(i => 
                    i.UsedInReporting == true &&
                    i.QuestionnaireIdentity == questionnaireIdentity.ToString());
                return q.ToList();
            });

            if(variables.Count == 0)
                return;

            
            var interviews = this.sessionFactory.Session.Query<InterviewSummary>()
                .Where(s => s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && s.QuestionnaireVersion == questionnaireIdentity.Version);

            var dynamicReportDenormalizer = new InterviewDynamicReportAnswersDenormalizer(
                this.questionnaireStorage,
                this.questionnaireCompositeItem);


            foreach (var interviewSummary in interviews)
            {
                var events = this.hqEventStore.Read(interviewSummary.InterviewId, 0);

                dynamicReportDenormalizer.Handle(interviewSummary, events);
            }
        }

        private async Task RemoveAllDynamicReportDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            this.logger.LogWarning("Removing all dynamic report data for {questionnaireId}", questionnaireIdentity);
            
            var queryText = $"DELETE FROM interview_report_answers as ira " +
                                  $"USING questionnaire_entities as qe " +
                                  $"WHERE ira.entity_id = qe.id " +
                                  $"AND qe.questionnaireidentity = :questionnaireIdentity ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireIdentity", questionnaireIdentity.ToString());
            await query.ExecuteUpdateAsync();
        }
    }
}

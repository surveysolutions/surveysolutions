using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireEventFilter : IEventsFilter
    {
        private readonly TenantDbContext dbContext;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDatabaseSchemaService databaseSchemaService;

        public QuestionnaireEventFilter(TenantDbContext dbContext,
            IQuestionnaireStorage questionnaireStorage,
            IDatabaseSchemaService databaseSchemaService)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
            this.databaseSchemaService = databaseSchemaService;
        }

        public async Task<List<Event>> FilterAsync(ICollection<Event> feed, CancellationToken cancellationToken = default)
        {
            var filterWatch = Stopwatch.StartNew();
            List<Event> result = new List<Event>();
            HashSet<QuestionnaireIdentity> questionnaireIds = new HashSet<QuestionnaireIdentity>();

            foreach (var @event in feed)
            {
                try
                {
                    if (@event.Payload == null) continue;

                    var isInterviewEvent = @event.Payload is InterviewActiveEvent || @event.Payload is InterviewPassiveEvent;
                    if (!isInterviewEvent)
                    {
                        result.Add(@event);
                        continue;
                    }

                    InterviewReference reference;

                    switch (@event.Payload)
                    {
                        case InterviewCreated interviewCreated:
                            reference = await AddInterviewReferenceAsync(@event, interviewCreated.QuestionnaireIdentity, cancellationToken);
                            break;
                        case InterviewOnClientCreated interviewOnClientCreated:
                            reference = await AddInterviewReferenceAsync(@event, interviewOnClientCreated.QuestionnaireIdentity, cancellationToken);
                            break;
                        case InterviewFromPreloadedDataCreated fromPreloaded:
                            reference = await AddInterviewReferenceAsync(@event, fromPreloaded.QuestionnaireIdentity, cancellationToken);
                            break;
                        case InterviewDeleted _:
                        case InterviewHardDeleted _:
                            reference = await this.dbContext.InterviewReferences.FindAsync(@event.EventSourceId);
                            reference.DeletedAtUtc = @event.EventTimeStamp;
                            questionnaireStorage.InvalidateQuestionnaire(new QuestionnaireIdentity(reference.QuestionnaireId));
                            break;
                        default:
                            reference = await this.dbContext.InterviewReferences.FindAsync(@event.EventSourceId);
                            break;
                    }

                    if (reference == null)
                    {
                        throw new Exception(
                            $"Encountered interview id {@event.EventSourceId} without corresponding InterviewCreated or InterviewOnClientCreated events");
                    }

                    var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(new QuestionnaireIdentity(reference.QuestionnaireId), token: cancellationToken);

                    if (questionnaire == null)
                        throw new InvalidOperationException("questionnaire must be not null.");

                    if (!questionnaire.IsDeleted)
                    {
                        result.Add(@event);
                    }

                    questionnaireIds.Add(questionnaire.QuestionnaireId);
                }
                catch (Exception e)
                {
                    e.Data.Add("WB:Event", @event.EventTypeName);
                    e.Data.Add("WB:GlobalSequence", @event.GlobalSequence);
                    e.Data.Add("WB:EventSourceId", @event.EventSourceId);

                    throw;
                }
            }

            foreach (var questionnaireId in questionnaireIds)
            {
                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId, token: cancellationToken);
                if (questionnaire == null)
                    throw new InvalidOperationException("questionnaire must be not null.");

                databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            }

            filterWatch.Stop();
            return result;
        }

        private async Task<InterviewReference> AddInterviewReferenceAsync(Event @event, string questionnaireIdentity, CancellationToken cancellationToken)
        {
            InterviewReference reference = await this.dbContext.InterviewReferences.FindAsync(new object[] { @event.EventSourceId}, cancellationToken);
            if (reference == null)
            {
                reference = new InterviewReference { QuestionnaireId = questionnaireIdentity, InterviewId = @event.EventSourceId };

                this.dbContext.InterviewReferences.Add(reference);
            }

            return reference;
        }
    }
}

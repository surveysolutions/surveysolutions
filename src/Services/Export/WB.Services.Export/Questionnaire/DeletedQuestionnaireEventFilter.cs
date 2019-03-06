using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Questionnaire
{
    public class DeletedQuestionnaireEventFilter : IEventsFilter
    {
        private readonly ITenantContext tenantContext;
        private readonly TenantDbContext dbContext;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public DeletedQuestionnaireEventFilter(ITenantContext tenantContext,
            TenantDbContext dbContext,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.tenantContext = tenantContext;
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task<List<Event>> FilterAsync(ICollection<Event> feed)
        {
            var filterWatch = Stopwatch.StartNew();
            List<Event> result = new List<Event>();
            foreach (var @event in feed)
            {
                try
                {
                    if (@event.Payload == null) continue;

                    InterviewReference reference;
                    
                    switch (@event.Payload)
                    {
                        case InterviewCreated interviewCreated:
                            reference = AddInterviewReference(@event.EventSourceId, interviewCreated.QuestionnaireIdentity, @event);
                            break;
                        case InterviewOnClientCreated interviewOnClientCreated:
                            reference = AddInterviewReference(@event.EventSourceId, interviewOnClientCreated.QuestionnaireIdentity, @event);
                            break;
                        case InterviewDeleted _:
                        case InterviewHardDeleted _:
                            reference = this.dbContext.InterviewReferences.Find(@event.EventSourceId);
                            reference.DeletedAtUtc = @event.EventTimeStamp;
                            break;
                        default:
                            reference = this.dbContext.InterviewReferences.Find(@event.EventSourceId);
                            break;
                    }

                    if (reference == null)
                    {
                        throw new Exception(
                            $"Encountered interview id {@event.EventSourceId} without corresponding InterviewCreated or InterviewOnClientCreated events");
                    }

                    var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(new QuestionnaireId(reference.QuestionnaireId));

                    if (!questionnaire.IsDeleted)
                    {
                        result.Add(@event);
                    }
                }
                catch (Exception e)
                {
                    e.Data.Add("Event", @event.EventTypeName);
                    e.Data.Add("GlobalSequence", @event.GlobalSequence);
                    e.Data.Add("InterviewId", @event.EventSourceId);

                    throw;
                }
            }

            this.dbContext.SaveChanges();
            filterWatch.Stop();

            Monitoring.TrackEventHandlerProcessingSpeed(this.tenantContext?.Tenant?.Name,
                this.GetType(), feed.Count / filterWatch.Elapsed.TotalSeconds);

            return result;
        }

        private InterviewReference AddInterviewReference(Guid interviewId, string questionnaireIdentity, Event @event)
        {
            InterviewReference reference = this.dbContext.InterviewReferences.Find(@event.EventSourceId);
            if (reference == null)
            {
                reference = new InterviewReference { QuestionnaireId = questionnaireIdentity, InterviewId = interviewId };

                this.dbContext.Add(reference);
            }

            reference.QuestionnaireId = questionnaireIdentity;
            reference.InterviewId = @event.EventSourceId;
            return reference;
        }
    }
}

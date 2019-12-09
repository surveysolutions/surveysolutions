﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;

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

        public async Task<List<Event>> FilterAsync(ICollection<Event> feed)
        {
            var filterWatch = Stopwatch.StartNew();
            List<Event> result = new List<Event>();
            HashSet<QuestionnaireId> questionnaireIds = new HashSet<QuestionnaireId>();

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
                            reference = await AddInterviewReferenceAsync(@event, interviewCreated.QuestionnaireIdentity);
                            break;
                        case InterviewOnClientCreated interviewOnClientCreated:
                            reference = await AddInterviewReferenceAsync(@event,interviewOnClientCreated.QuestionnaireIdentity);
                            break;
                        case InterviewFromPreloadedDataCreated fromPreloaded:
                            reference = await AddInterviewReferenceAsync(@event, fromPreloaded.QuestionnaireIdentity);
                            break;
                        case InterviewDeleted _:
                        case InterviewHardDeleted _:
                            reference = await this.dbContext.InterviewReferences.FindAsync(@event.EventSourceId);
                            reference.DeletedAtUtc = @event.EventTimeStamp;
                            questionnaireStorage.InvalidateQuestionnaire(new QuestionnaireId(reference.QuestionnaireId));
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

                    var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(new QuestionnaireId(reference.QuestionnaireId));

                    if (!questionnaire.IsDeleted)
                    {
                        result.Add(@event);
                    }

                    questionnaireIds.Add(questionnaire.QuestionnaireId);
                }
                catch (Exception e)
                {
                    e.Data.Add("Event", @event.EventTypeName);
                    e.Data.Add("GlobalSequence", @event.GlobalSequence);
                    e.Data.Add("InterviewId", @event.EventSourceId);

                    throw;
                }
            }

            foreach (var questionnaireId in questionnaireIds)
            {
                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId);
                databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            }

            this.dbContext.SaveChanges();
            filterWatch.Stop();
            return result;
        }

        private async Task<InterviewReference> AddInterviewReferenceAsync(Event @event, string questionnaireIdentity)
        {
            InterviewReference reference = await this.dbContext.InterviewReferences.FindAsync(@event.EventSourceId);
            if (reference == null)
            {
                reference = new InterviewReference { QuestionnaireId = questionnaireIdentity, InterviewId = @event.EventSourceId };

                await this.dbContext.AddAsync(reference);
            }

            return reference;
        }
    }
}

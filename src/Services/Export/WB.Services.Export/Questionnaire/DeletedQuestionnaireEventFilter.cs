using System;
using System.Collections.Generic;
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
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDatabaseSchemaService databaseSchemaService;

        public DeletedQuestionnaireEventFilter(ITenantContext tenantContext,
            IQuestionnaireStorage questionnaireStorage,
            IDatabaseSchemaService databaseSchemaService)
        {
            this.tenantContext = tenantContext;
            this.questionnaireStorage = questionnaireStorage;
            this.databaseSchemaService = databaseSchemaService;
        }

        public async Task<List<Event>> FilterAsync(ICollection<Event> feed)
        {
            List<Event> result = new List<Event>();
            foreach (var @event in feed)
            {
                if (@event.Payload == null) continue;

                InterviewReference reference;
                bool forceUpdateQuestionnaire = false;
                
                switch (@event.Payload)
                {
                    case InterviewCreated interviewCreated:
                        AddInterviewReference(@event.EventSourceId, interviewCreated.QuestionnaireIdentity);
                        break;
                    case InterviewOnClientCreated interviewOnClientCreated:
                        AddInterviewReference(@event.EventSourceId, interviewOnClientCreated.QuestionnaireIdentity);
                        break;
                    case InterviewDeleted _:
                    case InterviewHardDeleted _:
                        reference = this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId);
                        reference.DeletedAtUtc = @event.EventTimeStamp;
                        forceUpdateQuestionnaire = true;
                        break;
                    default:
                        reference = this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId);
                        break;
                }

                void AddInterviewReference(Guid interviewId, string questionnaireIdentity)
                {
                    reference = this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId);
                    if (reference == null)
                    {
                        reference = new InterviewReference
                        {
                            QuestionnaireId = questionnaireIdentity,
                            InterviewId = interviewId
                        };

                        this.tenantContext.DbContext.Add(reference);
                    }

                    reference.QuestionnaireId = questionnaireIdentity;
                    reference.InterviewId = @event.EventSourceId;
                }

                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(tenantContext.Tenant,
                    new QuestionnaireId(reference.QuestionnaireId), forceUpdateQuestionnaire);

                if (questionnaire.IsDeleted)
                {
                    var deletedReference = tenantContext.DbContext.DeletedQuestionnaires.Find(reference.QuestionnaireId);

                    if (deletedReference == null)
                    {
                        databaseSchemaService.DropQuestionnaireDbStructure(questionnaire);

                        tenantContext.DbContext.DeletedQuestionnaires.Add(new DeletedQuestionnaireReference(reference.QuestionnaireId));
                    }
                }
                else
                {
                    result.Add(@event);
                }

            }

            this.tenantContext.DbContext.SaveChanges();

            return result;
        }
    }
}

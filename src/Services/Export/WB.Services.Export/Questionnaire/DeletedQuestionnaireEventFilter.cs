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
                bool rereadQuestionanire = false;
                switch (@event.Payload)
                {
                    case InterviewCreated interviewCreated:
                        reference =
                            this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId)
                            ?? new InterviewReference();

                        reference.QuestionnaireId = interviewCreated.QuestionnaireIdentity;
                        reference.InterviewId = @event.EventSourceId;
                        this.tenantContext.DbContext.Add(reference);
                        break;
                    case InterviewOnClientCreated interviewOnClientCreated:
                        reference =
                            this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId)
                            ?? new InterviewReference();

                        reference.QuestionnaireId = interviewOnClientCreated.QuestionnaireIdentity;
                        reference.InterviewId = @event.EventSourceId;
                        this.tenantContext.DbContext.Add(reference);
                        break;
                    case InterviewDeleted _:
                    case InterviewHardDeleted _:

                        reference = this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId);

                        reference.DeletedAtUtc = @event.EventTimeStamp;
                        rereadQuestionanire = true;
                        break;
                    default:
                        reference = this.tenantContext.DbContext.InterviewReferences.Find(@event.EventSourceId);
                        break;
                }

                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(tenantContext.Tenant,
                    new QuestionnaireId(reference.QuestionnaireId), rereadQuestionanire);

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

            return result;
        }
    }
}

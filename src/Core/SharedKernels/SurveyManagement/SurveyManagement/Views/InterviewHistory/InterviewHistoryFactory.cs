using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoryFactory : IViewFactory<InterviewHistoryInputModel, InterviewHistoryView>
    {
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryReader<UserDocument> userReader;
        private readonly IEventStore eventStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireReader;

        public InterviewHistoryFactory(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireReader,
            IReadSideRepositoryReader<UserDocument> userReader, IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader, IEventStore eventStore)
        {
            this.questionnaireReader = questionnaireReader;
            this.userReader = userReader;
            this.interviewSummaryReader = interviewSummaryReader;
            this.eventStore = eventStore;
        }

        public InterviewHistoryView Load(InterviewHistoryInputModel input)
        {
            var interviewSummary = interviewSummaryReader.GetById(input.InterviewId);
            if (interviewSummary == null)
                return null;
            var questionnaire = questionnaireReader.GetById(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
            if (questionnaire == null)
                return null;

            var historyRecords = this.RestoreInterviewHistory(input);

            var userCache = new Dictionary<Guid, UserDocument>();
            return new InterviewHistoryView(input.InterviewId,
                historyRecords.Select((record, index) => CreateInterviewHistoricalRecordView(index+1, record, questionnaire.Questionnaire, userCache))
                    .ToList(), interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
        }

        private List<InterviewHistoricalRecord> RestoreInterviewHistory(InterviewHistoryInputModel input)
        {
            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoricalRecord>();
            var interviewHistoryDenormalizer =
                new InterviewHistoryDenormalizer(interviewHistoryReader);

            var events = this.eventStore.ReadFrom(input.InterviewId, 0, long.MaxValue);
            foreach (var @event in events)
            {
                this.PublishToHandlers(@event, interviewHistoryDenormalizer);
            }
            var historyRecords = interviewHistoryReader.QueryAll(i => i.InterviewId == input.InterviewId).ToList();
            return historyRecords;
        }

        private void PublishToHandlers(IPublishableEvent eventMessage, 
            InterviewHistoryDenormalizer interviewHistoryDenormalizer)
        {

            var publishedEventClosedType = typeof (PublishedEvent<>).MakeGenericType(eventMessage.Payload.GetType());
            var publishedEvent = (PublishedEvent) Activator.CreateInstance(publishedEventClosedType, eventMessage);
            var handleMethod = typeof(InterviewHistoryDenormalizer).GetMethod("Handle", new[] { publishedEventClosedType });

            if(handleMethod==null)
                return;

            var occurredExceptions = new List<Exception>();
            
            try
            {
                handleMethod.Invoke(interviewHistoryDenormalizer, new object[] { publishedEvent });
            }
            catch (Exception exception)
            {
                occurredExceptions.Add(exception);
            }

            /* if (occurredExceptions.Count > 0)
                throw new AggregateException(
                    string.Format("{0} handler(s) failed to handle published event '{1}'.", occurredExceptions.Count, eventMessage.EventIdentifier),
                    occurredExceptions);*/
        }

        private InterviewHistoricalRecordView CreateInterviewHistoricalRecordView(long index, InterviewHistoricalRecord record, QuestionnaireDocument questionnaire, Dictionary<Guid, UserDocument> userCache)
        {
            var user = this.GetUserDocument(record.OriginatorId, userCache);
            var userName = this.GetUserName(user);
            var userRole = this.GetUserRole(user);
            if (record.Action == InterviewHistoricalAction.AnswerSet || record.Action == InterviewHistoricalAction.CommentSet)
            {
                var newParameters = new Dictionary<string, string>();
                var questionId = record.Parameters["questionId"];
                var question = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == Guid.Parse(questionId));
                if (question != null)
                {
                    newParameters["question"] = question.StataExportCaption;
                    if (record.Action == InterviewHistoricalAction.CommentSet)
                    {
                        newParameters["comment"] = record.Parameters["comment"];
                    }
                    if (record.Action == InterviewHistoricalAction.AnswerSet)
                    {
                        newParameters["answer"] = record.Parameters["answer"];
                    }
                    if (record.Parameters.ContainsKey("roster"))
                    {
                        newParameters["roster"] = record.Parameters["roster"];
                    }
                }
                return new InterviewHistoricalRecordView(index, record.Action, record.OriginatorId, userName, userRole, newParameters, record.Timestamp);
                    
            }
            if (record.Action == InterviewHistoricalAction.InterviewerAssigned)
            {
                if (record.Parameters.ContainsKey("responsible"))
                {
                    var responsibleId = Guid.Parse(record.Parameters["responsible"]);
                    return new InterviewHistoricalRecordView(index, record.Action, record.OriginatorId, userName, userRole,
                        new Dictionary<string, string> { {"responsible", this.GetUserName(GetUserDocument(responsibleId, userCache)) }},
                        record.Timestamp);
                }
            }
            return new InterviewHistoricalRecordView(index, record.Action, record.OriginatorId, userName, userRole, record.Parameters, record.Timestamp);
        }

        private UserDocument GetUserDocument(Guid userId, Dictionary<Guid, UserDocument> userCache)
        {
            var user = userCache.ContainsKey(userId) ? userCache[userId] : this.userReader.GetById(userId);
            userCache[userId] = user;
            return user;
        }

        private string GetUserName(UserDocument responsible)
        {
            var userName = responsible != null ? responsible.UserName : "";
            return userName;
        }

        private string GetUserRole(UserDocument user)
        {
            const string UnknownUserRole = "";
            if (user == null || !user.Roles.Any())
                return UnknownUserRole;
            var firstRole = user.Roles.First();
            switch (firstRole)
            {
                case UserRoles.Operator: return "Interviewer";
                case UserRoles.Supervisor: return "Supervisor";
                case UserRoles.Headquarter: return "Headquarter";
            }
            return UnknownUserRole;
        }
    }
}

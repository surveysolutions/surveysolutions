using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewHistoryDenormalizer : IEventHandler<InterviewApprovedByHQ>,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<NumericQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<AnswerCommented>,

        IEventHandler
    {
        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(InterviewData), typeof(UserDocument), typeof(InterviewHistoricalRecord) }; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(InterviewHistoricalRecord) }; }
        }

        private readonly IReadSideRepositoryWriter<InterviewHistoricalRecord> interviewHistoryStore;

        public InterviewHistoryDenormalizer(IReadSideRepositoryWriter<InterviewHistoricalRecord> interviewHistoryStore)
        {
            this.interviewHistoryStore = interviewHistoryStore;
        }

        private InterviewHistoricalRecord CreateHistoricalRecord(Guid interviewId, InterviewHistoricalAction action, Guid userId,
            DateTime timestamp, Dictionary<string, string> parameters = null)
        {
            return new InterviewHistoricalRecord(interviewId, action, userId, parameters ?? new Dictionary<string, string>(), timestamp);
        }

        private void AddInterviewAction(Guid recordId, Guid interviewId, DateTime timeStamp, InterviewHistoricalAction action, Guid userId, Dictionary<string, string> parameters = null)
        {
            interviewHistoryStore.Store(CreateHistoricalRecord(interviewId, action, userId, timeStamp, parameters), recordId.FormatGuid());
        }

        private Dictionary<string, string> CreateCommentParameters(string comment)
        {
            if (string.IsNullOrEmpty(comment))
                return null;
            return new Dictionary<string, string> { { "comment", comment } };
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp, InterviewHistoricalAction.RejectedByHeadquarter,
               evnt.Payload.UserId, CreateCommentParameters(evnt.Payload.Comment));
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp, InterviewHistoricalAction.SupervisorAssigned,
                evnt.Payload.UserId);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp, InterviewHistoricalAction.InterviewerAssigned,
                evnt.Payload.UserId, new Dictionary<string, string> { { "responsible", evnt.Payload.InterviewerId.FormatGuid() } });
        }


        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var newStatus = evnt.Payload.Status;
            if (newStatus != InterviewStatus.Completed && newStatus != InterviewStatus.Restarted)
                return;

            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp,
                newStatus == InterviewStatus.Completed ? InterviewHistoricalAction.Completed : InterviewHistoricalAction.Restarted,
                /*evnt.Payload.UserId*/Guid.Empty, CreateCommentParameters(evnt.Payload.Comment));
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp, InterviewHistoricalAction.ApproveBySupervisor,
                 evnt.Payload.UserId, CreateCommentParameters(evnt.Payload.Comment));
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp, InterviewHistoricalAction.RejectedBySupervisor,
                evnt.Payload.UserId, CreateCommentParameters(evnt.Payload.Comment));
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.EventTimeStamp, InterviewHistoricalAction.RejectedByHeadquarter,
                evnt.Payload.UserId, CreateCommentParameters(evnt.Payload.Comment));
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
                evnt.Payload.UserId,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
                evnt.Payload.UserId,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedValues), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
               evnt.Payload.UserId,
               CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedValue), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
              evnt.Payload.UserId,
              CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
              evnt.Payload.UserId,
              CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
             evnt.Payload.UserId,
             CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
              evnt.Payload.UserId,
              CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
            evnt.Payload.UserId,
            CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Altitude,
                        evnt.Payload.Timestamp)), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
          evnt.Payload.UserId,
          CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedPropagationVectors), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
           evnt.Payload.UserId,
           CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedPropagationVector), evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
                evnt.Payload.UserId,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(new InterviewTextListAnswers(evnt.Payload.Answers)),
                    evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
                evnt.Payload.UserId,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer),
                    evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.AnswerTime, InterviewHistoricalAction.AnswerSet,
               evnt.Payload.UserId,
               CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.PictureFileName),
                   evnt.Payload.PropagationVector));
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            var parameters = new Dictionary<string, string>()
            {
                { "questionId", evnt.Payload.QuestionId.FormatGuid() },
                { "comment", evnt.Payload.Comment }
            };
            if (evnt.Payload.PropagationVector.Length > 0)
            {
                parameters.Add("roster", string.Join(",", evnt.Payload.PropagationVector));
            }
            AddInterviewAction(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.CommentTime, InterviewHistoricalAction.CommentSet,
              evnt.Payload.UserId, parameters);
        }

        private Dictionary<string, string> CreateAnswerParameters(Guid questionId, string answer, decimal[] propagationVector)
        {
            var result = new Dictionary<string, string>()
            {
                { "questionId", questionId.FormatGuid() },
                { "answer", answer }
            };

            if (propagationVector.Length > 0)
            {
                result.Add("roster", string.Join(",", propagationVector));
            }
            return result;
        }
    }
}

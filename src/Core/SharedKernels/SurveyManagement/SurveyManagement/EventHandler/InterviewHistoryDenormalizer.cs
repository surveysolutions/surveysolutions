﻿using System;
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
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewHistoryDenormalizer : AbstractFunctionalEventHandler<InterviewHistoryView, IReadSideRepositoryWriter<InterviewHistoryView>>,
        IUpdateHandler<InterviewHistoryView, SupervisorAssigned>,
        IUpdateHandler<InterviewHistoryView, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewHistoryView, InterviewerAssigned>,
        IUpdateHandler<InterviewHistoryView, InterviewStatusChanged>,
        IUpdateHandler<InterviewHistoryView, InterviewApproved>,
        IUpdateHandler<InterviewHistoryView, InterviewRejected>,
        IUpdateHandler<InterviewHistoryView, InterviewRestored>,
        IUpdateHandler<InterviewHistoryView, InterviewRejectedByHQ>,
        IUpdateHandler<InterviewHistoryView, TextQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, TextListQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, PictureQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, AnswerCommented>,
        IUpdateHandler<InterviewHistoryView, InterviewDeleted>,
        IUpdateHandler<InterviewHistoryView, InterviewHardDeleted>
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireReader;

        public InterviewHistoryDenormalizer(IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader, IReadSideRepositoryWriter<UserDocument> userReader,
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireReader)
            : base(readSideStorage)
        {
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireReader = questionnaireReader;
        }

        public override object[] Readers
        {
            get { return new object[] { interviewSummaryReader, userReader, questionnaireReader }; }
        }

        public InterviewHistoryView Update(InterviewHistoryView currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interviewSummary = interviewSummaryReader.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return null;

            var view = new InterviewHistoryView(evnt.EventSourceId, new List<InterviewHistoricalRecordView>(), interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            AddHistoricalRecord(view, InterviewHistoricalAction.SupervisorAssigned, evnt.Payload.UserId, evnt.EventTimeStamp);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.ApproveByHeadquarter, evnt.Payload.UserId, evnt.EventTimeStamp,
                CreateCommentParameters(evnt.Payload.Comment));
            
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewerAssigned> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.InterviewerAssigned, evnt.Payload.UserId, evnt.EventTimeStamp,
                new Dictionary<string, string> { { "responsible", evnt.Payload.InterviewerId.FormatGuid() } });

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var newStatus = evnt.Payload.Status;
            if (newStatus != InterviewStatus.Completed && newStatus != InterviewStatus.Restarted)
                return view;

            AddHistoricalRecord(view, newStatus == InterviewStatus.Completed ? InterviewHistoricalAction.Completed : InterviewHistoricalAction.Restarted, Guid.Empty, evnt.EventTimeStamp,
               CreateCommentParameters(evnt.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApproved> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.ApproveBySupervisor, evnt.Payload.UserId, evnt.EventTimeStamp,
              CreateCommentParameters(evnt.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejected> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.RejectedBySupervisor, evnt.Payload.UserId, evnt.EventTimeStamp,
              CreateCommentParameters(evnt.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.RejectedByHeadquarter, evnt.Payload.UserId, evnt.EventTimeStamp,
             CreateCommentParameters(evnt.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewDeleted> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.Deleted, evnt.Payload.UserId, evnt.EventTimeStamp);

            return view;
        }


        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestored> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.Restored, evnt.Payload.UserId, evnt.EventTimeStamp);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
            CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer), evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedValues),
                    evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedValue),
                    evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer),
                    evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
             CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer),
                 evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
                CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer),
                    evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
          CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Altitude,
                        evnt.Payload.Timestamp)),
              evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
              CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedPropagationVectors),
                  evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
             CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.SelectedPropagationVector),
                 evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
           CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(new InterviewTextListAnswers(evnt.Payload.Answers)),
               evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
            CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.Answer),
            evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, evnt.Payload.UserId, evnt.EventTimeStamp,
           CreateAnswerParameters(evnt.Payload.QuestionId, AnswerUtils.AnswerToString(evnt.Payload.PictureFileName),
           evnt.Payload.PropagationVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswerCommented> evnt)
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

            AddHistoricalRecord(view, InterviewHistoricalAction.CommentSet, evnt.Payload.UserId, evnt.EventTimeStamp, parameters);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return null;
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

        private InterviewHistoricalRecordView CreateInterviewHistoricalRecordView(InterviewHistoricalAction action, Guid userId,
            DateTime timestamp, Dictionary<string, string> parameters, QuestionnaireDocument questionnaire)
        {
            var user = this.GetUserDocument(userId);
            var userName = this.GetUserName(user);
            var userRole = this.GetUserRole(user);
            if (action == InterviewHistoricalAction.AnswerSet || action == InterviewHistoricalAction.CommentSet)
            {
                var newParameters = new Dictionary<string, string>();
                var questionId = parameters["questionId"];
                var question = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == Guid.Parse(questionId));
                if (question != null)
                {
                    newParameters["question"] = question.StataExportCaption;
                    if (action == InterviewHistoricalAction.CommentSet)
                    {
                        newParameters["comment"] = parameters["comment"];
                    }
                    if (action == InterviewHistoricalAction.AnswerSet)
                    {
                        newParameters["answer"] = parameters["answer"];
                    }
                    if (parameters.ContainsKey("roster"))
                    {
                        newParameters["roster"] = parameters["roster"];
                    }
                }
                return new InterviewHistoricalRecordView(0, action, userName, userRole, newParameters, timestamp);

            }
            if (action == InterviewHistoricalAction.InterviewerAssigned)
            {
                if (parameters.ContainsKey("responsible"))
                {
                    var responsibleId = Guid.Parse(parameters["responsible"]);
                    return new InterviewHistoricalRecordView(0, action, userName, userRole,
                        new Dictionary<string, string> { { "responsible", this.GetUserName(GetUserDocument(responsibleId)) } },
                        timestamp);
                }
            }
            return new InterviewHistoricalRecordView(0, action, userName, userRole, parameters, timestamp);
        }

        private UserDocument GetUserDocument(Guid originatorId)
        {
            return userReader.GetById(originatorId);
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

        private void AddHistoricalRecord(InterviewHistoryView view, InterviewHistoricalAction action, Guid userId, DateTime timestamp,
            Dictionary<string, string> parameters = null)
        {
            if(view ==null)
                return;

            var questionnaireWithVersion = questionnaireReader.GetById(view.QuestionnaireId, view.QuestionnaireVersion);
            if (questionnaireWithVersion == null || questionnaireWithVersion.Questionnaire == null)
                return;

            var questionnaire = questionnaireWithVersion.Questionnaire;

            var record = CreateInterviewHistoricalRecordView(action, userId, timestamp, parameters ?? new Dictionary<string, string>(),
                questionnaire);
            
            if(record==null)
                return;
            
            view.Records.Add(record);
        }

        private Dictionary<string, string> CreateCommentParameters(string comment)
        {
            if (string.IsNullOrEmpty(comment))
                return null;
            return new Dictionary<string, string> { { "comment", comment } };
        }
    }
}

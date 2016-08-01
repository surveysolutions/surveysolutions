using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class InterviewParaDataEventHandler : 
        IUpdateHandler<InterviewHistoryView, SupervisorAssigned>,
        IUpdateHandler<InterviewHistoryView, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewHistoryView, InterviewerAssigned>,
        IUpdateHandler<InterviewHistoryView, InterviewCompleted>,
        IUpdateHandler<InterviewHistoryView, InterviewRestarted>,
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
        IUpdateHandler<InterviewHistoryView, YesNoQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, PictureQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, AnswerCommented>,
        IUpdateHandler<InterviewHistoryView, InterviewDeleted>,
        IUpdateHandler<InterviewHistoryView, InterviewHardDeleted>,
        IUpdateHandler<InterviewHistoryView, AnswersDeclaredInvalid>,
        IUpdateHandler<InterviewHistoryView, AnswersDeclaredValid>,
        IUpdateHandler<InterviewHistoryView, QuestionsDisabled>,
        IUpdateHandler<InterviewHistoryView, QuestionsEnabled>,
        IUpdateHandler<InterviewHistoryView, GroupsDisabled>,
        IUpdateHandler<InterviewHistoryView, GroupsEnabled>,
        IUpdateHandler<InterviewHistoryView, AnswerRemoved>,
        IUpdateHandler<InterviewHistoryView, UnapprovedByHeadquarters>,
        IUpdateHandler<InterviewHistoryView, InterviewReceivedByInterviewer>,
        IUpdateHandler<InterviewHistoryView, InterviewReceivedBySupervisor>
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<UserDocument> userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage;
        private readonly ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireExportStructure> cacheQuestionnaireExportStructure = new ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireExportStructure>();
        private readonly ConcurrentDictionary<string, UserDocument> cacheUserDocument = new ConcurrentDictionary<string, UserDocument>();

        private readonly InterviewDataExportSettings interviewDataExportSettings;
        public InterviewParaDataEventHandler(
            IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IPlainStorageAccessor<UserDocument> userReader,
            InterviewDataExportSettings interviewDataExportSettings, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.readSideStorage = readSideStorage;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }


        public void Handle(IPublishableEvent evt)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            var updateMethod = this
                .GetType()
                .GetMethod("Update", new[] {typeof (InterviewHistoryView), eventType});

            if (updateMethod==null)
                return;

            InterviewHistoryView currentState = this.readSideStorage.GetById(evt.EventSourceId);

            if (currentState == null)
            {
                var interviewSummary = this.interviewSummaryReader.GetById(evt.EventSourceId);
                if (interviewSummary != null)
                    currentState = new InterviewHistoryView(evt.EventSourceId, new List<InterviewHistoricalRecordView>(), 
                        interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
            }

            var newState = (InterviewHistoryView)updateMethod
                .Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });

            if (newState != null)
            {
                this.readSideStorage.Store(newState, evt.EventSourceId);
            }
            else
            {
                this.readSideStorage.Remove(evt.EventSourceId);
            }
        }
        private bool Handles(IUncommittedEvent evt)
        {
            Type genericUpgrader = typeof(IUpdateHandler<,>);
            return genericUpgrader.MakeGenericType(typeof(InterviewHistoryView), evt.Payload.GetType()).IsInstanceOfType(this.GetType());
        }

        private PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<SupervisorAssigned> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.SupervisorAssigned, @event.Payload.UserId, @event.EventTimeStamp);
            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApprovedByHQ> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.ApproveByHeadquarter, @event.Payload.UserId, @event.EventTimeStamp,
                this.CreateCommentParameters(@event.Payload.Comment));
            
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewerAssigned> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.InterviewerAssigned, @event.Payload.UserId, @event.Payload.AssignTime ?? @event.EventTimeStamp,
                new Dictionary<string, string> { { "responsible", @event.Payload.InterviewerId.FormatGuid() } });

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewCompleted> @event){

            this.AddHistoricalRecord(view, InterviewHistoricalAction.Completed, Guid.Empty, @event.Payload.CompleteTime??@event.EventTimeStamp, this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }
        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestarted> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.Restarted, Guid.Empty, @event.Payload.RestartTime ?? @event.EventTimeStamp, this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApproved> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.ApproveBySupervisor, @event.Payload.UserId, @event.Payload.ApproveTime ?? @event.EventTimeStamp,
              this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejected> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.RejectedBySupervisor, @event.Payload.UserId, @event.Payload.RejectTime ?? @event.EventTimeStamp,
              this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejectedByHQ> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.RejectedByHeadquarter, @event.Payload.UserId, @event.EventTimeStamp,
             this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewDeleted> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.Deleted, @event.Payload.UserId, @event.EventTimeStamp);

            return view;
        }


        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestored> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.Restored, @event.Payload.UserId, @event.EventTimeStamp);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
            this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer), @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedValues),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedValue),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
             this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                 @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
          this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(new GeoPosition(@event.Payload.Latitude, @event.Payload.Longitude, @event.Payload.Accuracy, @event.Payload.Altitude,
                        @event.Payload.Timestamp)),
              @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswerRemoved> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerRemoved, @event.Payload.UserId,
                @event.Payload.RemoveTimeUtc, this.CreateQuestionParameters(@event.Payload.QuestionId, @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
              this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedRosterVectors),
                  @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
             this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedRosterVector),
                 @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
           this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(new InterviewTextListAnswers(@event.Payload.Answers)),
               @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
            this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
            @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
           this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.PictureFileName),
           @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<YesNoQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.AnswerTimeUtc,
                this.CreateAnswerParameters(@event.Payload.QuestionId,
                    AnswerUtils.AnswerToString(@event.Payload.AnsweredOptions),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswerCommented> @event)
        {
            var parameters = new Dictionary<string, string>()
            {
                { "questionId", @event.Payload.QuestionId.FormatGuid() },
                { "comment", @event.Payload.Comment }
            };
            if (@event.Payload.RosterVector.Length > 0)
            {
                parameters.Add("roster", string.Join(",", @event.Payload.RosterVector));
            }

            this.AddHistoricalRecord(view, InterviewHistoricalAction.CommentSet, @event.Payload.UserId, @event.Payload.CommentTime, parameters);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }

        private Dictionary<string, string> CreateAnswerParameters(Guid questionId, string answer,
            decimal[] propagationVector)
        {
            var result = this.CreateQuestionParameters(questionId, propagationVector);
            result.Add("answer", answer);
            return result;
        }

        private InterviewHistoricalRecordView CreateInterviewHistoricalRecordView(InterviewHistoricalAction action, Guid? userId,
            DateTime? timestamp, Dictionary<string, string> parameters, QuestionnaireExportStructure questionnaire)
        {
            string userName = string.Empty;
            string userRole = string.Empty;
            if (userId.HasValue)
            {
                var user = this.GetUserDocument(userId.Value);
                userName = this.GetUserName(user);
                userRole = this.GetUserRole(user);
            }
            if (action == InterviewHistoricalAction.AnswerSet 
                || action == InterviewHistoricalAction.CommentSet
                || action == InterviewHistoricalAction.QuestionDeclaredInvalid
                || action == InterviewHistoricalAction.QuestionDeclaredValid
                || action == InterviewHistoricalAction.QuestionEnabled
                || action == InterviewHistoricalAction.QuestionDisabled)
            {
                var newParameters = new Dictionary<string, string>();
                if (parameters.ContainsKey("questionId"))
                {
                    var questionId = Guid.Parse(parameters["questionId"]);
                    var question = questionnaire.HeaderToLevelMap.SelectMany(h => h.Value.HeaderItems).FirstOrDefault(q => q.Key == questionId);
                    if (!question.Equals(new KeyValuePair<Guid, ExportedHeaderItem>()))
                    {
                        newParameters["question"] = question.Value.VariableName;
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
                }
                return new InterviewHistoricalRecordView(0, action, userName, userRole, newParameters, timestamp);

            }
            if (action == InterviewHistoricalAction.GroupDisabled || action == InterviewHistoricalAction.GroupEnabled)
            {
                var newParameters = new Dictionary<string, string>();
                var groupId = parameters["groupId"];
                newParameters["group"] = groupId;
                newParameters["roster"] = parameters["roster"];
                return new InterviewHistoricalRecordView(0, action, userName, userRole, newParameters, timestamp);
            }
            if (action == InterviewHistoricalAction.InterviewerAssigned)
            {
                if (parameters.ContainsKey("responsible"))
                {
                    var responsibleId = Guid.Parse(parameters["responsible"]);
                    return new InterviewHistoricalRecordView(0, action, userName, userRole,
                        new Dictionary<string, string> { { "responsible", this.GetUserName(this.GetUserDocument(responsibleId)) } },
                        timestamp);
                }
            }
            return new InterviewHistoricalRecordView(0, action, userName, userRole, parameters, timestamp);
        }

        private void ReduceCacheIfNeeded<TKey, TValue>(ConcurrentDictionary<TKey, TValue> cache)
        {
            if (cache.Count > this.interviewDataExportSettings.LimitOfCachedItemsByDenormalizer)
            {
                var cachedItemKeysToRemove = cache.Keys.Take(cache.Count - this.interviewDataExportSettings.LimitOfCachedItemsByDenormalizer).ToList();
                foreach (var cachedItemKeyToRemove in cachedItemKeysToRemove)
                {
                    TValue removedItem;
                    cache.TryRemove(cachedItemKeyToRemove, out removedItem);
                }
            }
        }

        private QuestionnaireExportStructure GetQuestionnaire(Guid questionnaireId, long questionnaireVersion)
        {
            var cachedQuestionnaireExportStructure =
                this.cacheQuestionnaireExportStructure.GetOrAdd(
                    new QuestionnaireIdentity(questionnaireId, questionnaireVersion),
                    (key) => this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireId, questionnaireVersion)));

            this.ReduceCacheIfNeeded(this.cacheQuestionnaireExportStructure);
            return cachedQuestionnaireExportStructure;
        }

        private UserDocument GetUserDocument(Guid originatorId)
        {
            var cachedUserDocument = this.cacheUserDocument.GetOrAdd(originatorId.FormatGuid(),
                (key) => this.userReader.GetById(key));

            this.ReduceCacheIfNeeded(this.cacheUserDocument);

            return cachedUserDocument;
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

        private void AddHistoricalRecord(InterviewHistoryView view, InterviewHistoricalAction action, Guid? userId, DateTime? timestamp,
            Dictionary<string, string> parameters = null)
        {
            if(view ==null)
                return;

            var questionnaire = this.GetQuestionnaire(view.QuestionnaireId, view.QuestionnaireVersion);
            if (questionnaire == null)
                return;

            var record = this.CreateInterviewHistoricalRecordView(action, userId, timestamp, parameters ?? new Dictionary<string, string>(),
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

        private Dictionary<string, string> CreateQuestionParameters(Guid questionId, decimal[] propagationVector)
        {
            var result = new Dictionary<string, string>()
            {
                { "questionId", questionId.FormatGuid() }
            };

            if (propagationVector.Length > 0)
            {
                result.Add("roster", string.Join(",", propagationVector));
            }
            else
            {
                result.Add("roster", string.Empty);
            }
            return result;
        }

        private Dictionary<string, string> CreateGroupParameters(Guid groupId, decimal[] propagationVector)
        {
            var result = new Dictionary<string, string>()
            {
                { "groupId", groupId.FormatGuid() }
            };

            if (propagationVector.Length > 0)
            {
                result.Add("roster", string.Join(",", propagationVector));
            }
            else
            {
                result.Add("roster", string.Empty);
            }
            return result;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswersDeclaredInvalid> @event)
        {
            foreach (var question in @event.Payload.FailedValidationConditions.Keys)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDeclaredInvalid, null, null,
                this.CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswersDeclaredValid> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDeclaredValid, null, null,
                this.CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QuestionsDisabled> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDisabled, null, null,
                this.CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QuestionsEnabled> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.QuestionEnabled, null, null,
                this.CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GroupsDisabled> @event)
        {
            foreach (var group in @event.Payload.Groups)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.GroupDisabled, null, null,
                this.CreateGroupParameters(group.Id, group.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GroupsEnabled> @event)
        {
            foreach (var group in @event.Payload.Groups)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.GroupEnabled, null, null,
                this.CreateGroupParameters(group.Id, group.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<UnapprovedByHeadquarters> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.UnapproveByHeadquarters, @event.Payload.UserId, @event.EventTimeStamp,
                this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewReceivedByInterviewer> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.ReceivedByInterviewer, null, @event.EventTimeStamp);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewReceivedBySupervisor> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.ReceivedBySupervisor, null, @event.EventTimeStamp);

            return state;
        }
    }
}

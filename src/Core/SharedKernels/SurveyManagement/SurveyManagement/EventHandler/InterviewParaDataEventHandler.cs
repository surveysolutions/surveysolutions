using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
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
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureRepository;
        private readonly IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage;
        private readonly ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireExportStructure> cacheQuestionnaireExportStructure = new ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireExportStructure>();
        private readonly ConcurrentDictionary<string, UserDocument> cacheUserDocument = new ConcurrentDictionary<string, UserDocument>();

        private readonly InterviewDataExportSettings interviewDataExportSettings;
        public InterviewParaDataEventHandler(
            IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader, 
            IReadSideRepositoryWriter<UserDocument> userReader,
            InterviewDataExportSettings interviewDataExportSettings,
            IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureRepository)
        {
            this.readSideStorage = readSideStorage;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireExportStructureRepository = questionnaireExportStructureRepository;
        }


        public void Handle(IPublishableEvent evt)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            var updateMethod = this
                .GetType()
                .GetMethod("Update", new[] {typeof (InterviewHistoryView), eventType});

            if (updateMethod==null)
                return;

            InterviewHistoryView currentState = readSideStorage.GetById(evt.EventSourceId);

            var newState = (InterviewHistoryView)updateMethod
                .Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });

            if (newState != null)
            {
                readSideStorage.Store(newState, evt.EventSourceId);
            }
            else
            {
                readSideStorage.Remove(evt.EventSourceId);
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
            var interviewSummary = interviewSummaryReader.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return null;

            var view = new InterviewHistoryView(@event.EventSourceId, new List<InterviewHistoricalRecordView>(), interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            AddHistoricalRecord(view, InterviewHistoricalAction.SupervisorAssigned, @event.Payload.UserId, @event.EventTimeStamp);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApprovedByHQ> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.ApproveByHeadquarter, @event.Payload.UserId, @event.EventTimeStamp,
                CreateCommentParameters(@event.Payload.Comment));
            
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewerAssigned> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.InterviewerAssigned, @event.Payload.UserId, @event.Payload.AssignTime ?? @event.EventTimeStamp,
                new Dictionary<string, string> { { "responsible", @event.Payload.InterviewerId.FormatGuid() } });

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewCompleted> @event){

            AddHistoricalRecord(view, InterviewHistoricalAction.Completed, Guid.Empty, @event.Payload.CompleteTime??@event.EventTimeStamp, CreateCommentParameters(@event.Payload.Comment));

            return view;
        }
        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestarted> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.Restarted, Guid.Empty, @event.Payload.RestartTime ?? @event.EventTimeStamp, CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApproved> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.ApproveBySupervisor, @event.Payload.UserId, @event.Payload.ApproveTime ?? @event.EventTimeStamp,
              CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejected> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.RejectedBySupervisor, @event.Payload.UserId, @event.Payload.RejectTime ?? @event.EventTimeStamp,
              CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejectedByHQ> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.RejectedByHeadquarter, @event.Payload.UserId, @event.EventTimeStamp,
             CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewDeleted> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.Deleted, @event.Payload.UserId, @event.EventTimeStamp);

            return view;
        }


        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestored> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.Restored, @event.Payload.UserId, @event.EventTimeStamp);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
            CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer), @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedValues),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedValue),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
             CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                 @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
                CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
          CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(new GeoPosition(@event.Payload.Latitude, @event.Payload.Longitude, @event.Payload.Accuracy, @event.Payload.Altitude,
                        @event.Payload.Timestamp)),
              @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswerRemoved> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerRemoved, @event.Payload.UserId,
                @event.Payload.RemoveTimeUtc, CreateQuestionParameters(@event.Payload.QuestionId, @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
              CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedRosterVectors),
                  @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
             CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedRosterVector),
                 @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
           CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(new InterviewTextListAnswers(@event.Payload.Answers)),
               @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
            CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
            @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, @event.Payload.AnswerTimeUtc,
           CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.PictureFileName),
           @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<YesNoQuestionAnswered> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.AnswerTimeUtc,
                CreateAnswerParameters(@event.Payload.QuestionId,
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

            AddHistoricalRecord(view, InterviewHistoricalAction.CommentSet, @event.Payload.UserId, @event.Payload.CommentTime, parameters);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }

        private Dictionary<string, string> CreateAnswerParameters(Guid questionId, string answer,
            decimal[] propagationVector)
        {
            var result = CreateQuestionParameters(questionId, propagationVector);
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
                        new Dictionary<string, string> { { "responsible", this.GetUserName(GetUserDocument(responsibleId)) } },
                        timestamp);
                }
            }
            return new InterviewHistoricalRecordView(0, action, userName, userRole, parameters, timestamp);
        }

        private void ReduceCacheIfNeeded<TKey, TValue>(ConcurrentDictionary<TKey, TValue> cache)
        {
            if (cache.Count > interviewDataExportSettings.LimitOfCachedItemsByDenormalizer)
            {
                var cachedItemKeysToRemove = cache.Keys.Take(cache.Count - interviewDataExportSettings.LimitOfCachedItemsByDenormalizer).ToList();
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
                    (key) => this.questionnaireExportStructureRepository.GetById(new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString()));

            ReduceCacheIfNeeded(cacheQuestionnaireExportStructure);
            return cachedQuestionnaireExportStructure;
        }

        private UserDocument GetUserDocument(Guid originatorId)
        {
            var cachedUserDocument = this.cacheUserDocument.GetOrAdd(originatorId.FormatGuid(),
                (key) => userReader.GetById(key));

            ReduceCacheIfNeeded(cacheUserDocument);

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

            var questionnaire = GetQuestionnaire(view.QuestionnaireId, view.QuestionnaireVersion);
            if (questionnaire == null)
                return;

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
                AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDeclaredInvalid, null, null,
                CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswersDeclaredValid> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDeclaredValid, null, null,
                CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QuestionsDisabled> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDisabled, null, null,
                CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QuestionsEnabled> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                AddHistoricalRecord(view, InterviewHistoricalAction.QuestionEnabled, null, null,
                CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GroupsDisabled> @event)
        {
            foreach (var group in @event.Payload.Groups)
            {
                AddHistoricalRecord(view, InterviewHistoricalAction.GroupDisabled, null, null,
                CreateGroupParameters(group.Id, group.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GroupsEnabled> @event)
        {
            foreach (var group in @event.Payload.Groups)
            {
                AddHistoricalRecord(view, InterviewHistoricalAction.GroupEnabled, null, null,
                CreateGroupParameters(group.Id, group.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<UnapprovedByHeadquarters> @event)
        {
            AddHistoricalRecord(view, InterviewHistoricalAction.UnapproveByHeadquarters, @event.Payload.UserId, @event.EventTimeStamp,
                CreateCommentParameters(@event.Payload.Comment));

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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

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
        IUpdateHandler<InterviewHistoryView, AnswerRemoved>,
        IUpdateHandler<InterviewHistoryView, AnswersRemoved>,
        IUpdateHandler<InterviewHistoryView, UnapprovedByHeadquarters>,
        IUpdateHandler<InterviewHistoryView, InterviewReceivedByInterviewer>,
        IUpdateHandler<InterviewHistoryView, InterviewReceivedBySupervisor>,
        IUpdateHandler<InterviewHistoryView, AreaQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, AudioQuestionAnswered>,
        IUpdateHandler<InterviewHistoryView, VariablesChanged>,
        IUpdateHandler<InterviewHistoryView, VariablesEnabled>,
        IUpdateHandler<InterviewHistoryView, VariablesDisabled>,
        IUpdateHandler<InterviewHistoryView, InterviewKeyAssigned>,
        IUpdateHandler<InterviewHistoryView, InterviewPaused>,
        IUpdateHandler<InterviewHistoryView, InterviewResumed>,
        IUpdateHandler<InterviewHistoryView, InterviewOpenedBySupervisor>,
        IUpdateHandler<InterviewHistoryView, InterviewClosedBySupervisor>,
        IUpdateHandler<InterviewHistoryView, TranslationSwitched>
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IUserViewFactory userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage;
        private readonly ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireExportStructure> cacheQuestionnaireExportStructure = new ConcurrentDictionary<QuestionnaireIdentity, QuestionnaireExportStructure>();
        private readonly ConcurrentDictionary<Guid, UserView> cacheUserDocument = new ConcurrentDictionary<Guid, UserView>();

        private readonly InterviewDataExportSettings interviewDataExportSettings;
        public InterviewParaDataEventHandler(
            IReadSideRepositoryWriter<InterviewHistoryView> readSideStorage,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IUserViewFactory userReader,
            InterviewDataExportSettings interviewDataExportSettings, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.readSideStorage = readSideStorage;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        static readonly ConcurrentDictionary<Type, MethodInfo> methodsCache = new ConcurrentDictionary<Type, MethodInfo>();
        
        public void Handle(IPublishableEvent evt)
        {
            var payloadType = evt.Payload.GetType();

            var updateMethod = methodsCache.GetOrAdd(payloadType, t =>
            {
                var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

                return this
                    .GetType()
                    .GetMethod("Update", new[] { typeof(InterviewHistoryView), eventType });
            });
            
            if (updateMethod == null)
                return;

            InterviewHistoryView currentState = this.readSideStorage.GetById(evt.EventSourceId);

            if (currentState == null)
            {
                var interviewSummary = this.interviewSummaryReader.GetById(evt.EventSourceId);
                if (interviewSummary != null)
                    currentState = new InterviewHistoryView(evt.EventSourceId, new List<InterviewHistoricalRecordView>(), 
                        interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
                else //interview was deleted
                {
                    return;
                }
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
            this.AddHistoricalRecord(state, InterviewHistoricalAction.SupervisorAssigned, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);
            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApprovedByHQ> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.ApproveByHeadquarter, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                this.CreateCommentParameters(@event.Payload.Comment));
            
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewerAssigned> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.InterviewerAssigned, @event.Payload.UserId, 
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AssignTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                new Dictionary<string, string> { { "responsible", @event.Payload.InterviewerId.FormatGuid() } });

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewCompleted> @event){

            this.AddHistoricalRecord(view, InterviewHistoricalAction.Completed, Guid.Empty,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.CompleteTime??@event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }
        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestarted> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.Restarted, Guid.Empty,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.RestartTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewApproved> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.ApproveBySupervisor, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.ApproveTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
              this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejected> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.RejectedBySupervisor, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.RejectTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
              this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRejectedByHQ> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.RejectedByHeadquarter, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
             this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewDeleted> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.Deleted, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return view;
        }


        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<InterviewRestored> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.Restored, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
            this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer), @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedValues),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedValue),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, 
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
             this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
                 @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(view.QuestionnaireId, view.QuestionnaireVersion);
            var question = questionnaire.Find<DateTimeQuestion>(@event.Payload.QuestionId);

            DateTime answer = @event.Payload.Answer;
            if (question.IsTimestamp)
            {
                if (@event.Payload.OriginDate.HasValue)
                {
                    answer = @event.Payload.OriginDate.Value.DateTime;
                }
            }
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
                this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(answer, isTimestamp: question.IsTimestamp),
                    @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
          this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(new GeoPosition(@event.Payload.Latitude, @event.Payload.Longitude, @event.Payload.Accuracy, @event.Payload.Altitude,
                        @event.Payload.Timestamp)),
              @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswerRemoved> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerRemoved, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.RemoveTimeUtc,
                @event.Payload.OriginDate?.Offset,
                this.CreateQuestionParameters(@event.Payload.QuestionId, @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswersRemoved> @event)
        {
           @event.Payload.Questions.ForEach(e=>  this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerRemoved, null,
               @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.RemoveTime ?? @event.EventTimeStamp,
               @event.Payload.OriginDate?.Offset,
               this.CreateQuestionParameters(e.Id, e.RosterVector)));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
              this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedRosterVectors),
                  @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
             this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.SelectedRosterVector),
                 @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
           this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(new InterviewTextListAnswers(@event.Payload.Answers)),
               @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId, 
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
            this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.Answer),
            @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
           this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(@event.Payload.PictureFileName),
           @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<YesNoQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
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

            this.AddHistoricalRecord(view, InterviewHistoricalAction.CommentSet, @event.Payload.UserId, 
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.CommentTime,
                @event.Payload.OriginDate?.Offset,
                parameters);

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

        private InterviewHistoricalRecordView CreateInterviewHistoricalRecordView(InterviewHistoricalAction action, 
            Guid? userId,
            DateTime? timestamp, 
            TimeSpan? offset, 
            Dictionary<string, string> parameters, 
            QuestionnaireExportStructure questionnaire)
        {
            string userName = string.Empty;
            string userRole = string.Empty;
            if (userId.HasValue)
            {
                var user = this.GetUserDocument(userId.Value);
                userName = this.GetUserName(user);
                userRole = this.GetUserRole(user);
            }
            switch (action)
            {
                case InterviewHistoricalAction.QuestionEnabled:
                case InterviewHistoricalAction.QuestionDisabled:
                case InterviewHistoricalAction.GroupDisabled:
                case InterviewHistoricalAction.GroupEnabled:
                {
                        // ignore
                        return null;
                }
                case InterviewHistoricalAction.AnswerSet:
                case InterviewHistoricalAction.CommentSet:
                case InterviewHistoricalAction.QuestionDeclaredInvalid:
                case InterviewHistoricalAction.QuestionDeclaredValid:
                {
                    var newParameters = new Dictionary<string, string>();
                    if (parameters.ContainsKey("questionId"))
                    {
                        var questionId = Guid.Parse(parameters["questionId"]);
                        var question = questionnaire.GetExportedQuestionHeaderItemForQuestion(questionId);

                        if (question != null)
                        {
                            newParameters["question"] = question.VariableName;
                            switch (action)
                            {
                                case InterviewHistoricalAction.CommentSet:
                                    newParameters["comment"] = parameters["comment"];
                                    break;
                                case InterviewHistoricalAction.AnswerSet:
                                    newParameters["answer"] = parameters["answer"];
                                    break;
                            }

                            if (parameters.TryGetValue("roster", out var roster))
                            {
                                newParameters["roster"] = roster;
                            }
                        }
                    }
                    return new InterviewHistoricalRecordView(0, action, userName, userRole, newParameters, timestamp, offset);
                }
                case InterviewHistoricalAction.InterviewerAssigned 
                    when parameters.ContainsKey("responsible") && parameters["responsible"] != null:
                {
                    Guid responsibleId = Guid.Parse(parameters["responsible"]);
                    return new InterviewHistoricalRecordView(0, action, userName, userRole,
                        new Dictionary<string, string> {
                        {
                            "responsible", this.GetUserName(this.GetUserDocument(responsibleId))
                        } },
                        timestamp, offset);
                }
                case InterviewHistoricalAction.VariableSet:
                case InterviewHistoricalAction.VariableEnabled:
                case InterviewHistoricalAction.VariableDisabled:
                {
                    var newParameters = new Dictionary<string, string>();
                    if (parameters.ContainsKey("variableId"))
                    {
                        var variableId = Guid.Parse(parameters["variableId"]);
                        var variable = questionnaire.GetExportedQuestionHeaderItemForQuestion(variableId);
                     
                        if (variable == null)
                            return null;

                        newParameters["variable"] = variable.VariableName;
                        if (action == InterviewHistoricalAction.VariableSet)
                        {
                            newParameters["value"] = parameters["value"];
                        }

                        if (parameters.TryGetValue("roster", out var roster))
                        {
                            newParameters["roster"] = roster;
                        }
                        }
                    return new InterviewHistoricalRecordView(0, action, userName, userRole, newParameters, timestamp, offset);
                }
                default:
                    return new InterviewHistoricalRecordView(0, action, userName, userRole, parameters, timestamp, offset);
            }
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

        private UserView GetUserDocument(Guid originatorId)
        {
            var cachedUserDocument = this.cacheUserDocument.GetOrAdd(originatorId, key => this.userReader.GetUser(new UserViewInputModel(key)));

            this.ReduceCacheIfNeeded(this.cacheUserDocument);

            return cachedUserDocument;
        }

        private string GetUserName(UserView responsible)
        {
            var userName = responsible != null ? responsible.UserName : "";
            return userName;
        }

        private string GetUserRole(UserView user)
        {
            const string UnknownUserRole = "";
            if (user == null || !user.Roles.Any())
                return UnknownUserRole;
            var firstRole = user.Roles.First();
            switch (firstRole)
            {
                case UserRoles.Interviewer: return "Interviewer";
                case UserRoles.Supervisor: return "Supervisor";
                case UserRoles.Headquarter: return "Headquarter";
            }
            return UnknownUserRole;
        }

        private void AddHistoricalRecord(InterviewHistoryView view, InterviewHistoricalAction action, Guid? userId, DateTime? timestamp, TimeSpan? offset,
            Dictionary<string, string> parameters = null)
        {
            if(view ==null)
                return;

            var questionnaire = this.GetQuestionnaire(view.QuestionnaireId, view.QuestionnaireVersion);
            if (questionnaire == null)
                return;

            var record = this.CreateInterviewHistoricalRecordView(action, userId, timestamp, offset, parameters ?? new Dictionary<string, string>(),
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
                this.AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDeclaredInvalid, null, 
                    @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                    @event.Payload.OriginDate?.Offset,
                this.CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AnswersDeclaredValid> @event)
        {
            foreach (var question in @event.Payload.Questions)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.QuestionDeclaredValid, null,
                    @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp, 
                    @event.Payload.OriginDate?.Offset,
                this.CreateQuestionParameters(question.Id, question.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<UnapprovedByHeadquarters> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.UnapproveByHeadquarters, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                this.CreateCommentParameters(@event.Payload.Comment));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewReceivedByInterviewer> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.ReceivedByInterviewer, null,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewReceivedBySupervisor> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.ReceivedBySupervisor, null, 
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
            this.CreateAnswerParameters(@event.Payload.QuestionId, AnswerUtils.AnswerToString(
                new Area(@event.Payload.Geometry, @event.Payload.MapName,@event.Payload.NumberOfPoints, 
                    @event.Payload.AreaSize, @event.Payload.Length, @event.Payload.Coordinates, @event.Payload.DistanceToEditor)),
            @event.Payload.RosterVector));

            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            this.AddHistoricalRecord(view, InterviewHistoricalAction.AnswerSet, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.OriginDate?.Offset,
            this.CreateAnswerParameters(@event.Payload.QuestionId, $"{@event.Payload.FileName}, {@event.Payload.Length}", @event.Payload.RosterVector));
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<VariablesChanged> @event)
        {
            foreach (var variable in @event.Payload.ChangedVariables)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.VariableSet, null,
                    @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                    @event.Payload.OriginDate?.Offset,
                    this.CreateNewVariableValueParameters(variable.Identity.Id, variable.NewValue, variable.Identity.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<VariablesEnabled> @event)
        {
            foreach (var variable in @event.Payload.Variables)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.VariableEnabled, null,
                    @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                    @event.Payload.OriginDate?.Offset,
                    this.CreateVariableParameters(variable.Id, variable.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView view, IPublishedEvent<VariablesDisabled> @event)
        {
            foreach (var variable in @event.Payload.Variables)
            {
                this.AddHistoricalRecord(view, InterviewHistoricalAction.VariableDisabled, null,
                    @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                    @event.Payload.OriginDate?.Offset,
                    this.CreateVariableParameters(variable.Id, variable.RosterVector));
            }
            return view;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewKeyAssigned> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.KeyAssigned, null,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                new Dictionary<string, string>
                {
                    {"Key", @event.Payload.Key.ToString()}
                });

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewPaused> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.Paused, null,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewResumed> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.Resumed, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewOpenedBySupervisor> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.OpenedBySupervisor, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<InterviewClosedBySupervisor> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.ClosedBySupervisor, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset);

            return state;
        }

        public InterviewHistoryView Update(InterviewHistoryView state, IPublishedEvent<TranslationSwitched> @event)
        {
            this.AddHistoricalRecord(state, InterviewHistoricalAction.TranslationSwitched, @event.Payload.UserId,
                @event.Payload.OriginDate?.LocalDateTime ?? @event.EventTimeStamp,
                @event.Payload.OriginDate?.Offset,
                new Dictionary<string, string>
                {
                    { "translation", @event.Payload.Language ?? "ORIGINAL" }
                });

            return state;
        }

        private Dictionary<string, string> CreateVariableParameters(Guid variableId, decimal[] propagationVector)
        {
            var result = new Dictionary<string, string>()
            {
                { "variableId", variableId.FormatGuid() }
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

        private Dictionary<string, string> CreateNewVariableValueParameters(Guid variableId, object newValue,
            decimal[] propagationVector)
        {
            var result = this.CreateVariableParameters(variableId, propagationVector);
            result.Add("value", newValue?.ToString() ?? string.Empty);
            return result;
        }
    }
}

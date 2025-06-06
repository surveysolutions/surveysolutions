﻿using System;
using System.Linq;
using AngleSharp.Dom.Events;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Caching.Memory;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class InterviewSummaryDenormalizer :
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, InterviewCreated>,
        IUpdateHandler<InterviewSummary, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewSummary, InterviewOnClientCreated>,
        IUpdateHandler<InterviewSummary, InterviewStatusChanged>,
        IUpdateHandler<InterviewSummary, InterviewModeChanged>,
        IUpdateHandler<InterviewSummary, SupervisorAssigned>,
        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,
        IUpdateHandler<InterviewSummary, InterviewerAssigned>,
        IUpdateHandler<InterviewSummary, SynchronizationMetadataApplied>,
        IUpdateHandler<InterviewSummary, InterviewHardDeleted>,
        IUpdateHandler<InterviewSummary, InterviewKeyAssigned>,
        IUpdateHandler<InterviewSummary, InterviewReceivedByInterviewer>,
        IUpdateHandler<InterviewSummary, InterviewReceivedBySupervisor>,
        IUpdateHandler<InterviewSummary, AreaQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AudioQuestionAnswered>,
        IUpdateHandler<InterviewSummary, InterviewPaused>,
        IUpdateHandler<InterviewSummary, InterviewResumed>,
        IUpdateHandler<InterviewSummary, InterviewRestored>,
        IUpdateHandler<InterviewSummary, AnswerCommentResolved>,
        IUpdateHandler<InterviewSummary, AnswerCommented>,
        IUpdateHandler<InterviewSummary, SubstitutionTitlesChanged>,
        IUpdateHandler<InterviewSummary, VariablesChanged>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IMemoryCache memoryCache;
        private readonly IUserViewFactory users;

        public InterviewSummaryDenormalizer(
            IUserViewFactory users,
            IQuestionnaireStorage questionnaireStorage,
            IMemoryCache memoryCache)
        {
            this.users = users;
            this.questionnaireStorage = questionnaireStorage;
            this.memoryCache = memoryCache;
        }

        private InterviewSummary UpdateInterviewSummary(InterviewSummary interviewSummary, DateTime updateDateTime, Action<InterviewSummary> update)
        {
            update(interviewSummary);
            interviewSummary.UpdateDate = updateDateTime;
            return interviewSummary;
        }

        private void RecordFirstAnswer(InterviewSummary interviewSummary, DateTime answerTime)
        {
            if (interviewSummary.FirstAnswerDate.HasValue)
                return;

            if (!interviewSummary.InterviewCommentedStatuses.Any())
                return;

            // skip first answer date while interview status is zero.
            if (interviewSummary.Status == InterviewStatus.Restored) return;

            interviewSummary.FirstAnswerDate = answerTime;
        }
       
        private InterviewSummary AnswerQuestion(InterviewSummary interviewSummary, Guid questionId, object answer, DateTime updateDate, DateTime answerDate)
        {
            var questionnaire = GetQuestionnaire(interviewSummary);

            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                var questionCompositeId = questionnaire.GetEntityIdMapValue(questionId);
                if (interview.IsEntityIdentifying(questionCompositeId))
                {
                    interview.AnswerFeaturedQuestion(questionCompositeId, AnswerUtils.AnswerToString(answer));
                }
                
                RecordFirstAnswer(interview, answerDate);
            });
        }

        private InterviewSummary AnswerDateTimeQuestion(InterviewSummary interviewSummary, Guid questionId, DateTime answer, DateTime updateDate, DateTime answerDate)
        {
            var questionnaire = GetQuestionnaire(interviewSummary);
            var answerString = questionnaire.IsTimestampQuestion(questionId)
                    ? answer.ToString(DateTimeFormat.DateWithTimeFormat)
                    : answer.ToString(DateTimeFormat.DateFormat);
            
            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                var questionCompositeId = questionnaire.GetEntityIdMapValue(questionId);
                if (interview.IsEntityIdentifying(questionCompositeId))
                {
                    interview.AnswerDateTimeFeaturedQuestion(questionCompositeId, answer, answerString);
                }
                
                RecordFirstAnswer(interview, answerDate);
            });
        }

        private InterviewSummary AnswerIntegerQuestion(InterviewSummary interviewSummary, Guid questionId, int answer, DateTime updateDate, DateTime answerDate)
        {
            var questionnaire = GetQuestionnaire(interviewSummary);

            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                var questionCompositeId = questionnaire.GetEntityIdMapValue(questionId);
                if (interview.IsEntityIdentifying(questionCompositeId))
                {
                    interview.AnswerIntegerFeaturedQuestion(questionCompositeId, answer);
                }
                
                RecordFirstAnswer(interview, answerDate);
            });
        }

        private InterviewSummary AnswerRealQuestion(InterviewSummary interviewSummary, Guid questionId, decimal answer, DateTime updateDate, DateTime answerDate)
        {
            var questionnaire = GetQuestionnaire(interviewSummary);

            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                var questionCompositeId = questionnaire.GetEntityIdMapValue(questionId);
                if (interview.IsEntityIdentifying(questionCompositeId))
                {
                    interview.AnswerRealFeaturedQuestion(questionCompositeId, answer);
                }
                
                RecordFirstAnswer(interview, answerDate);
            });
        }

        private InterviewSummary AnswerFeaturedQuestionWithOptions(InterviewSummary interviewSummary, Guid questionId, 
            DateTime updateDate, DateTime? answerDateTime,
            params decimal[] answers)
        {
            var questionnaire = this.GetQuestionnaire(interviewSummary);

            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                var questionCompositeId = questionnaire.GetEntityIdMapValue(questionId);
                if (interview.IsEntityIdentifying(questionCompositeId))
                {
                    var isCascading = questionnaire.IsQuestionCascading(questionId);
                    int? parentValue = null;
                    if (isCascading)
                    {
                        var parentId = questionnaire.GetCascadingQuestionParentId(questionId);
                        if (parentId.HasValue)
                        {
                            var parentIntId = questionnaire.GetEntityIdMapValue(parentId.Value);
                            var parentEntity = interview.IdentifyEntitiesValues.FirstOrDefault(e => e.Entity.Id == parentIntId && e.Identifying == true);
                            if (parentEntity != null && parentEntity.AnswerCode.HasValue)
                            {
                                parentValue = Convert.ToInt32(parentEntity.AnswerCode.Value);
                            }
                        }
                    }
                    
                    var optionStrings = questionnaire.GetOptionsForQuestion(questionId, parentValue, null, null)
                        .Where(x => answers.Contains(x.Value)).Select(x => x.Title);

                    interview.AnswerFeaturedQuestion(questionCompositeId, string.Join(",", optionStrings), answers.First());
                }

                RecordFirstAnswer(interviewSummary, answerDateTime ?? updateDate);
            });
        }

        private InterviewSummary CreateInterviewSummary(Guid userId,
            Guid questionnaireId,
            long questionnaireVersion,
            Guid eventSourceId,
            DateTime eventTimeStamp,
            bool wasCreatedOnClient,
            int? assignmentId,
            DateTime? creationTime)
        {
            var responsible = this.users.GetUser(userId);
            var questionnaire = GetQuestionnaire(questionnaireId, questionnaireVersion);

            var interviewSummary = new InterviewSummary(questionnaire)
            {
                InterviewId = eventSourceId,
                CreatedDate = creationTime ?? eventTimeStamp,
                WasCreatedOnClient = wasCreatedOnClient,
                UpdateDate = eventTimeStamp,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString(),
                QuestionnaireTitle = questionnaire.Title,
                QuestionnaireVariable = questionnaire.VariableName ?? string.Empty,
                ResponsibleId = userId, // Creator is responsible
                ResponsibleName = responsible != null ? responsible.UserName : "<UNKNOWN USER>",
                ResponsibleRole = responsible?.Roles.First() ?? UserRoles.Interviewer,
                AssignmentId = assignmentId,
                LastResumeEventUtcTimestamp = creationTime
            };

            memoryCache.SetQuestionnaireIdentity(eventSourceId, new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

            CommonMetrics.InterviewsCreatedCount.Inc();

            return interviewSummary;
        }

        private IQuestionnaire GetQuestionnaire(InterviewSummary interview) =>
            GetQuestionnaire(interview.QuestionnaireId, interview.QuestionnaireVersion);

        private IQuestionnaire GetQuestionnaire(Guid questionnaireId, long version) =>
            this.questionnaireStorage.GetQuestionnaire(new QuestionnaireIdentity(questionnaireId, version), null);


        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCreated> @event)
        {
            return this.CreateInterviewSummary(@event.Payload.UserId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion, @event.EventSourceId, @event.EventTimeStamp, false, @event.Payload.AssignmentId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.CreationTime);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            return this.CreateInterviewSummary(@event.Payload.UserId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion, @event.EventSourceId, @event.EventTimeStamp, false, @event.Payload.AssignmentId, @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            return this.CreateInterviewSummary(@event.Payload.UserId, @event.Payload.QuestionnaireId,
             @event.Payload.QuestionnaireVersion, @event.EventSourceId, @event.EventTimeStamp, wasCreatedOnClient: true, assignmentId: @event.Payload.AssignmentId, creationTime: null);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewStatusChanged> @event)
        {
            if (@event.Payload.Status == InterviewStatus.Deleted)
            {
                return null;
            }

            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.Status = @event.Payload.Status;
                interview.WasRejectedBySupervisor = interview.WasRejectedBySupervisor || @event.Payload.Status == InterviewStatus.RejectedBySupervisor;

                if (interview.Status == @event.Payload.Status)
                {
                    interview.LastStatusChangeComment = @event.Payload.Comment;
                }

                if (!state.WasCompleted && @event.Payload.Status == InterviewStatus.Completed)
                {
                    state.WasCompleted = true;
                }
				
				if (@event.Payload.Status == InterviewStatus.ApprovedBySupervisor 
                    || @event.Payload.Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    interview.ReceivedByInterviewerAtUtc = null;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SupervisorAssigned> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                var user = this.users.GetUser(@event.Payload.SupervisorId);
                var supervisorName = user != null ? user.UserName : "<UNKNOWN SUPERVISOR>";

                interview.ResponsibleId = @event.Payload.SupervisorId;
                interview.ResponsibleName = supervisorName;
                interview.ResponsibleRole = UserRoles.Supervisor;
                interview.SupervisorId = @event.Payload.SupervisorId;
                interview.SupervisorName = supervisorName;
                interview.IsAssignedToInterviewer = false;
                interview.ReceivedByInterviewerAtUtc = null;

                if (interview.FirstSupervisorId == null)
                {
                    interview.FirstSupervisorId = interview.SupervisorId;
                }

                if (interview.FirstSupervisorName == null)
                {
                    interview.FirstSupervisorName = interview.SupervisorName;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.AnswerFeaturedQuestionWithOptions(state, @event.Payload.QuestionId, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc, @event.Payload.SelectedValues);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.AnswerFeaturedQuestionWithOptions(state, @event.Payload.QuestionId, @event.EventTimeStamp,
                @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc,
                @event.Payload.SelectedValue);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.AnswerRealQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.AnswerIntegerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.AnswerDateTimeQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            var answerByGeoQuestion = new GeoPosition(@event.Payload.Latitude, @event.Payload.Longitude, @event.Payload.Accuracy, @event.Payload.Altitude, @event.Payload.Timestamp);
            return this.AnswerQuestion(state, @event.Payload.QuestionId, answerByGeoQuestion, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var questionnaire = GetQuestionnaire(state);
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                foreach (var question in @event.Payload.Questions)
                {
                    var questionId = questionnaire.GetEntityIdMapValue(question.Id);
                    if (interview.IdentifyEntitiesValues.Any(x => x.Entity.Id == questionId && x.Identifying == true))
                    {
                        interview.RemoveAnswerFeaturedQuestion(questionId);
                    }
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewerAssigned> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                if (@event.Payload.InterviewerId.HasValue)
                {
                    var interviewerName = this.GetResponsibleIdName(@event.Payload.InterviewerId.Value);

                    interview.ResponsibleId = @event.Payload.InterviewerId.Value;
                    interview.ResponsibleName = interviewerName;
                    interview.ResponsibleRole = UserRoles.Interviewer;
                    interview.IsAssignedToInterviewer = true;

                    interview.ReceivedByInterviewerAtUtc = null;

                    if (interview.FirstInterviewerId == null)
                    {
                        interview.FirstInterviewerId = interview.ResponsibleId;
                    }

                    if (interview.FirstInterviewerName == null)
                    {
                        interview.FirstInterviewerName = interview.ResponsibleName;
                    }
                }
                else
                {
                    interview.ResponsibleId = interview.SupervisorId;
                    interview.ResponsibleName = interview.SupervisorName;
                    interview.ResponsibleRole = UserRoles.Supervisor;
                    interview.IsAssignedToInterviewer = false;
                    interview.ReceivedByInterviewerAtUtc = null;

                    if (interview.FirstSupervisorId == null)
                    {
                        interview.FirstSupervisorId = interview.ResponsibleId;
                    }

                    if (interview.FirstSupervisorName == null)
                    {
                        interview.FirstSupervisorName = interview.ResponsibleName;
                    }
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewKeyAssigned> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                if (interview.ClientKey == null)
                {
                    interview.ClientKey = @event.Payload.Key.ToString();
                }

                interview.Key = @event.Payload.Key.ToString();
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SynchronizationMetadataApplied> @event)
        {
            var questionnaire = GetQuestionnaire(state);
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                if (state.WasCreatedOnClient)
                {
                    if (@event.Payload.FeaturedQuestionsMeta != null)
                    {
                        foreach (var questionFromDto in @event.Payload.FeaturedQuestionsMeta)
                        {
                            var questionId = questionnaire.GetEntityIdMapValue(questionFromDto.Id);
                            if (interview.IdentifyEntitiesValues.Any(x => x.Entity.Id == questionId && x.Identifying == true))
                            {
                                var questionnaire = GetQuestionnaire(interview);
                                var questionType = questionnaire.GetQuestionType(questionFromDto.Id);
                                if (questionType == QuestionType.SingleOption)
                                {
                                    decimal[] answer = { Convert.ToDecimal(questionFromDto.Answer) };
                                    AnswerFeaturedQuestionWithOptions(interview, 
                                        questionFromDto.Id, @event.EventTimeStamp, @event.EventTimeStamp, answer);
                                }
                                else
                                {
                                    interview.AnswerFeaturedQuestion(questionId,
                                        AnswerUtils.AnswerToString(questionFromDto.Answer));
                                }

                                RecordFirstAnswer(interview, @event.EventTimeStamp);
                            }
                        }
                    }
                    var responsible = this.users.GetUser(state.ResponsibleId);
                    if (responsible?.Supervisor != null)
                    {
                        state.SupervisorId = responsible.Supervisor.Id;
                        state.SupervisorName = responsible.Supervisor.Name;
                    }
                    state.Status = @event.Payload.Status;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewReceivedByInterviewer> @event)
        {
            //do not update UpdateDate for interview
            state.ReceivedByInterviewerAtUtc = @event.Payload.OriginDate?.UtcDateTime ?? @event.EventTimeStamp;
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewReceivedBySupervisor> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.ReceivedByInterviewerAtUtc = null;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewPaused> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                LogInterviewTotalInterviewingTime(interview, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.UtcTime.Value);
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewResumed> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                var resumeDateUtc = @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.UtcTime;
                if (!state.LastResumeEventUtcTimestamp.HasValue)
                {
                    state.LastResumeEventUtcTimestamp = resumeDateUtc;
                }
                else if (state.LastResumeEventUtcTimestamp > resumeDateUtc)
                {
                    state.LastResumeEventUtcTimestamp = resumeDateUtc;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswerCommentResolved> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                state.HasResolvedComments = true;
            });
        }

        private static void LogInterviewTotalInterviewingTime(InterviewSummary interview, DateTime endTimestamp)
        {
            if (interview.LastResumeEventUtcTimestamp.HasValue)
            {
                TimeSpan timeDiffWithLastEvent = endTimestamp - interview.LastResumeEventUtcTimestamp.Value;
                if (interview.InterviewDuration.HasValue)
                {
                    interview.InterviewDuration += timeDiffWithLastEvent;
                }
                else
                {
                    interview.InterviewDuration = timeDiffWithLastEvent;
                }

                interview.LastResumeEventUtcTimestamp = null;
            }
        }

        private string GetResponsibleIdName(Guid responsibleId)
        {
            var responsible = this.users.GetUser(responsibleId);
            return responsible != null ? responsible.UserName : "<UNKNOWN RESPONSIBLE>";
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            var area = new Area(@event.Payload.Geometry, @event.Payload.MapName, @event.Payload.NumberOfPoints,
                @event.Payload.AreaSize, @event.Payload.Length, @event.Payload.Coordinates, 
                @event.Payload.DistanceToEditor, @event.Payload.RequestedAccuracy, @event.Payload.RequestedFrequency);
            return this.AnswerQuestion(state, @event.Payload.QuestionId, area, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            var audioAnswer = AudioAnswer.FromString(@event.Payload.FileName, @event.Payload.Length);
            return this.AnswerQuestion(state, @event.Payload.QuestionId, audioAnswer, @event.EventTimeStamp, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewRestored> @event)
        {
            if (@event.Origin == Implementation.Synchronization.Constants.HeadquartersSynchronizationOrigin)
                return state;

            var createdStatusRecord =
                state.InterviewCommentedStatuses.FirstOrDefault(s => s.Status == InterviewExportedAction.Created);
            state.CreatedDate = createdStatusRecord?.Timestamp ?? @event.EventTimeStamp;

            var firstAnswerSetStatusRecord =
                state.InterviewCommentedStatuses.FirstOrDefault(s =>
                    s.Status == InterviewExportedAction.FirstAnswerSet);
            if (firstAnswerSetStatusRecord != null)
            {
                state.FirstAnswerDate = firstAnswerSetStatusRecord.Timestamp;
                state.FirstSupervisorId = firstAnswerSetStatusRecord.SupervisorId;
                state.FirstSupervisorName = firstAnswerSetStatusRecord.SupervisorName;
                state.FirstInterviewerId = firstAnswerSetStatusRecord.InterviewerId;
                state.FirstInterviewerName = firstAnswerSetStatusRecord.InterviewerName;
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SubstitutionTitlesChanged> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                state.HasSmallSubstitutions = true;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<VariablesChanged> @event)
        {
            var questionnaire = GetQuestionnaire(state);

            var updateDateTime = @event.Payload.OriginDate?.UtcDateTime ?? @event.EventTimeStamp;
            return this.UpdateInterviewSummary(state, updateDateTime, interview =>
            {
                foreach (var changedVariable in @event.Payload.ChangedVariables)
                {
                    var variableId = questionnaire.GetEntityIdMapValue(changedVariable.Identity.Id);
                    var varType = questionnaire.GetVariableVariableType(changedVariable.Identity.Id);
                    if (interview.IsEntityIdentifying(variableId))
                    {
                        interview.AnswerFeaturedVariable(variableId, changedVariable.NewValue, varType);
                    }
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewModeChanged> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.InterviewMode = @event.Payload.Mode;
                interview.ReceivedByInterviewerAtUtc = null;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswerCommented> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                //updating last update date
            });
        }
    }
}

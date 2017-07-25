using System;
using System.Linq;
using System.Runtime.Caching;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class InterviewSummaryDenormalizer : AbstractFunctionalEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, InterviewCreated>,
        IUpdateHandler<InterviewSummary, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewSummary, InterviewOnClientCreated>,
        IUpdateHandler<InterviewSummary, InterviewStatusChanged>,
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
        IUpdateHandler<InterviewSummary, InterviewDeclaredInvalid>,
        IUpdateHandler<InterviewSummary, InterviewDeclaredValid>,
        IUpdateHandler<InterviewSummary, SynchronizationMetadataApplied>,
        IUpdateHandler<InterviewSummary, InterviewHardDeleted>,
        IUpdateHandler<InterviewSummary, AnswerRemoved>,
        IUpdateHandler<InterviewSummary, InterviewKeyAssigned>,
        IUpdateHandler<InterviewSummary, InterviewReceivedByInterviewer>,
        IUpdateHandler<InterviewSummary, InterviewReceivedBySupervisor>,
        IUpdateHandler<InterviewSummary, AreaQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AudioQuestionAnswered>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IUserViewFactory users;

        public InterviewSummaryDenormalizer(IReadSideRepositoryWriter<InterviewSummary> interviewSummary,
            IUserViewFactory users, 
            IQuestionnaireStorage questionnaireStorage)
            : base(interviewSummary)
        {
            this.users = users;
            this.questionnaireStorage = questionnaireStorage;
        }

        public override object[] Readers => new object[0];

        private InterviewSummary UpdateInterviewSummary(InterviewSummary interviewSummary, DateTime updateDateTime, Action<InterviewSummary> update)
        {
            update(interviewSummary);
            interviewSummary.UpdateDate = updateDateTime;
            return interviewSummary;
        }

        private InterviewSummary AnswerQuestion(InterviewSummary interviewSummary, Guid questionId, object answer, DateTime updateDate)
        {
           return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == questionId))
                {
                    interview.AnswerFeaturedQuestion(questionId, AnswerUtils.AnswerToString(answer));
                }
            });
        }

        private InterviewSummary AnswerFeaturedQuestionWithOptions(InterviewSummary interviewSummary, Guid questionId, DateTime updateDate,
            params decimal[] answers)
        {
            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == questionId))
                {
                    var questionnaire = this.GetQuestionnaire(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
                    if (questionnaire == null)
                        return;

                    var question = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == questionId);
                    if (question == null || question.Answers == null)
                        return;

                    var optionStrings = answers.Select(answerValue => question.Answers.First(x => decimal.Parse(x.AnswerValue) == answerValue).AnswerText)
                                       .ToList();

                    interview.AnswerFeaturedQuestion(questionId, string.Join(",", optionStrings));
                }
            });
        }

        private InterviewSummary CreateInterviewSummary(Guid userId, Guid questionnaireId, long questionnaireVersion,
            Guid eventSourceId, DateTime eventTimeStamp, bool wasCreatedOnClient, int? assignmentId)
        {
            var responsible = this.users.GetUser(new UserViewInputModel(userId));
            var questionnarie = this.GetQuestionnaire(questionnaireId, questionnaireVersion);

            var interviewSummary = new InterviewSummary(questionnarie)
            {
                InterviewId = eventSourceId,
                WasCreatedOnClient = wasCreatedOnClient,
                UpdateDate = eventTimeStamp,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                QuestionnaireTitle = questionnarie.Title,
                ResponsibleId = userId, // Creator is responsible
                ResponsibleName = responsible != null ? responsible.UserName : "<UNKNOWN USER>",
                ResponsibleRole = responsible.Roles.First(),
                AssignmentId = assignmentId
            };

            return interviewSummary;
        }

        private readonly MemoryCache questionnaireCache = new MemoryCache("QuestionnaireCache");

        private QuestionnaireDocument GetQuestionnaire(Guid questionnaireId, long questionnaireVersion)
        {
            string key = questionnaireId.ToString() + questionnaireVersion.ToString();
            if (this.questionnaireCache.Contains(key))
                return (QuestionnaireDocument)this.questionnaireCache[key];

            var questionare = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireId, questionnaireVersion);
            this.questionnaireCache[key] = questionare;
            return questionare;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCreated> @event)
        {
            return this.CreateInterviewSummary(@event.Payload.UserId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion, @event.EventSourceId, @event.EventTimeStamp, false, @event.Payload.AssignmentId);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            return this.CreateInterviewSummary(@event.Payload.UserId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion, @event.EventSourceId, @event.EventTimeStamp, false, @event.Payload.AssignmentId);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            return this.CreateInterviewSummary(@event.Payload.UserId, @event.Payload.QuestionnaireId,
             @event.Payload.QuestionnaireVersion, @event.EventSourceId, @event.EventTimeStamp, wasCreatedOnClient: true, assignmentId: @event.Payload.AssignmentId);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewStatusChanged> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.Status = @event.Payload.Status;
                interview.WasRejectedBySupervisor = interview.WasRejectedBySupervisor || @event.Payload.Status == InterviewStatus.RejectedBySupervisor;
                interview.IsDeleted = @event.Payload.Status == InterviewStatus.Deleted;

                if (interview.Status == @event.Payload.Status)
                {
                    interview.LastStatusChangeComment = @event.Payload.Comment;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.IsDeleted = true;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SupervisorAssigned> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                var user = this.users.GetUser(new UserViewInputModel(@event.Payload.SupervisorId));
                var supervisorName = user != null ? user.UserName : "<UNKNOWN SUPERVISOR>";

                interview.ResponsibleId = @event.Payload.SupervisorId;
                interview.ResponsibleName = supervisorName;
                interview.ResponsibleRole = UserRoles.Supervisor;
                interview.TeamLeadId = @event.Payload.SupervisorId;
                interview.TeamLeadName = supervisorName;
                interview.IsAssignedToInterviewer = false;
                interview.ReceivedByInterviewer = false;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp);
        }


        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswerRemoved> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == @event.Payload.QuestionId))
                {
                    interview.AnswerFeaturedQuestion(@event.Payload.QuestionId, "");
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.AnswerFeaturedQuestionWithOptions(state, @event.Payload.QuestionId, @event.EventTimeStamp, @event.Payload.SelectedValues);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.AnswerFeaturedQuestionWithOptions(state, @event.Payload.QuestionId, @event.EventTimeStamp, @event.Payload.SelectedValue);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer.ToString(ExportFormatSettings.ExportDateTimeFormat), @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            var answerByGeoQuestion = new GeoPosition(@event.Payload.Latitude, @event.Payload.Longitude, @event.Payload.Accuracy, @event.Payload.Altitude, @event.Payload.Timestamp);
            return this.AnswerQuestion(state, @event.Payload.QuestionId, answerByGeoQuestion, @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.AnswerQuestion(state, @event.Payload.QuestionId, @event.Payload.Answer, @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                foreach (var question in @event.Payload.Questions)
                {
                    if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == question.Id))
                    {
                        interview.AnswerFeaturedQuestion(question.Id, string.Empty);
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

                    interview.ReceivedByInterviewer = false;
                }
                else
                {
                    interview.ResponsibleId = interview.TeamLeadId;
                    interview.ResponsibleName = interview.TeamLeadName;
                    interview.ResponsibleRole = UserRoles.Supervisor;
                    interview.IsAssignedToInterviewer = false;
                    interview.ReceivedByInterviewer = false;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewDeclaredInvalid> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.HasErrors = true;
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

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewDeclaredValid> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.HasErrors = false;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SynchronizationMetadataApplied> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                if (state.WasCreatedOnClient)
                {
                    if (@event.Payload.FeaturedQuestionsMeta != null)
                    {
                        foreach (var questionFromDto in @event.Payload.FeaturedQuestionsMeta)
                        {
                            if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == questionFromDto.Id))
                            {
                                var questionnaire = GetQuestionnaire(interview.QuestionnaireId, interview.QuestionnaireVersion);
                                var questionType = questionnaire.FirstOrDefault<IQuestion>(x => x.PublicKey == questionFromDto.Id).QuestionType;
                                if (questionType == QuestionType.SingleOption)
                                {
                                    decimal[] answer = { Convert.ToDecimal(questionFromDto.Answer) };
                                    AnswerFeaturedQuestionWithOptions(interview, questionFromDto.Id, @event.EventTimeStamp, answer);
                                }
                                else
                                {
                                    interview.AnswerFeaturedQuestion(questionFromDto.Id, AnswerUtils.AnswerToString(questionFromDto.Answer));
                                }
                            }
                        }
                    }
                    var responsible = this.users.GetUser(new UserViewInputModel(state.ResponsibleId));
                    if (responsible?.Supervisor != null)
                    {
                        state.TeamLeadId = responsible.Supervisor.Id;
                        state.TeamLeadName = responsible.Supervisor.Name;
                    }
                    state.Status = @event.Payload.Status;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewReceivedByInterviewer> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.ReceivedByInterviewer = true;
            });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewReceivedBySupervisor> @event)
        {
            return this.UpdateInterviewSummary(state, @event.EventTimeStamp, interview =>
            {
                interview.ReceivedByInterviewer = false;
            });
        }

        private string GetResponsibleIdName(Guid responsibleId)
        {
            var responsible = this.users.GetUser(new UserViewInputModel(responsibleId));
            return responsible != null ? responsible.UserName : "<UNKNOWN RESPONSIBLE>";
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            var area = new Area(@event.Payload.Geometry, @event.Payload.MapName, @event.Payload.AreaSize, @event.Payload.Length, 
                @event.Payload.Coordinates, @event.Payload.DistanceToEditor);
            return this.AnswerQuestion(state, @event.Payload.QuestionId, area, @event.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            var audioAnswer = AudioAnswer.FromString(@event.Payload.FileName, @event.Payload.Length);
            return this.AnswerQuestion(state, @event.Payload.QuestionId, audioAnswer, @event.EventTimeStamp);
        }
    }
}

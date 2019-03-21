using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public interface IInterviewStatisticsReportDenormalizer : ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>> { }

    internal class InterviewStatisticsReportDenormalizer :
        IInterviewStatisticsReportDenormalizer,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,
        IUpdateHandler<InterviewSummary, QuestionsDisabled>,
        IUpdateHandler<InterviewSummary, QuestionsEnabled>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewStatisticsReportDenormalizer(IUnitOfWork unitOfWork, IQuestionnaireStorage questionnaireStorage)
        {
            this.unitOfWork = unitOfWork;
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            var payloadQuestionId = @event.Payload.QuestionId;
            var rv = new RosterVector(@event.Payload.RosterVector);
            
            UpdateReportStatisticsAnswer(state, payloadQuestionId, rv, @event.Payload.SelectedValue);

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(state.QuestionnaireId, state.QuestionnaireVersion);
            
            unitOfWork.Session.Connection.Execute("delete from readside.report_statistics " +
                                                  "where interview_id = @interviewId and entity_id = @entityId " +
                                                  "and rostervector = @rostervector",
                @event.Payload.Questions.Select(q => new
                {
                    InterviewId = state.Id,
                    EntityId = questionnaire.EntitiesIdMap[q.Id],
                    RosterVector = q.RosterVector.AsString()
                }));

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            UpdateReportStatisticsAnswer(state, @event.Payload.QuestionId, new RosterVector(@event.Payload.RosterVector), 
                @event.Payload.SelectedValues );

            return state;
        }

        private void UpdateReportStatisticsAnswer(InterviewSummary state, Guid payloadQuestionId, RosterVector rv, params decimal[] answer)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(state.QuestionnaireId, state.QuestionnaireVersion);

            var question = questionnaire.Find<IQuestion>(payloadQuestionId);

            if (!IsEligibleQuestion(question)) return;
            
            unitOfWork.Session.Connection.Execute(
                @"insert into readside.report_statistics (interview_id, entity_id, rostervector, answer, ""type"")
                    values(@interviewid,@entityId,@rostervector, @answer::int[], @type)
                    on conflict (interview_id, entity_id, rostervector)
                    do update set answer = @answer::int[]", new
                {
                    Type = StatisticsReportType.Categorical,
                    RosterVector = rv.AsString(),
                    EntityId = questionnaire.EntitiesIdMap[payloadQuestionId],
                    InterviewId = state.Id,
                    Answer = answer
                });
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsDisabled> @event)
        {
            UpdateQuestionEnablement(state, false, @event.Payload.Questions);
            return state;
        }

        private void UpdateQuestionEnablement(InterviewSummary summary, bool enabled, Identity[] questionIds)
        {
            var questionnaire =
                questionnaireStorage.GetQuestionnaireDocument(summary.QuestionnaireId, summary.QuestionnaireVersion);

            List<Identity> questions = questionIds
                .Where(q => IsEligibleQuestion(questionnaire.Find<IQuestion>(q.Id)))
                .ToList();

            unitOfWork.Session.Connection.Execute("update readside.report_statistics set is_enabled = @enabled " +
                                                  "where interview_id = @interviewid " +
                                                  "and rostervector = @rostervector and entity_id = @entityId",

                questions.Select(q =>
                    new
                    {
                        RosterVector = q.RosterVector.AsString(),
                        EntityId = questionnaire.EntitiesIdMap[q.Id],
                        InterviewId = summary.Id,
                        enabled
                    }));
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsEnabled> @event)
        {
            UpdateQuestionEnablement(state, true, @event.Payload.Questions);
            return state;
        }
        
        bool IsEligibleQuestion(IQuestion question)
        {
            if (question.CascadeFromQuestionId != null) return false;
            if (question.LinkedToQuestionId != null || question.LinkedToRosterId != null) return false;
            if (question.IsFilteredCombobox == true) return false;
            return true;
        }
    }
}

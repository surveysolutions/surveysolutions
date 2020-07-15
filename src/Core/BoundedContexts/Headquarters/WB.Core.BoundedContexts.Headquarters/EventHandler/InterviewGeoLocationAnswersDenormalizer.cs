﻿using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Refit;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewGeoLocationAnswersDenormalizer :
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, QuestionsEnabled>,
        IUpdateHandler<InterviewSummary, QuestionsDisabled>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,
        IUpdateHandler<InterviewSummary, RosterInstancesRemoved>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewGeoLocationAnswersDenormalizer(IQuestionnaireStorage questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            var questionId = @event.Payload.QuestionId;
            var rosterVector = NormalizeRosterVector(RosterVector.Convert(@event.Payload.RosterVector));

            var answer = state.GpsAnswers.FirstOrDefault(x => x.QuestionId == questionId && x.RosterVector == rosterVector);
                
            if (answer != null)
            {
                answer.Latitude = @event.Payload.Latitude;
                answer.Longitude = @event.Payload.Longitude;
                answer.Timestamp = @event.Payload.Timestamp;
                answer.IsEnabled = true;
            }
            else
            {
                state.GpsAnswers.Add(new InterviewGps
                {
                    QuestionId = @event.Payload.QuestionId,
                    RosterVector = rosterVector,
                    Latitude = @event.Payload.Latitude,
                    Longitude = @event.Payload.Longitude,
                    Timestamp = @event.Payload.Timestamp,
                    IsEnabled = true,
                    InterviewSummary = state
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var gpsQuestionIdentities = GetGpsIdentities(state, @event.Payload.Questions);
            if (!gpsQuestionIdentities.Any()) return state;

            var questionIdentities = @event.Payload.Questions
                .Select(x => (x.Id, NormalizeRosterVector(x.RosterVector)))
                .ToHashSet();

            var toRemove = state.GpsAnswers.Where(g => questionIdentities.Contains((g.QuestionId, g.RosterVector))).ToList();

            foreach (var remove in toRemove)
            {
                state.GpsAnswers.Remove(remove);
            }
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsEnabled> @event)
        {
            Identity[] gpsQuestionIdentities = GetGpsIdentities(state, @event.Payload.Questions);
            if (gpsQuestionIdentities.Length == 0) return state;

            var questionIdentities = @event.Payload.Questions
                .Select(x => (x.Id, NormalizeRosterVector(x.RosterVector)))
                .ToHashSet();

            foreach (var answer in state.GpsAnswers.Where(g =>
                questionIdentities.Contains((g.QuestionId, g.RosterVector))))
            {
                answer.IsEnabled = true;
            }

            return state;
        }

        private string NormalizeRosterVector(RosterVector rosterVector)
        {
            return rosterVector.ToString().Trim('_');
        }
        
        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsDisabled> @event)
        {
            var gpsQuestionIdentities = GetGpsIdentities(state, @event.Payload.Questions);
            if (!gpsQuestionIdentities.Any()) return state;

            var questionIdentities = @event.Payload.Questions
                .Select(x => (x.Id, NormalizeRosterVector(x.RosterVector)))
                .ToHashSet();

            foreach (var answer in state.GpsAnswers.Where(g =>
                questionIdentities.Contains((g.QuestionId, g.RosterVector))))
            {
                answer.IsEnabled = false;
            }

            return state;
        }
        
        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<RosterInstancesRemoved> @event)
        {
            var removedRosterInstances = @event.Payload.Instances
                .Select(x => $"{x.GroupId}{NormalizeRosterVector(x.GetIdentity().RosterVector)}")
                .ToArray();

            var questionsInRosters = state.GpsAnswers.Where(x => !string.IsNullOrEmpty(x.RosterVector))
                .ToList();
            
            if (questionsInRosters.Count > 0)
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(
                    new QuestionnaireIdentity(state.QuestionnaireId, state.QuestionnaireVersion), null);
                
                if (questionnaire == null) return state;

                var storedAnswersInRosters = questionsInRosters.Select(x => new
                    {
                        entity = x,
                        identityToRemove = $"{questionnaire.GetRostersFromTopToSpecifiedQuestion(x.QuestionId).Last()}{x.RosterVector}"
                    })
                    .ToList();

                foreach (var removedRosterInstance in removedRosterInstances)
                {
                    foreach (var storedAnswer in storedAnswersInRosters)
                    {
                        if (storedAnswer.identityToRemove == removedRosterInstance)
                        {
                            state.GpsAnswers.Remove(storedAnswer.entity);
                        }
                    }
                }
            }
        
            return state;
        }

        private Identity[] GetGpsIdentities(InterviewSummary interview, Identity[] allQuestionIdentities)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(interview.QuestionnaireIdentity), null);

            return allQuestionIdentities
                .Where(x => questionnaire.GetQuestionType(x.Id) == QuestionType.GpsCoordinates)
                .ToArray();
        }

    }
}

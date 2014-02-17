﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Events.User;
using Main.Core.Utility;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class AnswersByVariableDenormalizer :
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<AnswerRemoved>,
        IEventHandler
    {
        private readonly IReadSideRepositoryWriter<AnswersByVariableCollection> answersByVariableStorage;
        private readonly IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private readonly IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> variablesStorage;

        public AnswersByVariableDenormalizer(IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage,
            IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> variablesStorage,
            IReadSideRepositoryWriter<AnswersByVariableCollection> answersByVariableStorage)
        {
            this.interviewBriefStorage = interviewBriefStorage;
            this.variablesStorage = variablesStorage;
            this.answersByVariableStorage = answersByVariableStorage;
        }

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
        {
            Guid interviewId = evnt.EventSourceId;
            Guid questionId = evnt.Payload.QuestionId;
            decimal[] propagationVector = evnt.Payload.PropagationVector;

             var interviewBrief = this.interviewBriefStorage.GetById(interviewId);

            if (interviewBrief == null) return;

            var questionnaireVersiondKey = RepositoryKeysHelper.GetVersionedKey(interviewBrief.QuestionnaireId, interviewBrief.QuestionnaireVersion);

            var variablesInfoStorage = this.variablesStorage.GetById(questionnaireVersiondKey);

            if (variablesInfoStorage == null)  return;

            string variableName = variablesInfoStorage.QuestionIdToVariableMap[questionId];

            var variableByQuestionnaireKey = SharedKernels.DataCollection.Utils.RepositoryKeysHelper.GetVariableByQuestionnaireKey(variableName, questionnaireVersiondKey);

            var collectedAnswers = this.answersByVariableStorage.GetById(variableByQuestionnaireKey);

            if (collectedAnswers == null) return;

            if (!collectedAnswers.Answers.ContainsKey(interviewId)) return;

            var levelId = this.CreateLevelIdFromPropagationVector(propagationVector);

            if (!collectedAnswers.Answers[interviewId].ContainsKey(levelId)) return;

            collectedAnswers.Answers[interviewId].Remove(levelId);
            this.answersByVariableStorage.Store(collectedAnswers, variableByQuestionnaireKey);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            var answerString = string.Format("{0};{1}", evnt.Payload.Latitude, evnt.Payload.Longitude);

            this.UpdateAnswerCollection(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector, answerString);
        }

        private void UpdateAnswerCollection(Guid interviewId, Guid questionId, decimal[] propagationVector, string answerString)
        {
            var interviewBrief = this.interviewBriefStorage.GetById(interviewId);

            if (interviewBrief == null) return;

            var questionnaireVersiondKey = RepositoryKeysHelper.GetVersionedKey(interviewBrief.QuestionnaireId, interviewBrief.QuestionnaireVersion);

            var variablesInfoStorage = this.variablesStorage.GetById(questionnaireVersiondKey);

            if (variablesInfoStorage == null) return;

            string variableName = variablesInfoStorage.QuestionIdToVariableMap[questionId];

            var variableByQuestionnaireKey = SharedKernels.DataCollection.Utils.RepositoryKeysHelper.GetVariableByQuestionnaireKey(variableName, questionnaireVersiondKey);

            var collectedAnswers =  this.answersByVariableStorage.GetById(variableByQuestionnaireKey) ?? new AnswersByVariableCollection();

            if (!collectedAnswers.Answers.ContainsKey(interviewId))
            {
                collectedAnswers.Answers.Add(interviewId, new Dictionary<string, string>());
            }
            var levelId = this.CreateLevelIdFromPropagationVector(propagationVector);

            if (!collectedAnswers.Answers[interviewId].ContainsKey(levelId))
            {
                collectedAnswers.Answers[interviewId].Add(levelId, answerString);
            }
            else
            {
                collectedAnswers.Answers[interviewId][levelId] = answerString;
            }

            this.answersByVariableStorage.Store(collectedAnswers, variableByQuestionnaireKey);
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            return vector.Length == 0 ? "#" : string.Join(",", vector);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof (InterviewBrief), typeof (SynchronizationDelta) }; }
        }
    }
}
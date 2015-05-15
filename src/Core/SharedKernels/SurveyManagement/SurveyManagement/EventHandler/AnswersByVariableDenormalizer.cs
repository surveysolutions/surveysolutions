using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class AnswersByVariableDenormalizer : BaseDenormalizer,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<AnswersRemoved>
    {
        private readonly IReadSideKeyValueStorage<AnswersByVariableCollection> answersByVariableStorage;
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewBriefStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> variablesStorage;

        public override object[] Writers
        {
            get { return new[] { answersByVariableStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] { interviewBriefStorage, variablesStorage }; }
        }

        public AnswersByVariableDenormalizer(IReadSideRepositoryReader<InterviewSummary> interviewBriefStorage,
            IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> variablesStorage,
            IReadSideKeyValueStorage<AnswersByVariableCollection> answersByVariableStorage)
        {
            this.interviewBriefStorage = interviewBriefStorage;
            this.variablesStorage = variablesStorage;
            this.answersByVariableStorage = answersByVariableStorage;
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            var answerString = string.Format("{0};{1}", evnt.Payload.Latitude, evnt.Payload.Longitude);

            this.UpdateAnswerCollection(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector, answerString);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.HandleSingleRemove(evnt.EventSourceId, question.Id, question.RosterVector);
            }
        }

        public void HandleSingleRemove(Guid interviewId, Guid questionId, decimal[] propagationVector)
        {
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
            return vector.Length == 0 ? "#" : EventHandlerUtils.CreateLeveKeyFromPropagationVector(vector);
        }
    }
}
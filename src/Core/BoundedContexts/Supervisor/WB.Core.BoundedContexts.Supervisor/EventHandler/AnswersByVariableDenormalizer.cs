using System;
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
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.Synchronization;
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

        private readonly Dictionary<string, QuestionnaireQuestionsInfo> questionsInfoCache =
            new Dictionary<string, QuestionnaireQuestionsInfo>();

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

            string variableByQuestionnaireKey;

            var collectedAnswers = GetAnswersCollection(interviewId, questionId, out variableByQuestionnaireKey);

            if (collectedAnswers == null)
                return;

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
            string variableByQuestionnaireKey;

            var collectedAnswers = GetAnswersCollection(interviewId, questionId, out variableByQuestionnaireKey) ?? new AnswersByVariableCollection();

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

        private AnswersByVariableCollection GetAnswersCollection(Guid interviewId, Guid questionId, out string variableByQuestionnaireKey)
        {
            variableByQuestionnaireKey = string.Empty;

            var interviewBrief = this.interviewBriefStorage.GetById(interviewId);

            var questionnaireVersiondKey = RepositoryKeysHelper.GetVersionedKey(interviewBrief.QuestionnaireId, interviewBrief.QuestionnaireVersion);

            string variableName = this.GetVariableNameByQuestionId(questionnaireVersiondKey, questionId);

            if (string.IsNullOrWhiteSpace(variableName))
                return null;

             variableByQuestionnaireKey = RepositoryKeysHelper.GetVariableByQuestionnaireKey(variableName, questionnaireVersiondKey);

            return this.answersByVariableStorage.GetById(variableByQuestionnaireKey);
        }

        private string GetVariableNameByQuestionId(string questionnaireVersiondKey, Guid questionId)
        {
            if (!questionsInfoCache.ContainsKey(questionnaireVersiondKey))
            {
                questionsInfoCache[questionnaireVersiondKey] = variablesStorage.GetById(questionnaireVersiondKey);
            }

            return this.questionsInfoCache[questionnaireVersiondKey] == null 
                ? null 
                : this.questionsInfoCache[questionnaireVersiondKey].GuidToVariableMap[questionId];
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

    public class AnswersByVariableCollection : IReadSideRepositoryEntity
    {
        public AnswersByVariableCollection()
        {
            this.Answers = new Dictionary<Guid, Dictionary<string, string>>();
        }
        public Dictionary<Guid, Dictionary<string, string>> Answers { get; set; }
    }

}
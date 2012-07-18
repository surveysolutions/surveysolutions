using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class UpdateGroupCommand : ICommand
    {
        public string GroupText
        {
            get;
            private set;
        }
        public Propagate Paropagateble
        {
            get;
            private set;
        }
        public string QuestionnaireId
        {
            get;
            private set;
        }
        public Guid GroupPublicKey
        {
            get;
            private set;
        }
        public List<Guid> Triggers { get; private set; }
        public UserLight Executor { get; set; }
        public string ConditionExpression { get; set; }

        public UpdateGroupCommand(string groupText, Propagate propagateble, string questionnaireId, List<Guid> triggers, Guid parentGroup, UserLight executor, string conditionExpression)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = parentGroup;
            this.Triggers = triggers;
            this.ConditionExpression = conditionExpression;
            Executor = executor;
        }
        public UpdateGroupCommand(string groupText, Propagate propagateble, string questionnaireId, Guid parentGroup, UserLight executor, string conditionExpression)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.GroupPublicKey = parentGroup;
            this.ConditionExpression = conditionExpression;
            Executor = executor;
        }
    }
}

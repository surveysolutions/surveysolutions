using System;
using Ncqrs.Commanding;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateGroup")]
    public class UpdateGroupCommand : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
        public string GroupText { get; set; }
        public Propagate Propagateble { get; set; }
        public Guid GroupPublicKey { get; set; }
       // public List<Guid> Triggers { get; set; }
        public UserLight Executor { get; set; }
        public string ConditionExpression { get; set; }

        //public UpdateGroupCommand(string groupText, Propagate propagateble, Guid questionnaireId, List<Guid> triggers, Guid parentGroup, UserLight executor, string conditionExpression)
        //{
        //    this.GroupText = groupText;
        //    this.Propagateble = propagateble;
        //    this.QuestionnaireId = questionnaireId;
        //    this.GroupPublicKey = parentGroup;
        //    this.Triggers = triggers;
        //    this.ConditionExpression = conditionExpression;
        //    this.Executor = executor;
        //}

        public UpdateGroupCommand(string groupText, Propagate propagateble, Guid questionnaireId, Guid groupPublicKey, UserLight executor, string conditionExpression)
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupText = groupText;
            this.Propagateble = propagateble;
            this.GroupPublicKey = groupPublicKey;
            this.ConditionExpression = conditionExpression;
            this.Executor = executor;
        }
    }
}

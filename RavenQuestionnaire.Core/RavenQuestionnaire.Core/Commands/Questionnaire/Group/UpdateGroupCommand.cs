using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateGroup")]
    public class UpdateGroupCommand : CommandBase
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
        [AggregateRootId]
        public Guid QuestionnaireId
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
        public UpdateGroupCommand(string groupText, Propagate propagateble, Guid questionnaireId, List<Guid> triggers, Guid parentGroup)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = questionnaireId;
            this.GroupPublicKey = parentGroup;
            this.Triggers = triggers;
        }
        public UpdateGroupCommand(string groupText, Propagate propagateble, Guid questionnaireId, Guid parentGroup)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = questionnaireId;
            this.GroupPublicKey = parentGroup;
        }
    }
}

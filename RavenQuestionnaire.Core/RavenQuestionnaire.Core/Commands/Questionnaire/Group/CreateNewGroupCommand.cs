using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class CreateNewGroupCommand : ICommand
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
            get;private set;
        }
        public Guid? ParentGroupPublicKey
        {
            get; private set;
        }
        public List<Guid> Triggers { get; private set; }
        public UserLight Executor { get; set; }
        public CreateNewGroupCommand(string groupText, Propagate propagateble, string questionnaireId, Guid? parentGroup, UserLight executor)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.ParentGroupPublicKey = parentGroup;
            Executor = executor;
        }
        public CreateNewGroupCommand(string groupText, Propagate propagateble, string questionnaireId, List<Guid> triggers, Guid? parentGroup, UserLight executor)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Triggers = triggers;
            this.ParentGroupPublicKey = parentGroup;
            Executor = executor;
        }
    }
}

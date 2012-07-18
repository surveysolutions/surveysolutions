using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class CreateNewGroupCommand : ICommand
    {
        public string GroupText { get; private set; }
        public Propagate Paropagateble { get; private set; }
        public string QuestionnaireId { get; private set; }
        public string Condition { get; set; }
        public Guid? ParentGroupPublicKey { get; private set; }
        public Guid PublicKey { get; private set; }
        public List<Guid> Triggers { get; private set; }

        public UserLight Executor { get; set; }
        public CreateNewGroupCommand(Guid publicKey, string groupText, Propagate propagateble, string questionnaireId, Guid? parentGroup, UserLight executor, string condition)
        {
            this.PublicKey = publicKey;
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.ParentGroupPublicKey = parentGroup;
            this.Condition = condition;
            Executor = executor;
        }
        public CreateNewGroupCommand(Guid publicKey, string groupText, Propagate propagateble, string questionnaireId, List<Guid> triggers, Guid? parentGroup, UserLight executor, string condition)
        {
            this.PublicKey = publicKey;
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Triggers = triggers;
            this.ParentGroupPublicKey = parentGroup;
            this.Condition = condition;
            Executor = executor;
        }

        public CreateNewGroupCommand(string groupText, Guid publicKey, Propagate propagateble, string questionnaireId, Guid? parentGroup, UserLight executor, string condition)
        {
            this.GroupText = groupText;
            this.Paropagateble = propagateble;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.ParentGroupPublicKey = parentGroup;
            this.PublicKey = publicKey;
            Executor = executor;
            this.Condition = condition;
        }
        
    }
}

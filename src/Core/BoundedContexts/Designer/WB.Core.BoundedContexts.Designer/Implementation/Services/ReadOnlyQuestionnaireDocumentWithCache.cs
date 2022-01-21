using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{

    public class ReadOnlyQuestionnaireDocumentWithCache : ReadOnlyQuestionnaireDocument 
    {
        private readonly Dictionary<Guid, EntityWithMeta> allItemsWithMeta = new Dictionary<Guid, EntityWithMeta>();
        private readonly Dictionary<string, IQuestion> questionsCache = new Dictionary<string, IQuestion>();
        private readonly Dictionary<string, IVariable> variableCache = new Dictionary<string, IVariable>();
        private readonly Dictionary<string, IGroup> groupCache = new Dictionary<string, IGroup>();
        
        public ReadOnlyQuestionnaireDocumentWithCache(QuestionnaireDocument questionnaire, string? translation = null) 
            : base(questionnaire, translation)
        {
            foreach (var current in allItems)
            {
                this.allItemsWithMeta[current.Entity.PublicKey] = current;

                if(current.Entity is IQuestion asQuestion && !string.IsNullOrEmpty(asQuestion.StataExportCaption))
                    questionsCache.Add(asQuestion.StataExportCaption, asQuestion);
                else if (current.Entity is IVariable asVariable && !string.IsNullOrEmpty(asVariable.Name))
                    variableCache.Add(asVariable.Name, asVariable);
                else if(current.Entity is IGroup asGroup && !string.IsNullOrEmpty(asGroup.VariableName))
                    groupCache.Add(asGroup.VariableName, asGroup);
            }
        }
        
        protected override EntityWithMeta? FindEntityWithMeta(Guid publicKey)
        {
            return this.allItemsWithMeta.ContainsKey(publicKey) 
                ? this.allItemsWithMeta[publicKey]
                : null;
        }
        
        public IComposite? GetEntityByIdOrNull(Guid id)
        {
            return this.allItemsWithMeta.ContainsKey(id) 
                ? this.allItemsWithMeta[id].Entity
                : null;
        }
        
        public override IComposite? GetEntityByVariable(string variableName)
        {
            var question = GetQuestionByName(variableName);
            if (question != null) return question;
            
            var variable = GetVariableByName(variableName);
            if(variable!= null) return variable;
            
            return GetGroupByName(variableName);
        }
        
        public override bool IsRosterSizeQuestion(IQuestion question)
        {
            return groupCache.Values.Any(group => group.RosterSizeQuestionId == question.PublicKey);
        }
        
        public IQuestion? GetQuestionByName(string name) =>
            questionsCache.ContainsKey(name)
                ? questionsCache[name]
                : null;

        public IVariable? GetVariableByName(string name)
            => variableCache.ContainsKey(name)
                ? variableCache[name]
                : null;
        
        public IGroup? GetGroupByName(string name)
            => groupCache.ContainsKey(name)
                ? groupCache[name]
                : null;
    }
}

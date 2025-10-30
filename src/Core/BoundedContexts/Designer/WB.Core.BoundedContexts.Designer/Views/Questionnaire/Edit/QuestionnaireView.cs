using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireView
    {
        private readonly Guid? compileQuestionnaireId;

        public QuestionnaireView(QuestionnaireDocument doc, IEnumerable<SharedPersonView> sharedPersons, 
            Guid? compileQuestionnaireId = null)
        {
            this.compileQuestionnaireId = compileQuestionnaireId;
            this.Source = doc;
            this.SharedPersons = sharedPersons.ToReadOnlyCollection();
        }

        public QuestionnaireDocument Source { get; }
        public IReadOnlyCollection<SharedPersonView> SharedPersons { get; }

        public Guid? CreatedBy => this.Source.CreatedBy;

        public Guid PublicKey => this.Source.PublicKey;

        public string Title => this.Source.Title;

        public bool IsPublic => this.Source.IsPublic;

        public QuestionnaireDocument GetCompiledReadyDocument()
        {
            if (!compileQuestionnaireId.HasValue)
                return Source;

            var clone = Source.Clone();
            clone.PublicKey = compileQuestionnaireId.Value;
            return clone;
        }

        public QuestionnaireDocument GetClientReadyDocument(IMacrosSubstitutionService macrosSubstitutionService)
        {
            var clone = Source.Clone();
            
            clone.GetEntitiesByType<TextQuestion>()
                .ForEach(x => x.Mask = 
                    x.Mask != null 
                        ? macrosSubstitutionService.InlineMacros(x.Mask, clone.Macros.Values) 
                        : null);
            
            if (compileQuestionnaireId.HasValue)
            {
                clone.PublicKey = compileQuestionnaireId.Value;
            }

            return clone;
        }
    }
}


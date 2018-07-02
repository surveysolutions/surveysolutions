using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class MultiLanguageQuestionnaireDocument
    {
        public ReadOnlyQuestionnaireDocument Questionnaire { get; }
        public IReadOnlyCollection<ReadOnlyQuestionnaireDocument> TranslatedQuestionnaires { get; }
        public IReadOnlyCollection<SharedPersonView> SharedPersons { get; }

        public MultiLanguageQuestionnaireDocument(ReadOnlyQuestionnaireDocument originalQuestionnaireDocument,
            IEnumerable<ReadOnlyQuestionnaireDocument> translatedQuestionnaireDocuments,
            IEnumerable<SharedPersonView> sharedPersons)
        {
            this.Questionnaire = originalQuestionnaireDocument;
            this.TranslatedQuestionnaires = translatedQuestionnaireDocuments.ToReadOnlyCollection();
            this.SharedPersons = sharedPersons.ToReadOnlyCollection();
        }

        public Dictionary<Guid, Macro> Macros => this.Questionnaire.Macros;
        public Dictionary<Guid, LookupTable> LookupTables => this.Questionnaire.LookupTables;
        public List<Attachment> Attachments => this.Questionnaire.Attachments;
        public List<Translation> Translations => this.Questionnaire.Translations;
        public string Title => this.Questionnaire.Title;
        public string VariableName => this.Questionnaire.VariableName;
        public Guid PublicKey => this.Questionnaire.PublicKey;

        public T Find<T>(Guid publicKey) where T : class, IComposite
            => this.Questionnaire.Find<T>(publicKey);

        public IEnumerable<T> Find<T>() where T : class
            => this.Questionnaire.Find<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Questionnaire.Find<T>(condition);

        public bool Has<T>(Func<T, bool> condition) where T : class
            => this.Questionnaire.Find<T>(condition).Any();

        public IEnumerable<T> FindInGroup<T>(Guid groupId)
             => this.Questionnaire.FindInGroup<T>(groupId);

        public class TranslatedEntity<TEntity>
        {
            public TranslatedEntity(TEntity entity, string translationName)
            {
                this.Entity = entity;
                this.TranslationName = translationName;
            }

            public TEntity Entity { get; private set; }
            public string TranslationName { get; private set; }
        }

        public IEnumerable<TranslatedEntity<T>> FindWithTranslations<T>(Func<T, bool> condition) where T : class
        {
            var allQuestionnaires = this.Questionnaire.ToEnumerable().Union(this.TranslatedQuestionnaires);
            foreach (var questionnaire in allQuestionnaires)
            {
                var findResult = questionnaire.Find<T>(condition);
                foreach (var entity in findResult)
                {
                    yield return new TranslatedEntity<T>(entity, questionnaire.Translation); 
                }
            }
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
            => this.Questionnaire.FirstOrDefault(condition);

        public IEnumerable<ReadOnlyQuestionnaireDocument.QuestionnaireItemTypeReference> GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder()
            => Questionnaire.GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder();

        
    }
}

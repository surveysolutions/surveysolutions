using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class MultiLanguageQuestionnaireDocument
    {
        private ReadOnlyQuestionnaireDocument Questionnaire { get; set; }
        private ReadOnlyQuestionnaireDocument[] TranslatedQuestionnaires { get; set; }

        public MultiLanguageQuestionnaireDocument(ReadOnlyQuestionnaireDocument originalQuestionnaireDocument,
            params ReadOnlyQuestionnaireDocument[] translatedQuestionnaireDocuments)
        {
            this.Questionnaire = originalQuestionnaireDocument;
            this.TranslatedQuestionnaires = translatedQuestionnaireDocuments.ToArray();
        }



        public Dictionary<Guid, Macro> Macros => this.Questionnaire.Macros;
        public Dictionary<Guid, LookupTable> LookupTables => this.Questionnaire.LookupTables;
        public List<Attachment> Attachments => this.Questionnaire.Attachments;
        public List<Translation> Translations => this.Questionnaire.Translations;
        public string Title => this.Questionnaire.Title;
        public Guid PublicKey => this.Questionnaire.PublicKey;

        public T Find<T>(Guid publicKey) where T : class, IComposite
            => this.Questionnaire.Find<T>(publicKey);

        public IEnumerable<T> Find<T>() where T : class
            => this.Questionnaire.Find<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Questionnaire.Find<T>(condition);

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
            => this.Questionnaire.FirstOrDefault(condition);

        public static implicit operator ReadOnlyQuestionnaireDocument(MultiLanguageQuestionnaireDocument questionnaireDocument)
        {
            return questionnaireDocument.Questionnaire;
        }
//
//
//        private IComposite[] CreateEntitiesIdAndTypePairsInQuestionnaireFlowOrder(QuestionnaireDocument questionnaire)
//        {
//            var result = new List<IComposite>();
//            var stack = new Stack<IComposite>();
//            stack.Push(questionnaire);
//            while (stack.Any())
//            {
//                var current = stack.Pop();
//                for (int i = current.Children.Count - 1; i >= 0; i--)
//                {
//                    var child = current.Children[i];
//                    stack.Push(child);
//                }
//                result.Add(current);
//            }
//            return result.Skip(1).ToArray();
//        }
    }
}
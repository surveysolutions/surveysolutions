using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translator
{
    internal class QuestionnaireTranslator : IQuestionnaireTranslator
    {
        public QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, IQuestionnaireTranslation translation)
        {
            var translatedDocument = originalDocument.Clone();

            TranslateTitles(translatedDocument, translation);

            return translatedDocument;
        }

        private static void TranslateTitles(QuestionnaireDocument questionnaireDocument, IQuestionnaireTranslation translation)
        {
            foreach (var entity in questionnaireDocument.Find<IComposite>())
            {
                TranslateTitle(entity, translation);
            }

            foreach (var question in questionnaireDocument.Find<IQuestion>())
            {
                TranslateInstruction(question, translation);
            }

            // options
            // error messages
            // roster fixed titles
        }

        private static void TranslateTitle(IComposite entity, IQuestionnaireTranslation translation)
        {
            string original = entity.GetTitle();
            string translated = translation.GetTitle(entity.PublicKey);

            string result = string.IsNullOrWhiteSpace(translated) ? translated : original;

            entity.SetTitle(result);
        }

        private static void TranslateInstruction(IQuestion question, IQuestionnaireTranslation translation)
        {
            string original = question.Instructions;
            string translated = translation.GetInstruction(question.PublicKey);

            string result = string.IsNullOrWhiteSpace(translated) ? translated : original;

            question.Instructions = result;
        }
    }
}

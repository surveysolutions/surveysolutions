using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
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
        }

        private static void TranslateTitle(IComposite entity, IQuestionnaireTranslation translation)
        {
            string originalTitle = entity.GetTitle();
            string translatedTitle = translation.GetTitle(entity);

            string resultTitle =
                string.IsNullOrWhiteSpace(translatedTitle)
                    ? translatedTitle
                    : originalTitle;

            entity.SetTitle(resultTitle);
        }
    }
}

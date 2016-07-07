using System;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translator;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationsService
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public TranslationsService(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        public IQuestionnaireTranslation Get(Guid questionnaireId, string culture)
        {
            var storedTranslations =
                this.translations.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId && x.Culture == culture).ToList());

            return new QuestionnaireTranslation(storedTranslations);
        }

        public void Store(Guid questionnaireId, string culture, Stream excelRepresentation)
        {
            using (ExcelPackage package = new ExcelPackage(excelRepresentation))
            {
                var worksheet = package.Workbook.Worksheets[1];

                for (int rowNumber = 2; ; rowNumber++)
                {
                    var questionnaireEntityIdColumn = 3;
                    var translactionColumn = 5;
                    var translationIndexColumn = 2;
                    var translationTypeColumn = 1;

                    if (worksheet.Cells[rowNumber, questionnaireEntityIdColumn].Value != null && worksheet.Cells[rowNumber, translactionColumn].Value != null)
                    {
                        TranslationInstance instance = new TranslationInstance();

                        instance.QuestionnaireId = questionnaireId;
                        instance.Culture = culture;
                        instance.QuestionnaireEntityId = Guid.Parse(worksheet.Cells[rowNumber, questionnaireEntityIdColumn].GetValue<string>());
                        instance.Translation = worksheet.Cells[rowNumber, translactionColumn].GetValue<string>();
                        instance.TranslationIndex = worksheet.Cells[rowNumber, translationIndexColumn].GetValue<string>();
                        instance.Type = (TranslationType) Enum.Parse(typeof(TranslationType), worksheet.Cells[rowNumber, translationTypeColumn].GetValue<string>());

                        translations.Store(instance, instance);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
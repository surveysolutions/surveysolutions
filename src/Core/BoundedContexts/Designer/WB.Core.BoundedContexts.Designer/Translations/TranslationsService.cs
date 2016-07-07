using System;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using OfficeOpenXml;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translator;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public class TranslationsService
    {
        const int translationTypeColumn = 1;
        const int translationIndexColumn = 2;
        const int questionnaireEntityIdColumn = 3;
        const int originalTextColumn = 4;
        const int translactionColumn = 5;

        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage;

        public TranslationsService(IPlainStorageAccessor<TranslationInstance> translations,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.translations = translations;
            this.questionnaireStorage = questionnaireStorage;
        }

        public IQuestionnaireTranslation Get(Guid questionnaireId, string culture)
        {
            var storedTranslations =
                this.translations.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId && x.Culture == culture).ToList());

            return new QuestionnaireTranslation(storedTranslations);
        }

        public byte[] GetAsExcelFile(Guid questionnaireId, string culture)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            var translation = this.Get(questionnaireId, culture);

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Worksheets.Add("Translations");
                var worksheet = excelPackage.Workbook.Worksheets[1];
                var cells = worksheet.Cells;

                cells[1, translationTypeColumn].Value = "Type";
                cells[1, translationIndexColumn].Value = "Index";
                cells[1, questionnaireEntityIdColumn].Value = "Entity Id";
                cells[1, originalTextColumn].Value = "Original";
                cells[1, translationIndexColumn].Value = "Translation";

                int currentRowNumber = 2;

                foreach (var childNode in questionnaire.Children.TreeToEnumerable(x => x.Children))
                {
                    AppendTranslationRow(cells, ref currentRowNumber, TranslationType.Title, childNode.PublicKey, childNode.GetTitle(), translation.GetTitle(childNode.PublicKey));

                    var validatable = childNode as IValidatable;
                    if (validatable != null)
                    {
                        for (int i = 1; i < validatable.ValidationConditions.Count + 1; i++)
                        {
                            AppendTranslationRow(cells,
                                ref currentRowNumber,
                                TranslationType.ValidationMessage,
                                validatable.PublicKey,
                                validatable.ValidationConditions[i - 1].Message,
                                translation.GetValidationMessage(validatable.PublicKey, i),
                                i.ToString());
                        }
                    }

                    var question = childNode as IQuestion;
                    if (question != null)
                    {
                        if (!string.IsNullOrEmpty(question.Instructions))
                        {
                            AppendTranslationRow(cells, ref currentRowNumber, TranslationType.Instruction, question.PublicKey, question.Instructions, translation.GetInstruction(question.PublicKey));
                        }

                        for (int i = 0; i < question.Answers?.Count; i++)
                        {
                            var answerOptionValue = question.Answers[i].AnswerValue;
                            var translatedOptionTitle = translation.GetAnswerOption(question.PublicKey, answerOptionValue);
                            var originalAnswerTitle = question.Answers[i].AnswerText;

                            AppendTranslationRow(cells,
                              ref currentRowNumber,
                              TranslationType.OptionTitle,
                              question.PublicKey,
                              originalAnswerTitle,
                              translatedOptionTitle,
                              answerOptionValue);
                        }
                    }
                }

                return excelPackage.GetAsByteArray();
            }
        }

        private static void AppendTranslationRow(ExcelRange cells,
            ref int currentRowNumber, 
            TranslationType translationType, 
            Guid entityId,
            string originalValue, 
            string translatedValue,
            string translationIndex = null)
        {
            cells[currentRowNumber, translationTypeColumn].Value = (int)translationType;
            cells[currentRowNumber, questionnaireEntityIdColumn].Value = entityId;
            cells[currentRowNumber, originalTextColumn].Value = originalValue;
            cells[currentRowNumber, translactionColumn].Value = translatedValue;
            cells[currentRowNumber, translationIndexColumn].Value = translationIndex;
            currentRowNumber++;
        }

        public void Store(Guid questionnaireId, string culture, byte[] excelRepresentation)
        {
            using (MemoryStream stream = new MemoryStream(excelRepresentation))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[1];

                    for (int rowNumber = 2; ; rowNumber++)
                    {
                        if (worksheet.Cells[rowNumber, questionnaireEntityIdColumn].Value != null && worksheet.Cells[rowNumber, translactionColumn].Value != null)
                        {
                            TranslationInstance instance = new TranslationInstance();

                            instance.QuestionnaireId = questionnaireId;
                            instance.Culture = culture;
                            instance.QuestionnaireEntityId = Guid.Parse(worksheet.Cells[rowNumber, questionnaireEntityIdColumn].GetValue<string>());
                            instance.Translation = worksheet.Cells[rowNumber, translactionColumn].GetValue<string>();
                            instance.TranslationIndex = worksheet.Cells[rowNumber, translationIndexColumn].GetValue<string>();
                            instance.Type = (TranslationType) Enum.Parse(typeof(TranslationType), worksheet.Cells[rowNumber, translationTypeColumn].GetValue<string>());

                            this.translations.Store(instance, instance);
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
}
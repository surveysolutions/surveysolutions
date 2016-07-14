using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using OfficeOpenXml;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    internal class TranslationsService : ITranslationsService
    {
        const int translationTypeColumn = 1;
        const int translationIndexColumn = 2;
        const int questionnaireEntityIdColumn = 3;
        const int originalTextColumn = 4;
        const int translationColumn = 5;
        private const string EntityIdColumnName = "Entity Id";
        private const string TranslationTypeColumnName = "Type";

        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage;

        protected TranslationsService()
        {
            
        }

        public TranslationsService(IPlainStorageAccessor<TranslationInstance> translations,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.translations = translations;
            this.questionnaireStorage = questionnaireStorage;
        }

        public ITranslation Get(Guid questionnaireId, Guid? translationId)
        {
            var language = translationId.FormatGuid();
            if (string.IsNullOrEmpty(language))
                return new QuestionnaireTranslation(new List<TranslationDto>());

            var storedTranslations = this.translations.Query(
                _ => _.Where(x => x.QuestionnaireId == questionnaireId && x.Language == language).ToList())
                .Cast<TranslationDto>()
                .ToList();

            return new QuestionnaireTranslation(storedTranslations);
        }

        public TranslationFile GetAsExcelFile(Guid questionnaireId, Guid? translationId)
        {
            var translationFile = new TranslationFile();
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            var translation = this.Get(questionnaireId, translationId);

            translationFile.QuestionnaireTitle = questionnaire.Title;
            translationFile.TranslationName = translationId.HasValue ? questionnaire.Translations.FirstOrDefault(x => x.TranslationId == translationId)?.Name : string.Empty;

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Worksheets.Add("Translations");
                var worksheet = excelPackage.Workbook.Worksheets[1];
                var cells = worksheet.Cells;

                worksheet.Column(translationTypeColumn).Hidden = true;
                worksheet.Column(translationIndexColumn).Hidden = true;
                worksheet.Column(questionnaireEntityIdColumn).Hidden = true;
                cells[1, translationTypeColumn].Value = TranslationTypeColumnName;
                cells[1, translationIndexColumn].Value = "Index";
                cells[1, questionnaireEntityIdColumn].Value = EntityIdColumnName;
                cells[1, originalTextColumn].Value = "Original";
                cells[1, translationColumn].Value = "Translation";

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

                    var group = childNode as IGroup;
                    if (group != null)
                    {
                        for (int i = 0; i < group.FixedRosterTitles?.Length; i++)
                        {
                            var fixedRosterValue = group.FixedRosterTitles[i].Value;
                            var translatedOptionTitle = translation.GetFixedRosterTitle(group.PublicKey, fixedRosterValue);
                            var originalAnswerTitle = group.FixedRosterTitles[i].Title;

                            AppendTranslationRow(cells,
                              ref currentRowNumber,
                              TranslationType.FixedRosterTitle,
                              group.PublicKey,
                              originalAnswerTitle,
                              translatedOptionTitle,
                              fixedRosterValue.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }

                translationFile.ContentAsExcelFile = excelPackage.GetAsByteArray();

                return translationFile;
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
            cells[currentRowNumber, translationColumn].Value = translatedValue;
            cells[currentRowNumber, translationIndexColumn].Value = translationIndex;
            currentRowNumber++;
        }

        public void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation)
        {
            var language = translationId.FormatGuid();

            if (language == null) throw new ArgumentNullException(nameof(language));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            using (MemoryStream stream = new MemoryStream(excelRepresentation))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ValidatePackage(package);

                    var worksheet = package.Workbook.Worksheets[1];

                    var oldTranslations = this.translations.Query(
                        _ => _.Where(x => x.QuestionnaireId == questionnaireId && x.Language == language).ToList());
                    this.translations.Remove(oldTranslations);


                    for (int rowNumber = 2; ; rowNumber++)
                    {
                        if (worksheet.Cells[rowNumber, questionnaireEntityIdColumn].Value != null && worksheet.Cells[rowNumber, translationColumn].Value != null)
                        {
                            TranslationInstance instance = new TranslationInstance();

                            instance.QuestionnaireId = questionnaireId;
                            instance.Language = language;
                            instance.QuestionnaireEntityId = Guid.Parse(worksheet.Cells[rowNumber, questionnaireEntityIdColumn].GetValue<string>());
                            instance.Value = worksheet.Cells[rowNumber, translationColumn].GetValue<string>();
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

        public void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId)
        {
            var language = translationId.FormatGuid();
            var newLanguage = newTranslationId.FormatGuid();

            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.Language == language)
                .ToList());

            foreach (var storedTranslation in storedTranslations)
            {
                storedTranslation.Language = newLanguage;
                storedTranslation.QuestionnaireId = newQuestionnaireId;
                this.translations.Store(storedTranslation, storedTranslation);
            }
        }

        public void Delete(Guid questionnaireId, Guid translationId)
        {
            var language = translationId.FormatGuid();
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.Language == language)
                .ToList());
            this.translations.Remove(storedTranslations);
        }

        private void ValidatePackage(ExcelPackage package)
        {
            if (package.Workbook.Worksheets.Count == 0)
            {
                throw new InvalidExcelFileException("Excel file is empty - contains no worksheets");
            }

            List<TranslationValidationError> foundErrors = new List<TranslationValidationError>();
            var translationTypesWithIndex = new List<TranslationType>
            {
                TranslationType.FixedRosterTitle,
                TranslationType.OptionTitle,
                TranslationType.ValidationMessage
            };

            var worksheet = package.Workbook.Worksheets[1];
            for (int rowNumber = 2; ; rowNumber++)
            {
                if (foundErrors.Count > 10)
                {
                    break;
                }
                if (worksheet.Cells[rowNumber, questionnaireEntityIdColumn].Value != null)
                {
                    string entityId = worksheet.Cells[rowNumber, questionnaireEntityIdColumn].GetValue<string>();
                    Guid parsedId;
                    if (!Guid.TryParse(entityId, out parsedId))
                    {
                        foundErrors.Add(new TranslationValidationError
                        {
                            Message = $"{EntityIdColumnName} has invalid id at [{worksheet.Cells[rowNumber, questionnaireEntityIdColumn].Address}]",
                            ErrorAddress =  worksheet.Cells[rowNumber, questionnaireEntityIdColumn].Address 
                        });
                    }
                    string translationTypeString = worksheet.Cells[rowNumber, translationTypeColumn].GetValue<string>();
                    TranslationType type;
                    if (!Enum.TryParse(translationTypeString, out type) || type == TranslationType.Unknown)
                    {
                        foundErrors.Add(new TranslationValidationError
                        {
                            Message = $"{TranslationTypeColumnName} has invalid type [{worksheet.Cells[rowNumber, translationTypeColumn].Address}]",
                            ErrorAddress = worksheet.Cells[rowNumber, translationTypeColumn].Address
                        });
                    }

                    string translationIndex = worksheet.Cells[rowNumber, translationIndexColumn].GetValue<string>();
                    if (translationTypesWithIndex.Contains(type) && string.IsNullOrEmpty(translationIndex))
                    {
                        var errorAddress = worksheet.Cells[rowNumber, translationIndexColumn].Address;
                        foundErrors.Add(new TranslationValidationError
                        {
                            Message = $"{TranslationTypeColumnName} has invalid index at [{errorAddress}]",
                            ErrorAddress = errorAddress
                        });
                    }
                }
                else
                {
                    break;
                }
            }

            if (foundErrors.Count > 0)
            {
                throw new InvalidExcelFileException("Found errors in excel file")
                {
                    FoundErrors = foundErrors
                };
            }
        }
    }
}
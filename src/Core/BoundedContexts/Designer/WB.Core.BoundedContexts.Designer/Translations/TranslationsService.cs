using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
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
        private class TranslationRow
        {
            public string EntityId { get; set; }
            public string Type { get; set; }
            public string OptionValueOrValidationIndexOrFixedRosterId { get; set; }
            public string OriginalText { get; set; }
            public string Translation { get; set; }
            public string Sheet { get; set; } = WorksheetName;
        }

        private readonly TranslationType[] translationTypesWithIndexes =
        {
            TranslationType.FixedRosterTitle, TranslationType.OptionTitle, TranslationType.ValidationMessage
        };

        private const string EntityIdColumnName = "Entity Id";
        private const string TranslationTypeColumnName = "Type";
        private const string WorksheetName = "Translations";
        private const string OptionsWorksheetPreffix = "@@_";

        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;


        public TranslationsService(IPlainStorageAccessor<TranslationInstance> translations,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.translations = translations;
            this.questionnaireStorage = questionnaireStorage;
        }

        public ITranslation Get(Guid questionnaireId, Guid translationId)
        {
            var storedTranslations = this.translations.Query(
                _ => _.Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId).ToList())
                .Cast<TranslationDto>()
                .ToList();

            return new QuestionnaireTranslation(storedTranslations);
        }

        public TranslationFile GetAsExcelFile(Guid questionnaireId, Guid translationId) =>
            this.GetTranslationFileWithSpecifiedFormat(questionnaireId, translationId);

        public TranslationFile GetTemplateAsExcelFile(Guid questionnaireId) =>
            this.GetTemplateFileWithSpecifiedFormat(questionnaireId);

        private TranslationFile GetTranslationFileWithSpecifiedFormat(Guid questionnaireId, Guid translationId)
        {
            var translation = this.Get(questionnaireId, translationId);
            return this.GenerateTranslationFileWithGivenTranslation(questionnaireId, translationId, translation);
        }

        private TranslationFile GetTemplateFileWithSpecifiedFormat(Guid questionnaireId)
        {
            var translation = new QuestionnaireTranslation(new List<TranslationDto>());
            return this.GenerateTranslationFileWithGivenTranslation(questionnaireId, Guid.Empty, translation);
        }

        private TranslationFile GenerateTranslationFileWithGivenTranslation(Guid questionnaireId, Guid translationId, ITranslation translation)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());

            var translationFile = new TranslationFile
            {
                QuestionnaireTitle = questionnaire.Title,
                TranslationName = questionnaire.Translations.FirstOrDefault(x => x.Id == translationId)?.Name ?? string.Empty,
                ContentAsExcelFile = this.GetExcelFileContentEEPlus(questionnaire, translation)
            };

            return translationFile;
        }

        public void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation)
        {
            if (translationId == null) throw new ArgumentNullException(nameof(translationId));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            using (MemoryStream stream = new MemoryStream(excelRepresentation))
            {
                try
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        if (package.Workbook.Worksheets.Count == 0)
                        {
                            throw new InvalidExcelFileException("Excel file is empty - contains no worksheets");
                        }

                        var sheetsWithTranslation = package.Workbook.Worksheets
                            .Where(x => x.Name == WorksheetName || x.Name.StartsWith(OptionsWorksheetPreffix))
                            .ToList();

                        if (!sheetsWithTranslation.Any())
                            throw new InvalidExcelFileException("Worksheet with translations not found");

                        var translationErrors = this.Verify(sheetsWithTranslation).Take(10).ToList();
                        if (translationErrors.Any())
                            throw new InvalidExcelFileException("Found errors in excel file") { FoundErrors = translationErrors };

                        this.Delete(questionnaireId, translationId);
                        var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
                        HashSet<Guid> idsOfAllQuestionnaireEntities =
                            new HashSet<Guid>(questionnaire.Children.TreeToEnumerable(x => x.Children).Select(x => x.PublicKey));

                        var translationInstances = new List<TranslationInstance>();
                        foreach (var worksheet in sheetsWithTranslation)
                        {
                            var end = worksheet.Dimension.End.Row;

                            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
                            {
                                TranslationRow importedTranslation = GetExcelTranslation(worksheet.Cells, rowNumber);

                                if (string.IsNullOrWhiteSpace(importedTranslation.Translation)) continue;

                                var questionnaireEntityId = Guid.Parse(importedTranslation.EntityId);
                                if (!idsOfAllQuestionnaireEntities.Contains(questionnaireEntityId)) continue;

                                var translationInstance = new TranslationInstance
                                {
                                    QuestionnaireId = questionnaireId,
                                    TranslationId = translationId,
                                    QuestionnaireEntityId = questionnaireEntityId,
                                    Value = importedTranslation.Translation,
                                    TranslationIndex = importedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                                    Type = (TranslationType)Enum.Parse(typeof(TranslationType), importedTranslation.Type)
                                };

                                translationInstances.Add(translationInstance);
                            }
                        }

                        var uniqueTranslationInstances = translationInstances
                            .Distinct(new TranslationInstance.IdentityComparer())
                            .ToList();

                        foreach (var translationInstance in uniqueTranslationInstances)
                        {
                            this.translations.Store(translationInstance, translationInstance);
                        }
                    }
                }
                catch (COMException e)
                {
                    throw new InvalidExcelFileException("Failed to extract translations from uploaded file", e);
                }
            }
        }

        private IEnumerable<TranslationValidationError> Verify(IEnumerable<ExcelWorksheet> sheetsWithTranslation)
        {
            var errors = new List<TranslationValidationError>();
            foreach (var worksheet in sheetsWithTranslation)
            {
                errors.AddRange(this.Verify(worksheet).Take(10).ToList());
                if (errors.Count >= 10)
                    return errors;
            }
            return errors;
        }

        public void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId)
        {
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                .ToList());

            foreach (var storedTranslation in storedTranslations)
            {
                var translationCopy = storedTranslation.Clone();
                translationCopy.TranslationId = newTranslationId;
                translationCopy.QuestionnaireId = newQuestionnaireId;
                this.translations.Store(translationCopy, translationCopy);
            }
        }

        public void Delete(Guid questionnaireId, Guid translationId)
        {
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                .ToList());
            this.translations.Remove(storedTranslations);
        }

        public int Count(Guid questionnaireId, Guid translationId)
            => this.translations.Query(_ => _.Count(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId));

        private TranslationRow GetExcelTranslation(ExcelRange cells, int rowNumber) => new TranslationRow
        {
            EntityId = cells[$"A{rowNumber}"].GetValue<string>(),
            Type = cells[$"B{rowNumber}"].GetValue<string>(),
            OptionValueOrValidationIndexOrFixedRosterId = cells[$"C{rowNumber}"].GetValue<string>(),
            Translation = cells[$"E{rowNumber}"].GetValue<string>()
        };

        private IEnumerable<TranslationValidationError> Verify(ExcelWorksheet worksheet)
        {
            var end = worksheet.Dimension.End.Row;

            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
            {
                var importedTranslation = GetExcelTranslation(worksheet.Cells, rowNumber);

                Guid entityId;
                if (!Guid.TryParse(importedTranslation.EntityId, out entityId))
                {
                    var cellAddress = $"A{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = $"{EntityIdColumnName} has invalid id at [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }

                TranslationType importedType;
                if (!Enum.TryParse(importedTranslation.Type, out importedType) || importedType == TranslationType.Unknown)
                {
                    var cellAddress = $"B{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = $"{TranslationTypeColumnName} has invalid type [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }

                if (translationTypesWithIndexes.Contains(importedType) && string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = $"C{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = $"{TranslationTypeColumnName} has invalid index at [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }
            }
        }

        private byte[] GetExcelFileContentEEPlus(QuestionnaireDocument questionnaire, ITranslation translation)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var textsToTranslateGroupedBySheets = this.GetTranlsatedTexts(questionnaire, translation)
                    .OrderByDescending(x => x.Sheet)
                    .GroupBy(x => x.Sheet)
                    .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var textsToTranslate in textsToTranslateGroupedBySheets)
                {
                    string workSheetName = this.GenerateWorksheetName(excelPackage.Workbook.Worksheets.Select(sheet => sheet.Name).ToList(),
                            textsToTranslate.Key);

                    var worksheet = excelPackage.Workbook.Worksheets.Add(workSheetName);

                    worksheet.Cells["A1"].Value = EntityIdColumnName;
                    worksheet.Cells["B1"].Value = TranslationTypeColumnName;
                    worksheet.Cells["C1"].Value = "Index";
                    worksheet.Cells["D1"].Value = "Original text";
                    worksheet.Cells["E1"].Value = "Translation";

                    FormatCell(worksheet, "A1");
                    FormatCell(worksheet, "B1");
                    FormatCell(worksheet, "C1");
                    FormatCell(worksheet, "D1");
                    FormatCell(worksheet, "E1");

                    int currentRowNumber = 1;

                    foreach (var translationRow in textsToTranslate.Value)
                    {
                        if (string.IsNullOrWhiteSpace(translationRow.OriginalText)) continue;

                        currentRowNumber++;

                        worksheet.Cells[$"A{currentRowNumber}"].Value = translationRow.EntityId;
                        worksheet.Cells[$"A{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"B{currentRowNumber}"].Value = translationRow.Type;
                        worksheet.Cells[$"B{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"C{currentRowNumber}"].Value =
                            translationRow.OptionValueOrValidationIndexOrFixedRosterId;
                        worksheet.Cells[$"C{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"D{currentRowNumber}"].Value = CleanUpString(translationRow.OriginalText);
                        worksheet.Cells[$"D{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"E{currentRowNumber}"].Value = CleanUpString(translationRow.Translation);
                        worksheet.Cells[$"E{currentRowNumber}"].Style.WrapText = true;
                    }

                    for (int i = 1; i <= 4; i++)
                    {
                        LockAndAutofitColumn(worksheet, i);
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    worksheet.Column(5).AutoFit();
                    worksheet.Protection.AllowFormatColumns = true;
                }

                if (excelPackage.Workbook.Worksheets.Count == 0)
                    excelPackage.Workbook.Worksheets.Add(WorksheetName);

                return excelPackage.GetAsByteArray();
            }
        }

        private string GenerateWorksheetName(List<string> addedWorksheetNames, string newWorksheetName)
        {
            if (!newWorksheetName.StartsWith(OptionsWorksheetPreffix)) return newWorksheetName;

            newWorksheetName = newWorksheetName.Substring(0, newWorksheetName.Length > 31 ? 31 : newWorksheetName.Length);

            if(!addedWorksheetNames.Contains(newWorksheetName)) return newWorksheetName;

            newWorksheetName = newWorksheetName.Substring(0, newWorksheetName.Length > 28 ? 28 : newWorksheetName.Length);

            return $"{newWorksheetName}_{(addedWorksheetNames.Count + 1).ToString("D2")}";
        }

        private string CleanUpString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            return new string(text.Where(c => char.IsWhiteSpace(c) || !char.IsControl(c)).ToArray());
        }

        private static void LockAndAutofitColumn(ExcelWorksheet worksheet, int i)
        {
            worksheet.Column(i).Style.Locked = true;
            worksheet.Column(i).Style.WrapText = true;
            worksheet.Column(i).AutoFit();
        }

        private void FormatCell(ExcelWorksheet worksheet, string address)
        {
            var cell = worksheet.Cells[address];
            cell.Style.Font.Bold = true;
        }

        private IEnumerable<TranslationRow> GetTranlsatedTexts(QuestionnaireDocument questionnaire, ITranslation translation)
        {
            foreach (var entity in questionnaire.Children.TreeToEnumerable(x => x.Children))
            {
                yield return GetTranslatedTitle(entity, translation);

                var group = entity as IGroup;
                var question = entity as IQuestion;
                var validatable = entity as IValidatable;

                if (validatable != null)
                    foreach (var translatedValidationMessage in GetTranslatedValidationMessages(validatable, translation))
                        yield return translatedValidationMessage;

                if (question != null)
                {
                    if (!string.IsNullOrEmpty(question.Instructions))
                        yield return GetTranslatedInstrution(question, translation);

                    foreach (var translatedOption in GetTranslatedOptions(question, translation))
                        yield return translatedOption;
                }

                if (group != null)
                    foreach (var translatedRosterTitle in GetTranslatedRosterTitles(@group, translation))
                        yield return translatedRosterTitle;
            }
        }

        private static TranslationRow GetTranslatedTitle(IComposite entity, ITranslation translation) => new TranslationRow
        {
            EntityId = entity.PublicKey.FormatGuid(),
            Type = TranslationType.Title.ToString("G"),
            OriginalText = entity.GetTitle(),
            Translation = translation.GetTitle(entity.PublicKey)
        };

        private static TranslationRow GetTranslatedInstrution(IQuestion question, ITranslation translation) => new TranslationRow
        {
            EntityId = question.PublicKey.FormatGuid(),
            Type = TranslationType.Instruction.ToString("G"),
            OriginalText = question.Instructions,
            Translation = translation.GetInstruction(question.PublicKey)
        };

        private static IEnumerable<TranslationRow> GetTranslatedValidationMessages(IValidatable validatable, ITranslation translation)
            => from validationCondition in validatable.ValidationConditions
               let validationIndex = validatable.ValidationConditions.IndexOf(validationCondition) + 1
               select new TranslationRow
               {
                   EntityId = validatable.PublicKey.FormatGuid(),
                   Type = TranslationType.ValidationMessage.ToString("G"),
                   OriginalText = validationCondition.Message,
                   Translation = translation.GetValidationMessage(validatable.PublicKey, validationIndex),
                   OptionValueOrValidationIndexOrFixedRosterId = validationIndex.ToString()
               };

        private static IEnumerable<TranslationRow> GetTranslatedOptions(IQuestion question, ITranslation translation)
        {
            var singleOptionQuestion = question as SingleQuestion;

            var isLongOptionsList = (singleOptionQuestion?.CascadeFromQuestionId.HasValue ?? false) || (singleOptionQuestion?.IsFilteredCombobox ?? false);

            return from option in question.Answers
                   select new TranslationRow
                   {
                       EntityId = question.PublicKey.FormatGuid(),
                       Type = TranslationType.OptionTitle.ToString("G"),
                       OriginalText = option.AnswerText,
                       Translation = translation.GetAnswerOption(question.PublicKey, option.AnswerValue),
                       OptionValueOrValidationIndexOrFixedRosterId = option.AnswerValue,
                       Sheet = isLongOptionsList ? $"{OptionsWorksheetPreffix}{question.StataExportCaption}" : WorksheetName
                   };
        }

        private static IEnumerable<TranslationRow> GetTranslatedRosterTitles(IGroup @group, ITranslation translation)
            => from fixedRoster in @group.FixedRosterTitles
               select new TranslationRow
               {
                   EntityId = @group.PublicKey.FormatGuid(),
                   Type = TranslationType.FixedRosterTitle.ToString("G"),
                   OriginalText = fixedRoster.Title,
                   Translation = translation.GetFixedRosterTitle(@group.PublicKey, fixedRoster.Value),
                   OptionValueOrValidationIndexOrFixedRosterId = fixedRoster.Value.ToString("F0", CultureInfo.InvariantCulture)
               };
    }
}
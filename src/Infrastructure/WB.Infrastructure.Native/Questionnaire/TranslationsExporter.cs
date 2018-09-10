using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using OfficeOpenXml;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class TranslationsExportService : ITranslationsExportService
    {
        private class TranslationRow
        {
            public string EntityId { get; set; }
            public string Variable { get; set; }
            public string Type { get; set; }
            public string OptionValueOrValidationIndexOrFixedRosterId { get; set; }
            public string OriginalText { get; set; }
            public string Translation { get; set; }
            public string Sheet { get; set; } = TranslationExcelOptions.WorksheetName;
        }
        
        public TranslationFile GenerateTranslationFile(QuestionnaireDocument questionnaire, Guid translationId, ITranslation translation = null)
        {
            var translationFile = new TranslationFile
            {
                QuestionnaireTitle = questionnaire.Title,
                TranslationName = questionnaire.Translations.FirstOrDefault(x => x.Id == translationId)?.Name ?? string.Empty,
                ContentAsExcelFile = this.GetExcelFileContentEEPlus(questionnaire, translation ?? new QuestionnaireTranslation(new List<TranslationDto>()))
            };

            return translationFile;
        }
        
        private byte[] GetExcelFileContentEEPlus(QuestionnaireDocument questionnaire, ITranslation translation)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                var textsToTranslateGroupedBySheets = GetTranlsatedTexts(questionnaire, translation)
                    .OrderByDescending(x => x.Sheet)
                    .GroupBy(x => x.Sheet)
                    .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var textsToTranslate in textsToTranslateGroupedBySheets)
                {
                    string workSheetName = this.GenerateWorksheetName(excelPackage.Workbook.Worksheets.Select(sheet => sheet.Name).ToList(),
                        textsToTranslate.Key);

                    var worksheet = excelPackage.Workbook.Worksheets.Add(workSheetName);

                    worksheet.Cells["A1"].Value = TranslationExcelOptions.EntityIdColumnName;
                    worksheet.Cells["B1"].Value = "Variable";
                    worksheet.Cells["C1"].Value = TranslationExcelOptions.TranslationTypeColumnName;
                    worksheet.Cells["D1"].Value = TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName;
                    worksheet.Cells["E1"].Value = "Original text";
                    worksheet.Cells["F1"].Value = TranslationExcelOptions.TranslationTextColumnName;

                    void FormatCell(string address)
                    {
                        var cell = worksheet.Cells[address];
                        cell.Style.Font.Bold = true;
                    }

                    FormatCell("A1");
                    FormatCell("B1");
                    FormatCell("C1");
                    FormatCell("D1");
                    FormatCell("E1");
                    FormatCell("F1");

                    int currentRowNumber = 1;

                    foreach (var translationRow in textsToTranslate.Value)
                    {
                        if (string.IsNullOrWhiteSpace(translationRow.OriginalText)) continue;

                        currentRowNumber++;

                        worksheet.Cells[$"A{currentRowNumber}"].Value = translationRow.EntityId;
                        worksheet.Cells[$"A{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"B{currentRowNumber}"].Value = translationRow.Variable;
                        worksheet.Cells[$"B{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"C{currentRowNumber}"].Value = translationRow.Type;
                        worksheet.Cells[$"C{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"D{currentRowNumber}"].Value = translationRow.OptionValueOrValidationIndexOrFixedRosterId;
                        worksheet.Cells[$"D{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"E{currentRowNumber}"].Value = CleanUpString(translationRow.OriginalText);
                        worksheet.Cells[$"E{currentRowNumber}"].Style.WrapText = true;
                        worksheet.Cells[$"F{currentRowNumber}"].Value = CleanUpString(translationRow.Translation);
                        worksheet.Cells[$"F{currentRowNumber}"].Style.WrapText = true;
                    }

                    for (int i = 1; i <= 5; i++)
                    {
                        LockAndAutofitColumn(worksheet, i);
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    worksheet.Column(6).AutoFit();
                    worksheet.Protection.AllowFormatColumns = true;
                }

                if (excelPackage.Workbook.Worksheets.Count == 0)
                    excelPackage.Workbook.Worksheets.Add(TranslationExcelOptions.WorksheetName);

                return excelPackage.GetAsByteArray();
            }
        }

        private string GenerateWorksheetName(List<string> addedWorksheetNames, string newWorksheetName)
        {
            if (!newWorksheetName.StartsWith(TranslationExcelOptions.OptionsWorksheetPreffix)) return newWorksheetName;

            newWorksheetName = newWorksheetName.Substring(0, newWorksheetName.Length > 31 ? 31 : newWorksheetName.Length);

            if (!addedWorksheetNames.Contains(newWorksheetName)) return newWorksheetName;

            newWorksheetName = newWorksheetName.Substring(0, newWorksheetName.Length > 28 ? 28 : newWorksheetName.Length);

            return $"{newWorksheetName}_{(addedWorksheetNames.Count + 1):D2}";
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
                        yield return GetTranslatedInstruction(question, translation);

                    foreach (var translatedOption in GetTranslatedOptions(question, translation))
                        yield return translatedOption;
                }

                if (group != null)
                    foreach (var translatedRosterTitle in GetTranslatedRosterTitles(group, translation))
                        yield return translatedRosterTitle;
            }
        }

        private static TranslationRow GetTranslatedTitle(IComposite entity, ITranslation translation) => new TranslationRow
        {
            EntityId = entity.PublicKey.FormatGuid(),
            Variable = entity.VariableName,
            Type = TranslationType.Title.ToString("G"),
            OriginalText = entity.GetTitle(),
            Translation = translation.GetTitle(entity.PublicKey)
        };

        private static TranslationRow GetTranslatedInstruction(IQuestion question, ITranslation translation) => new TranslationRow
        {
            EntityId = question.PublicKey.FormatGuid(),
            Variable = question.VariableName,
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
                    Variable = (validatable as IComposite)?.VariableName,
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
                    Variable = question.VariableName,
                    Type = question.QuestionType == QuestionType.Numeric ? TranslationType.SpecialValue.ToString("G") : TranslationType.OptionTitle.ToString("G"),
                    OriginalText = option.AnswerText,
                    Translation = question.QuestionType == QuestionType.Numeric ? translation.GetSpecialValue(question.PublicKey, option.AnswerValue) : translation.GetAnswerOption(question.PublicKey, option.AnswerValue),
                    OptionValueOrValidationIndexOrFixedRosterId = option.AnswerValue,
                    Sheet = isLongOptionsList ? $"{TranslationExcelOptions.OptionsWorksheetPreffix}{question.StataExportCaption}" : TranslationExcelOptions.WorksheetName
                };
        }

        private static IEnumerable<TranslationRow> GetTranslatedRosterTitles(IGroup group, ITranslation translation)
            => from fixedRoster in @group.FixedRosterTitles
                select new TranslationRow
                {
                    EntityId = @group.PublicKey.FormatGuid(),
                    Variable = @group.VariableName,
                    Type = TranslationType.FixedRosterTitle.ToString("G"),
                    OriginalText = fixedRoster.Title,
                    Translation = translation.GetFixedRosterTitle(@group.PublicKey, fixedRoster.Value),
                    OptionValueOrValidationIndexOrFixedRosterId = fixedRoster.Value.ToString("F0", CultureInfo.InvariantCulture)
                };
    }
}

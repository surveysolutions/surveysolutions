using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;

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
        
        public TranslationFile GenerateTranslationFile(QuestionnaireDocument questionnaire, Guid translationId, ITranslation translation, ICategories categoriesService)
        {
            var translationFile = new TranslationFile
            (
                questionnaireTitle : questionnaire.Title,
                translationName : questionnaire.Translations.FirstOrDefault(x => x.Id == translationId)?.Name ?? string.Empty,
                contentAsExcelFile : this.GetExcelFileContentEEPlus(questionnaire, 
                    translation ?? new QuestionnaireTranslation(new List<TranslationDto>()),  categoriesService)
            );

            return translationFile;
        }
        
        private byte[] GetExcelFileContentEEPlus(QuestionnaireDocument questionnaire, ITranslation translation, ICategories categoriesService)
        {
            using (XLWorkbook excelPackage = new XLWorkbook())
            {
                var textsToTranslateGroupedBySheets = GetTranslatedTexts(questionnaire, translation, categoriesService)
                    .OrderByDescending(x => x.Sheet)
                    .GroupBy(x => x.Sheet)
                    .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var textsToTranslate in textsToTranslateGroupedBySheets)
                {
                    string workSheetName = this.GenerateWorksheetName(excelPackage.Worksheets.Select(sheet => sheet.Name).ToList(),
                        textsToTranslate.Key);

                    IXLWorksheet worksheet = excelPackage.Worksheets.Add(workSheetName);

                    worksheet.Cell("A1").SetValue(TranslationExcelOptions.EntityIdColumnName);
                    worksheet.Cell("B1").SetValue("Variable");
                    worksheet.Cell("C1").SetValue(TranslationExcelOptions.TranslationTypeColumnName);
                    worksheet.Cell("D1").SetValue(TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName);
                    worksheet.Cell("E1").SetValue("Original text");
                    worksheet.Cell("F1").SetValue(TranslationExcelOptions.TranslationTextColumnName);
                    
                    void FormatCell(string address)
                    {
                        var cell = worksheet.Cell(address);
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
                        worksheet.Cell($"A{currentRowNumber}").SetValue(translationRow.EntityId);
                        worksheet.Cell($"B{currentRowNumber}").SetValue(translationRow.Variable);
                        worksheet.Cell($"C{currentRowNumber}").SetValue(translationRow.Type);
                        worksheet.Cell($"D{currentRowNumber}").SetValue(translationRow.OptionValueOrValidationIndexOrFixedRosterId);
                        worksheet.Cell($"E{currentRowNumber}").SetValue(CleanUpString(translationRow.OriginalText));
                        worksheet.Cell($"F{currentRowNumber}").SetValue(CleanUpString(translationRow.Translation));
                    }

                    for (int i = 1; i <= 5; i++)
                    {
                        LockAndAutofitColumn(worksheet, i);
                    }
                }

                if (excelPackage.Worksheets.Count == 0)
                    excelPackage.Worksheets.Add(TranslationExcelOptions.WorksheetName);

                var stream = new MemoryStream();
                excelPackage.SaveAs(stream);
                
                return stream.ToArray();
            }
        }

        private string GenerateWorksheetName(List<string> addedWorksheetNames, string newWorksheetName)
        {
            if (!newWorksheetName.StartsWith(TranslationExcelOptions.OptionsWorksheetPreffix) &&
                !newWorksheetName.StartsWith(TranslationExcelOptions.CategoriesWorksheetPreffix)) return newWorksheetName;

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

        private static void LockAndAutofitColumn(IXLWorksheet worksheet, int i)
        {
            var xlColumn = worksheet.Column(i);
            xlColumn.Style.Protection.Locked = true;
            xlColumn.Style.Alignment.WrapText = true;
            xlColumn.AdjustToContents();
        }

        private IEnumerable<TranslationRow> GetTranslatedTexts(QuestionnaireDocument questionnaire, ITranslation translation, ICategories categoriesService)
        {
            yield return GetTranslatedTitle(questionnaire, translation);
            
            foreach (var entity in questionnaire.Children.TreeToEnumerable(x => x.Children))
            {
                yield return GetTranslatedTitle(entity, translation);

                var group = entity as IGroup;
                var question = entity as IQuestion;

                if (entity is IValidatable validatable)
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

            foreach (var categories in questionnaire.Categories)
            {
                foreach (var translatedOption in GetTranslatedOptions(categories, translation, categoriesService))
                    yield return translatedOption;
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
            if (question is ICategoricalQuestion {CategoriesId: { }})
                return Enumerable.Empty<TranslationRow>();
            
            var singleOptionQuestion = question as SingleQuestion;

            var isLongOptionsList = (question?.CascadeFromQuestionId.HasValue ?? false) || (singleOptionQuestion?.IsFilteredCombobox ?? false);
            
            return from option in question.Answers
                select new TranslationRow
                {
                    EntityId = question.PublicKey.FormatGuid(),
                    Variable = question.VariableName,
                    Type = question.QuestionType == QuestionType.Numeric ? TranslationType.SpecialValue.ToString("G") : TranslationType.OptionTitle.ToString("G"),
                    OriginalText = option.AnswerText,
                    Translation = question.QuestionType == QuestionType.Numeric ? translation.GetSpecialValue(question.PublicKey, option.AnswerValue) : translation.GetAnswerOption(question.PublicKey, option.AnswerValue, option.ParentValue),
                    OptionValueOrValidationIndexOrFixedRosterId = option.ParentValue == null ? option.AnswerValue : $"{option.AnswerValue}${option.ParentValue}",
                    Sheet = isLongOptionsList ? $"{TranslationExcelOptions.OptionsWorksheetPreffix}{question.StataExportCaption}" : TranslationExcelOptions.WorksheetName
                };
        }

        private IEnumerable<TranslationRow> GetTranslatedOptions(Categories categories, ITranslation translation, ICategories categoriesService) =>
            categoriesService.GetCategories(categories.Id).Select(x =>
                new TranslationRow
                {
                    OriginalText = x.Text,
                    Translation = translation.GetCategoriesText(categories.Id, x.Id, x.ParentId),
                    OptionValueOrValidationIndexOrFixedRosterId = x.ParentId == null ? $"{x.Id}" : $"{x.Id}${x.ParentId}",
                    Sheet = $"{TranslationExcelOptions.CategoriesWorksheetPreffix}{categories.Name}"
                });

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

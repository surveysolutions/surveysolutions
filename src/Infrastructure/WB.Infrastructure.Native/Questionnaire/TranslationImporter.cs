using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using SixLabors.Fonts;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.Infrastructure.Native.Questionnaire
{
    public class TranslationImporter : ITranslationImporter
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

        public List<TranslationInstance> GetTranslationInstancesFromExcelFile(QuestionnaireDocument questionnaire,
                    QuestionnaireIdentity questionnaireIdentity, Guid translationId, byte[] excelRepresentation)
        {

            List<TranslationInstance> translations = new List<TranslationInstance>();
            using MemoryStream stream = new MemoryStream(excelRepresentation);

            try
            {
                //non windows fonts
                var firstFont = SystemFonts.Collection.Families.First();
                var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
                
                using var package = new XLWorkbook(stream, loadOptions);

                if (package.Worksheets.Count == 0)
                    throw new InvalidOperationException("Translation file is empty");

                var sheetsWithTranslation = package.Worksheets
                    .Where(x => x.Name == TranslationExcelOptions.WorksheetName ||
                                x.Name.StartsWith(TranslationExcelOptions.OptionsWorksheetPreffix) ||
                                (x.Name.StartsWith(TranslationExcelOptions.CategoriesWorksheetPreffix) &&
                                 questionnaire.Categories.Any(y =>
                                     y.Name.ToLower() == x.Name.ToLower().TrimStart(TranslationExcelOptions.CategoriesWorksheetPreffix))))
                    .ToList();

                if (!sheetsWithTranslation.Any())
                    throw new InvalidOperationException("Translation is missing.");

                var translationsWithHeaderMap = sheetsWithTranslation.Select(CreateHeaderMap).ToList();
                var idsOfAllQuestionnaireEntities = questionnaire.Children.TreeToEnumerable(x => x.Children)
                    .ToDictionary(composite => composite.PublicKey, x => x is Group);

                var translationInstances = new List<TranslationInstance>();
                foreach (var translationWithHeaderMap in translationsWithHeaderMap)
                {
                    var worksheetTranslations = GetWorksheetTranslations(translationWithHeaderMap,
                        questionnaire, idsOfAllQuestionnaireEntities, questionnaireIdentity, translationId);
                    translationInstances.AddRange(worksheetTranslations);
                }

                var uniqueTranslationInstances = translationInstances
                    .Distinct(new TranslationInstance.IdentityComparer())
                    .ToList();

                foreach (var translationInstance in uniqueTranslationInstances)
                {
                    translations.Add(translationInstance);
                }
            }
            catch (NullReferenceException e)
            {
                throw new InvalidOperationException("Translation cannot be extracted.", e);
            }
            catch (InvalidDataException e)
            {
                throw new InvalidOperationException("Translation cannot be extracted.", e);
            }
            catch (COMException e)
            {
                throw new InvalidOperationException("Translation cannot be extracted.", e);
            }

            return translations;
        }

        private IEnumerable<TranslationInstance> GetWorksheetTranslations(
            TranslationsWithHeaderMap translationWithHeaderMap, QuestionnaireDocument questionnaire,
            Dictionary<Guid, bool> idsOfAllQuestionnaireEntities, QuestionnaireIdentity questionnaireIdentity, Guid translationId)
        {
            var worksheet = translationWithHeaderMap.Worksheet;
            var worksheetName = worksheet.Name.ToLower();
            var end = worksheet.LastRowUsed().RowNumber();

            var isCategoriesWorksheet = worksheetName.StartsWith(TranslationExcelOptions.CategoriesWorksheetPreffix);
            var categoriesWorksheetName = isCategoriesWorksheet
                ? worksheetName.TrimStart(TranslationExcelOptions.CategoriesWorksheetPreffix)
                : null;
            var categoriesId = isCategoriesWorksheet
                ? questionnaire.Categories.Single(x => x.Name.ToLower() == categoriesWorksheetName).Id
                : (Guid?)null;

            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
            {
                TranslationRow importedTranslation = GetExcelTranslation(translationWithHeaderMap, rowNumber);

                if (string.IsNullOrWhiteSpace(importedTranslation.Translation)) continue;

                var translationInstance = categoriesId.HasValue
                    ? GetCategoriesTranslation(questionnaireIdentity, translationId, categoriesId.Value, importedTranslation)
                    : GetQuestionnaireTranslation(questionnaireIdentity, translationId,
                        importedTranslation, idsOfAllQuestionnaireEntities);

                if (translationInstance != null)
                    yield return translationInstance;
            }
        }

        private TranslationRow GetExcelTranslation(TranslationsWithHeaderMap worksheetWithHeadersMap, int rowNumber)
        {
            var entityId = worksheetWithHeadersMap.Worksheet.Cell($"{worksheetWithHeadersMap.EntityIdIndex}{rowNumber}")
                ?.GetValue<string>();
            var type = worksheetWithHeadersMap.Worksheet.Cell($"{worksheetWithHeadersMap.TypeIndex}{rowNumber}")
                ?.GetString();
            var optionValueOrValidationIndexOrFixedRosterId = worksheetWithHeadersMap.Worksheet
                .Cell($"{worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex}{rowNumber}")
                ?.GetString();
            var translation = worksheetWithHeadersMap.Worksheet
                .Cell($"{worksheetWithHeadersMap.TranslationIndex}{rowNumber}")?.GetString();
            return this.AdjustIndexValue(new TranslationRow
            {
                EntityId = entityId.NullIfEmpty(),
                Type = type.NullIfEmpty(),
                OptionValueOrValidationIndexOrFixedRosterId = optionValueOrValidationIndexOrFixedRosterId.NullIfEmpty(),
                Translation = translation.NullIfEmpty()
            });
        }

        private TranslationRow AdjustIndexValue(TranslationRow row)
        {
            row.OptionValueOrValidationIndexOrFixedRosterId =
                row.OptionValueOrValidationIndexOrFixedRosterId?.TrimEnd('$');
            return row;
        }

        private TranslationInstance GetQuestionnaireTranslation(QuestionnaireIdentity questionnaireIdentity,
            Guid translationId, TranslationRow extractedTranslation,
            Dictionary<Guid, bool> idsOfAllQuestionnaireEntities)
        {
            var questionnaireEntityId = Guid.Parse(extractedTranslation.EntityId);
            if (!idsOfAllQuestionnaireEntities.Keys.Contains(questionnaireEntityId)) return null;

            var translationType = (TranslationType)Enum.Parse(typeof(TranslationType), extractedTranslation.Type);

            return new TranslationInstance
            {
                //Id = Guid.NewGuid(),
                QuestionnaireId = questionnaireIdentity,
                TranslationId = translationId,
                QuestionnaireEntityId = questionnaireEntityId,
                Value = extractedTranslation.Translation,
                TranslationIndex = extractedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                Type = translationType
            };
        }

        private TranslationInstance GetCategoriesTranslation(QuestionnaireIdentity questionnaireIdentity, Guid translationId,
            Guid categoriesId, TranslationRow extractedTranslation) =>
            new TranslationInstance
            {
                        //Id = Guid.NewGuid(),
                        QuestionnaireId = questionnaireIdentity,
                TranslationId = translationId,
                QuestionnaireEntityId = categoriesId,
                Value = extractedTranslation.Translation,
                TranslationIndex = extractedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                Type = TranslationType.Categories
            };


        private TranslationsWithHeaderMap CreateHeaderMap(IXLWorksheet worksheet)
        {
            var items = new List<Tuple<string, string>>()
                    {
                        new Tuple<string, string>(worksheet.Cell(1, "A").GetString(), "A"),
                        new Tuple<string, string>(worksheet.Cell(1, "B").GetString(), "B"),
                        new Tuple<string, string>(worksheet.Cell(1, "C").GetString(), "C"),
                        new Tuple<string, string>(worksheet.Cell(1, "D").GetString(), "D"),
                        new Tuple<string, string>(worksheet.Cell(1, "E").GetString(), "E"),
                        new Tuple<string, string>(worksheet.Cell(1, "F").GetString(), "F"),
                    }.Where(kv => !string.IsNullOrEmpty(kv.Item1));
            var headers = items.ToDictionary(k => k.Item1.Trim(), v => v.Item2);
            return new TranslationsWithHeaderMap(worksheet)
            {
                EntityIdIndex = headers.GetOrNull(TranslationExcelOptions.EntityIdColumnName),
                TypeIndex = headers.GetOrNull(TranslationExcelOptions.TranslationTypeColumnName),
                OptionValueOrValidationIndexOrFixedRosterIdIndex = headers.GetOrNull(TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName),
                TranslationIndex = headers.GetOrNull(TranslationExcelOptions.TranslationTextColumnName),
            };
        }

        private class TranslationsWithHeaderMap
        {
            public TranslationsWithHeaderMap(IXLWorksheet worksheet)
            {
                Worksheet = worksheet;
            }
            public IXLWorksheet Worksheet { get; set; }
            public string EntityIdIndex { get; set; }
            public string TypeIndex { get; set; }
            public string OptionValueOrValidationIndexOrFixedRosterIdIndex { get; set; }
            public string TranslationIndex { get; set; }
        }
    }

    public interface ITranslationImporter
    {
        List<TranslationInstance> GetTranslationInstancesFromExcelFile(QuestionnaireDocument questionnaire,
            QuestionnaireIdentity questionnaireIdentity, Guid translationId, byte[] excelRepresentation);
    }
}

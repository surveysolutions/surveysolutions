using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
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
            public string Translation { get; set; }
        }

        private class TranslationsWithHeaderMap
        {
            public ExcelWorksheet Worksheet { get; set; }
            public string EntityIdIndex { get; set; }
            public string TypeIndex { get; set; }
            public string OptionValueOrValidationIndexOrFixedRosterIdIndex { get; set; }
            public string TranslationIndex { get; set; }
        }

        private readonly TranslationType[] translationTypesWithIndexes =
        {
            TranslationType.FixedRosterTitle,
            TranslationType.OptionTitle,
            TranslationType.ValidationMessage,
            TranslationType.SpecialValue
        };
        
        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly ITranslationsExportService translationsExportService;
        
        public TranslationsService(IPlainStorageAccessor<TranslationInstance> translations,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            ITranslationsExportService translationsExportService)
        {
            this.translations = translations;
            this.questionnaireStorage = questionnaireStorage;
            this.translationsExportService = translationsExportService;
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
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            var translation = this.Get(questionnaireId, translationId);
            return translationsExportService.GenerateTranslationFile(questionnaire, translationId, translation);
        }

        private TranslationFile GetTemplateFileWithSpecifiedFormat(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            var translation = new QuestionnaireTranslation(new List<TranslationDto>());
            return translationsExportService.GenerateTranslationFile(questionnaire, Guid.Empty, translation);
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
                            throw new InvalidExcelFileException(ExceptionMessages.TranslationFileIsEmpty);
                        }

                        var sheetsWithTranslation = package.Workbook.Worksheets
                            .Where(x => x.Name == TranslationExcelOptions.WorksheetName || x.Name.StartsWith(TranslationExcelOptions.OptionsWorksheetPreffix))
                            .ToList();

                        if (!sheetsWithTranslation.Any())
                            throw new InvalidExcelFileException(ExceptionMessages.TranslationWorksheetIsMissing);

                        var translationsWithHeaderMap = sheetsWithTranslation.Select(CreateHeaderMap).ToList();

                        var translationErrors = this.Verify(translationsWithHeaderMap).Take(10).ToList();
                        if (translationErrors.Any())
                            throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) { FoundErrors = translationErrors };

                        var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
                        Dictionary<Guid, bool> idsOfAllQuestionnaireEntities =
                            questionnaire.Children.TreeToEnumerable(x => x.Children).ToDictionary(composite => composite.PublicKey, x=>x is Group);

                        var translationInstances = new List<TranslationInstance>();
                        foreach (var translationWithHeaderMap in translationsWithHeaderMap)
                        {
                            var worksheet = translationWithHeaderMap.Worksheet;
                            var end = worksheet.Dimension.End.Row;

                            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
                            {
                                TranslationRow importedTranslation = GetExcelTranslation(translationWithHeaderMap, rowNumber);

                                if (string.IsNullOrWhiteSpace(importedTranslation.Translation)) continue;

                                var questionnaireEntityId = Guid.Parse(importedTranslation.EntityId);
                                if (!idsOfAllQuestionnaireEntities.Keys.Contains(questionnaireEntityId)) continue;

                                var translationType = (TranslationType) Enum.Parse(typeof(TranslationType), importedTranslation.Type);

                                var cleanedValue = this.GetCleanedValue(translationType, idsOfAllQuestionnaireEntities[questionnaireEntityId], importedTranslation.Translation);

                                var translationInstance = new TranslationInstance
                                {
                                    QuestionnaireId = questionnaireId,
                                    TranslationId = translationId,
                                    QuestionnaireEntityId = questionnaireEntityId,
                                    Value = cleanedValue,
                                    TranslationIndex = importedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                                    Type = translationType
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

                        this.translations.Flush();
                    }
                }
                catch (NullReferenceException e)
                {
                    throw new InvalidExcelFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
                }
                catch (InvalidDataException e)
                {
                    throw new InvalidExcelFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
                }
                catch (COMException e)
                {
                    throw new InvalidExcelFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
                }
            }
        }

        private TranslationsWithHeaderMap CreateHeaderMap(ExcelWorksheet worksheet)
        {
            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(worksheet.Cells["A1"].GetValue<string>(), "A"),
                new Tuple<string, string>(worksheet.Cells["B1"].GetValue<string>(), "B"),
                new Tuple<string, string>(worksheet.Cells["C1"].GetValue<string>(), "C"),
                new Tuple<string, string>(worksheet.Cells["D1"].GetValue<string>(), "D"),
                new Tuple<string, string>(worksheet.Cells["E1"].GetValue<string>(), "E"),
                new Tuple<string, string>(worksheet.Cells["F1"].GetValue<string>(), "F"),
            }.Where(kv => kv.Item1 != null).ToDictionary(k => k.Item1.Trim(), v => v.Item2);
            return new TranslationsWithHeaderMap()
            {
                Worksheet = worksheet,
                EntityIdIndex = headers.GetOrNull(TranslationExcelOptions.EntityIdColumnName),
                TypeIndex = headers.GetOrNull(TranslationExcelOptions.TranslationTypeColumnName),
                OptionValueOrValidationIndexOrFixedRosterIdIndex = headers.GetOrNull(TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName),
                TranslationIndex = headers.GetOrNull(TranslationExcelOptions.TranslationTextColumnName),
            };
        }

        private string GetCleanedValue(TranslationType translationType, bool isGroup, string value)
        {
            switch (translationType)
            {
                case TranslationType.Title:
                case TranslationType.Instruction:
                    return isGroup ? 
                        System.Web.HttpUtility.HtmlDecode(CommandUtils.SanitizeHtml(value, true)):
                        System.Web.HttpUtility.HtmlDecode(CommandUtils.SanitizeHtml(value));                
                default:
                    return System.Web.HttpUtility.HtmlDecode(CommandUtils.SanitizeHtml(value, true));
            }
        }

        private IEnumerable<TranslationValidationError> Verify(IEnumerable<TranslationsWithHeaderMap> sheetsWithTranslation)
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

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList());
            this.translations.Remove(storedTranslations);
        }

        public int Count(Guid questionnaireId, Guid translationId)
            => this.translations.Query(_ => _.Count(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId));

        private TranslationRow GetExcelTranslation(TranslationsWithHeaderMap worksheetWithHeadersMap, int rowNumber) => new TranslationRow
        {
            EntityId = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.EntityIdIndex}{rowNumber}"].GetValue<string>(),
            Type = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.TypeIndex}{rowNumber}"].GetValue<string>(),
            OptionValueOrValidationIndexOrFixedRosterId = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex}{rowNumber}"].GetValue<string>(),
            Translation = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.TranslationIndex}{rowNumber}"].GetValue<string>()
        };

        private IEnumerable<TranslationValidationError> Verify(TranslationsWithHeaderMap worksheetWithHeadersMap)
        {
            var worksheet = worksheetWithHeadersMap.Worksheet;
            var end = worksheet.Dimension.End.Row;

            if (worksheetWithHeadersMap.EntityIdIndex == null)
                yield return new TranslationValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.EntityIdColumnName),
                };
            if (worksheetWithHeadersMap.TypeIndex == null)
                yield return new TranslationValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.TranslationTypeColumnName),
                };
            if (worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex == null)
                yield return new TranslationValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName),
                };
            if (worksheetWithHeadersMap.TranslationIndex == null)
                yield return new TranslationValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.TranslationTextColumnName),
                };

            if (worksheetWithHeadersMap.TypeIndex == null
                || worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex == null
                || worksheetWithHeadersMap.TranslationIndex == null
                || worksheetWithHeadersMap.EntityIdIndex == null)
            {
                yield break;
            }


            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
            {
                var importedTranslation = GetExcelTranslation(worksheetWithHeadersMap, rowNumber);

                if (!Guid.TryParse(importedTranslation.EntityId, out _))
                {
                    var cellAddress = $"{worksheetWithHeadersMap.EntityIdIndex}{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCel_A_lIsInvalid, TranslationExcelOptions.EntityIdColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }

                if (!Enum.TryParse(importedTranslation.Type, out TranslationType importedType) || importedType == TranslationType.Unknown)
                {
                    var cellAddress = $"{worksheetWithHeadersMap.TypeIndex}{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellTypeIsInvalid, TranslationExcelOptions.TranslationTypeColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }

                if (translationTypesWithIndexes.Contains(importedType) && string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = $"{worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex}{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellIndexIsInvalid, TranslationExcelOptions.TranslationTypeColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }
            }
        }
    }
}

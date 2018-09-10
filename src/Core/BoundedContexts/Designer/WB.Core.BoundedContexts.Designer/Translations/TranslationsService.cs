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

                        var translationErrors = this.Verify(sheetsWithTranslation).Take(10).ToList();
                        if (translationErrors.Any())
                            throw new InvalidExcelFileException(ExceptionMessages.TranlationExcelFileHasErrors) { FoundErrors = translationErrors };

                        var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
                        Dictionary<Guid, bool> idsOfAllQuestionnaireEntities =
                            questionnaire.Children.TreeToEnumerable(x => x.Children).ToDictionary(composite => composite.PublicKey, x=>x is Group);

                        var translationInstances = new List<TranslationInstance>();
                        foreach (var worksheet in sheetsWithTranslation)
                        {
                            var end = worksheet.Dimension.End.Row;

                            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
                            {
                                TranslationRow importedTranslation = GetExcelTranslation(worksheet.Cells, rowNumber);

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

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList());
            this.translations.Remove(storedTranslations);
        }

        public int Count(Guid questionnaireId, Guid translationId)
            => this.translations.Query(_ => _.Count(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId));

        private TranslationRow GetExcelTranslation(ExcelRange cells, int rowNumber) => new TranslationRow
        {
            EntityId = cells[$"A{rowNumber}"].GetValue<string>(),
            Type = cells[$"C{rowNumber}"].GetValue<string>(),
            OptionValueOrValidationIndexOrFixedRosterId = cells[$"D{rowNumber}"].GetValue<string>(),
            Translation = cells[$"F{rowNumber}"].GetValue<string>()
        };

        private IEnumerable<TranslationValidationError> Verify(ExcelWorksheet worksheet)
        {
            var end = worksheet.Dimension.End.Row;

            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
            {
                var importedTranslation = GetExcelTranslation(worksheet.Cells, rowNumber);

                if (!Guid.TryParse(importedTranslation.EntityId, out _))
                {
                    var cellAddress = $"A{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCel_A_lIsInvalid, TranslationExcelOptions.EntityIdColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }

                if (!Enum.TryParse(importedTranslation.Type, out TranslationType importedType) || importedType == TranslationType.Unknown)
                {
                    var cellAddress = $"C{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellTypeIsInvalid, TranslationExcelOptions.TranslationTypeColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }

                if (translationTypesWithIndexes.Contains(importedType) && string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = $"D{rowNumber}";

                    yield return new TranslationValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellIndexIsInvalid, TranslationExcelOptions.TranslationTypeColumnName,
                            cellAddress),
                        ErrorAddress = cellAddress
                    };
                }
            }
        }
    }
}

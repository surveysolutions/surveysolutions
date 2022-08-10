using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    internal class TranslationsService : IDesignerTranslationService
    {
        private class TranslationRow
        {
            public string? EntityId { get; set; }
            public string? Type { get; set; }
            public string? OptionValueOrValidationIndexOrFixedRosterId { get; set; }
            public string? Translation { get; set; }
        }

        private class TranslationsWithHeaderMap
        {
            public TranslationsWithHeaderMap(IXLWorksheet worksheet)
            {
                Worksheet = worksheet;
            }
            public IXLWorksheet Worksheet { get; set; }
            public string? EntityIdIndex { get; set; }
            public string? TypeIndex { get; set; }
            public string? OptionValueOrValidationIndexOrFixedRosterIdIndex { get; set; }
            public string? TranslationIndex { get; set; }
        }

        private readonly TranslationType[] translationTypesWithIndexes =
        {
            TranslationType.FixedRosterTitle,
            TranslationType.OptionTitle,
            TranslationType.ValidationMessage,
            TranslationType.SpecialValue
        };
        
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireViewFactory questionnaireStorage;
        private readonly ITranslationsExportService translationsExportService;
        private readonly ICategoriesService categoriesService;

        public TranslationsService(DesignerDbContext dbContext,
            IQuestionnaireViewFactory questionnaireStorage,
            ITranslationsExportService translationsExportService,
            ICategoriesService categoriesService)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
            this.translationsExportService = translationsExportService;
            this.categoriesService = categoriesService;
        }

        public ITranslation Get(Guid questionnaireId, Guid translationId)
        {
            var storedTranslations = this.dbContext.TranslationInstances
                    .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                    .ToList()
                .Cast<TranslationDto>()
                .ToList();

            return new QuestionnaireTranslation(storedTranslations);
        }

        public TranslationFile GetAsExcelFile(QuestionnaireRevision questionnaireId, Guid translationId) =>
            this.GetTranslationFile(questionnaireId, translationId);

        public TranslationFile GetTemplateAsExcelFile(QuestionnaireRevision questionnaireId) =>
            this.GetTranslationFile(questionnaireId);

        public bool HasTranslatedTitle(QuestionnaireDocument questionnaire)
        {
            var allTranslationIds = questionnaire.Translations.Select(x => x.Id).ToList();

            var hasTranslatedTitle = this.dbContext.TranslationInstances.Any(x => 
                allTranslationIds.Contains(x.TranslationId) 
                && x.QuestionnaireId == questionnaire.PublicKey
                && x.QuestionnaireEntityId == questionnaire.PublicKey
                );
            return hasTranslatedTitle;
        }

        private TranslationFile GetTranslationFile(QuestionnaireRevision questionnaireId, Guid? translationId = null)
        {
            var questionnaire = this.questionnaireStorage.Load(questionnaireId);
            if(questionnaire == null) throw new InvalidOperationException("Questionnaire was not found.");
            
            var translation = translationId.HasValue
                ? this.Get(questionnaire.PublicKey, translationId.Value)
                : new QuestionnaireTranslation(new List<TranslationDto>());

            var categoriesService = new CategoriesService(questionnaire.PublicKey, this.categoriesService);

            return translationsExportService.GenerateTranslationFile(questionnaire.Source, translationId ?? Guid.Empty, translation, categoriesService);
        }

        public void Store(Guid questionnaireId, Guid translationId, byte[]? excelRepresentation)
        {
            //if (translationId == null) throw new ArgumentNullException(nameof(translationId));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            using MemoryStream stream = new MemoryStream(excelRepresentation);

            try
            {
                using var package = new XLWorkbook(stream);

                if (package.Worksheets.Count == 0)
                    throw new InvalidFileException(ExceptionMessages.TranslationFileIsEmpty);

                var questionnaire = this.questionnaireStorage.Load(new QuestionnaireRevision(questionnaireId));
                if (questionnaire == null)
                    throw new InvalidFileException(ExceptionMessages.QuestionnaireCantBeFound);

                var sheetsWithTranslation = package.Worksheets
                    .Where(x => x.Name == TranslationExcelOptions.WorksheetName ||
                                x.Name.StartsWith(TranslationExcelOptions.OptionsWorksheetPreffix) ||
                                (x.Name.StartsWith(TranslationExcelOptions.CategoriesWorksheetPreffix) &&
                                 questionnaire.Source.Categories.Any(y =>
                                     y.Name.ToLower() == x.Name.ToLower().TrimStart(TranslationExcelOptions.CategoriesWorksheetPreffix))))
                    .ToList();

                if (!sheetsWithTranslation.Any())
                    throw new InvalidFileException(ExceptionMessages.TranslationWorksheetIsMissing);

                var translationsWithHeaderMap = sheetsWithTranslation.Select(CreateHeaderMap).ToList();
                Dictionary<Guid, bool> idsOfAllQuestionnaireEntities = questionnaire.Source.Children.TreeToEnumerable(x => x.Children)
                    .ToDictionary(composite => composite.PublicKey, x => x is Group);
                idsOfAllQuestionnaireEntities[questionnaireId] = true;
                
                var translationInstances = new List<TranslationInstance>();
                foreach (var translationWithHeaderMap in translationsWithHeaderMap)
                {
                    var worksheetTranslations = GetWorksheetTranslations(translationWithHeaderMap,
                        questionnaire.Source, idsOfAllQuestionnaireEntities, questionnaireId, translationId);
                    translationInstances.AddRange(worksheetTranslations);
                }

                var uniqueTranslationInstances = translationInstances
                    .Distinct(new TranslationInstance.IdentityComparer())
                    .ToList();

                foreach (var translationInstance in uniqueTranslationInstances)
                {
                    this.dbContext.TranslationInstances.Add(translationInstance);
                }

                this.dbContext.SaveChanges();
            }
            catch (NullReferenceException e)
            {
                throw new InvalidFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
            }
            catch (InvalidDataException e)
            {
                throw new InvalidFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
            }
            catch (COMException e)
            {
                throw new InvalidFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
            }
        }

        private IEnumerable<TranslationInstance> GetWorksheetTranslations(
            TranslationsWithHeaderMap translationWithHeaderMap, QuestionnaireDocument questionnaire,
            Dictionary<Guid, bool> idsOfAllQuestionnaireEntities, Guid questionnaireId, Guid translationId)
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
                : (Guid?) null;

            var translationErrors = (isCategoriesWorksheet
                ? this.VerifyCategories(translationWithHeaderMap)
                : this.Verify(translationWithHeaderMap)).Take(10).ToList();

            if (translationErrors.Any())
                throw new InvalidFileException(ExceptionMessages.TranlationExcelFileHasErrors)
                    {FoundErrors = translationErrors};

            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
            {
                TranslationRow importedTranslation = GetExcelTranslation(translationWithHeaderMap, rowNumber);

                if (string.IsNullOrWhiteSpace(importedTranslation.Translation)) continue;

                var translationInstance = categoriesId.HasValue
                    ? GetCategoriesTranslation(questionnaireId, translationId, categoriesId.Value, importedTranslation)
                    : GetQuestionnaireTranslation(questionnaireId, translationId,
                        importedTranslation, idsOfAllQuestionnaireEntities);

                if (translationInstance != null)
                    yield return translationInstance;
            }
        }

        private TranslationInstance GetCategoriesTranslation(Guid questionnaireId, Guid translationId,
            Guid categoriesId, TranslationRow importedTranslation) =>
            new TranslationInstance
            {
                Id = Guid.NewGuid(),
                QuestionnaireId = questionnaireId,
                TranslationId = translationId,
                QuestionnaireEntityId = categoriesId,
                Value = this.GetCleanedValue(TranslationType.Categories, false, importedTranslation.Translation),
                TranslationIndex = importedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                Type = TranslationType.Categories
            };

        private TranslationInstance? GetQuestionnaireTranslation(Guid questionnaireId, Guid translationId, TranslationRow importedTranslation,
            Dictionary<Guid, bool> idsOfAllQuestionnaireEntities)
        {
            if (importedTranslation.EntityId == null) throw new InvalidOperationException("Invalid EntityId.");
            if (importedTranslation.Type == null) throw new InvalidOperationException("Invalid Entity type.");

            var questionnaireEntityId = Guid.Parse(importedTranslation.EntityId);
            if (!idsOfAllQuestionnaireEntities.Keys.Contains(questionnaireEntityId)) return null;

            var translationType = (TranslationType) Enum.Parse(typeof(TranslationType), importedTranslation.Type);

            var cleanedValue = this.GetCleanedValue(translationType, idsOfAllQuestionnaireEntities[questionnaireEntityId],
                importedTranslation.Translation);

            return new TranslationInstance
            {
                Id = Guid.NewGuid(),
                QuestionnaireId = questionnaireId,
                TranslationId = translationId,
                QuestionnaireEntityId = questionnaireEntityId,
                Value = cleanedValue,
                TranslationIndex = importedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                Type = translationType
            };
        }

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

        private string GetCleanedValue(TranslationType translationType, bool isGroup, string? value)
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

        public void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId)
        {
            var storedTranslations = this.dbContext.TranslationInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                .ToList();

            foreach (var storedTranslation in storedTranslations)
            {
                var translationCopy = storedTranslation.Clone();
                translationCopy.Id = Guid.NewGuid();
                translationCopy.TranslationId = newTranslationId;
                translationCopy.QuestionnaireId = newQuestionnaireId;
                this.dbContext.TranslationInstances.Add(translationCopy);
            }
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var storedTranslations = this.dbContext.TranslationInstances
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList();
            this.dbContext.TranslationInstances.RemoveRange(storedTranslations);
        }

        public int Count(Guid questionnaireId, Guid translationId)
            => this.dbContext.TranslationInstances.Count(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId);

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

        private IEnumerable<ImportValidationError> Verify(TranslationsWithHeaderMap worksheetWithHeadersMap)
        {
            var worksheet = worksheetWithHeadersMap.Worksheet;
            var end = worksheet.LastRowUsed().RowNumber();

            if (worksheetWithHeadersMap.EntityIdIndex == null)
                yield return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.EntityIdColumnName),
                };
            if (worksheetWithHeadersMap.TypeIndex == null)
                yield return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.TranslationTypeColumnName),
                };
            if (worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex == null)
                yield return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName),
                };
            if (worksheetWithHeadersMap.TranslationIndex == null)
                yield return new ImportValidationError
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

                    yield return new ImportValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCel_A_lIsInvalid, TranslationExcelOptions.EntityIdColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }

                if (!Enum.TryParse(importedTranslation.Type, out TranslationType importedType) || importedType == TranslationType.Unknown)
                {
                    var cellAddress = $"{worksheetWithHeadersMap.TypeIndex}{rowNumber}";

                    yield return new ImportValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellTypeIsInvalid, TranslationExcelOptions.TranslationTypeColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }

                if (translationTypesWithIndexes.Contains(importedType) && string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = $"{worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex}{rowNumber}";

                    yield return new ImportValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellIndexIsInvalid, TranslationExcelOptions.TranslationTypeColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }
            }
        }

        private IEnumerable<ImportValidationError> VerifyCategories(TranslationsWithHeaderMap worksheetWithHeadersMap)
        {
            var worksheet = worksheetWithHeadersMap.Worksheet;
            var end = worksheet.LastRowUsed().RowNumber();
            
            if (worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex == null)
                yield return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName),
                };
            if (worksheetWithHeadersMap.TranslationIndex == null)
                yield return new ImportValidationError
                {
                    Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, TranslationExcelOptions.TranslationTextColumnName),
                };

            if (worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex == null
                || worksheetWithHeadersMap.TranslationIndex == null)
            {
                yield break;
            }


            for (int rowNumber = 2; rowNumber <= end; rowNumber++)
            {
                var importedTranslation = GetExcelTranslation(worksheetWithHeadersMap, rowNumber);

                if (string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = $"{worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex}{rowNumber}";

                    yield return new ImportValidationError
                    {
                        Message = string.Format(ExceptionMessages.TranslationCellIndexIsInvalid, TranslationExcelOptions.TranslationTypeColumnName, cellAddress),
                        ErrorAddress = cellAddress
                    };
                }
            }
        }

        private class CategoriesService : ICategories
        {
            private readonly Guid questionnaireId;
            private readonly ICategoriesService categoriesService;

            public CategoriesService(Guid questionnaireId, ICategoriesService categoriesService)
            {
                this.questionnaireId = questionnaireId;
                this.categoriesService = categoriesService;
            }

            public List<CategoriesItem> GetCategories(Guid categoriesId) =>
                this.categoriesService.GetCategoriesById(questionnaireId, categoriesId).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;

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
        
        private readonly DesignerDbContext dbContext;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly ITranslationsExportService translationsExportService;
        private readonly ICategoriesService categoriesService;

        public TranslationsService(DesignerDbContext dbContext,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
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

        public TranslationFile GetAsExcelFile(Guid questionnaireId, Guid translationId) =>
            this.GetTranslationFile(questionnaireId, translationId);

        public TranslationFile GetTemplateAsExcelFile(Guid questionnaireId) =>
            this.GetTranslationFile(questionnaireId);

        private TranslationFile GetTranslationFile(Guid questionnaireId, Guid? translationId = null)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            var translation = translationId.HasValue
                ? this.Get(questionnaireId, translationId.Value)
                : new QuestionnaireTranslation(new List<TranslationDto>());

            var categoriesService = new CategoriesService(questionnaireId, this.categoriesService);

            return translationsExportService.GenerateTranslationFile(questionnaire, translationId ?? Guid.Empty, translation, categoriesService);
        }

        public void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation)
        {
            if (translationId == null) throw new ArgumentNullException(nameof(translationId));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            using MemoryStream stream = new MemoryStream(excelRepresentation);

            try
            {
                using ExcelPackage package = new ExcelPackage(stream);

                if (package.Workbook.Worksheets.Count == 0)
                    throw new InvalidFileException(ExceptionMessages.TranslationFileIsEmpty);

                var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());

                var sheetsWithTranslation = package.Workbook.Worksheets
                    .Where(x => x.Name == TranslationExcelOptions.WorksheetName ||
                                x.Name.StartsWith(TranslationExcelOptions.OptionsWorksheetPreffix) ||
                                (x.Name.StartsWith(TranslationExcelOptions.CategoriesWorksheetPreffix) &&
                                 questionnaire.Categories.Any(y =>
                                     y.Name.ToLower() == x.Name.ToLower().TrimStart(TranslationExcelOptions.CategoriesWorksheetPreffix))))
                    .ToList();

                if (!sheetsWithTranslation.Any())
                    throw new InvalidFileException(ExceptionMessages.TranslationWorksheetIsMissing);

                var translationsWithHeaderMap = sheetsWithTranslation.Select(CreateHeaderMap).ToList();
                var idsOfAllQuestionnaireEntities = questionnaire.Children.TreeToEnumerable(x => x.Children)
                    .ToDictionary(composite => composite.PublicKey, x => x is Group);

                var translationInstances = new List<TranslationInstance>();
                foreach (var translationWithHeaderMap in translationsWithHeaderMap)
                {
                    translationInstances.AddRange(GetWorksheetTranslations(translationWithHeaderMap,
                        questionnaire, idsOfAllQuestionnaireEntities, questionnaireId, translationId));
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
            var end = worksheet.Dimension.End.Row;

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

        private TranslationInstance GetQuestionnaireTranslation(Guid questionnaireId, Guid translationId, TranslationRow importedTranslation,
            Dictionary<Guid, bool> idsOfAllQuestionnaireEntities)
        {
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

        private TranslationRow GetExcelTranslation(TranslationsWithHeaderMap worksheetWithHeadersMap, int rowNumber) => this.AdjustIndexValue(new TranslationRow
        {
            EntityId = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.EntityIdIndex}{rowNumber}"].GetValue<string>(),
            Type = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.TypeIndex}{rowNumber}"].GetValue<string>(),
            OptionValueOrValidationIndexOrFixedRosterId = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.OptionValueOrValidationIndexOrFixedRosterIdIndex}{rowNumber}"].GetValue<string>(),
            Translation = worksheetWithHeadersMap.Worksheet.Cells[$"{worksheetWithHeadersMap.TranslationIndex}{rowNumber}"].GetValue<string>()
        });

        private TranslationRow AdjustIndexValue(TranslationRow row)
        {
            row.OptionValueOrValidationIndexOrFixedRosterId =
                row.OptionValueOrValidationIndexOrFixedRosterId?.TrimEnd('$');
            return row;
        }

        private IEnumerable<ImportValidationError> Verify(TranslationsWithHeaderMap worksheetWithHeadersMap)
        {
            var worksheet = worksheetWithHeadersMap.Worksheet;
            var end = worksheet.Dimension.End.Row;

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
            var end = worksheet.Dimension.End.Row;
            
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

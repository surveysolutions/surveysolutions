using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using SpreadsheetGear;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        }

        private readonly TranslationType[] translationTypesWithIndexes =
        {
            TranslationType.FixedRosterTitle, TranslationType.OptionTitle, TranslationType.ValidationMessage
        };

        private const string EntityIdColumnName = "Entity Id";
        private const string TranslationTypeColumnName = "Type";
        private const string WorksheetName = "Translations";
        private const string workSheetPassword = "Qwerty1234";

        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage;


        public TranslationsService(IPlainStorageAccessor<TranslationInstance> translations,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
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
        
        public TranslationFile GetAsExcelFile(Guid questionnaireId, Guid translationId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            var translation = this.Get(questionnaireId, translationId);
            var translationFile = new TranslationFile
            {
                QuestionnaireTitle = questionnaire.Title,
                TranslationName = questionnaire.Translations.FirstOrDefault(x => x.TranslationId == translationId)?.Name,
                ContentAsExcelFile = this.GetExcelFileContent(questionnaire, translation)
            };

            return translationFile;
        }

        public TranslationFile GetTemplateAsExcelFile(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            var translation = new QuestionnaireTranslation(new List<TranslationDto>());
            var translationFile = new TranslationFile
            {
                QuestionnaireTitle = questionnaire.Title,
                TranslationName = string.Empty,
                ContentAsExcelFile = this.GetExcelFileContent(questionnaire, translation)
            };

            return translationFile;
        }

        
        public void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation)
        {
            if (translationId == null) throw new ArgumentNullException(nameof(translationId));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            IWorkbookSet workbookSet = Factory.GetWorkbookSet();
            IWorkbook workbook;

            try
            {
                workbook = workbookSet.Workbooks.OpenFromMemory(excelRepresentation);
            }
            catch (Exception ex)
            {
                throw new InvalidExcelFileException(ex.Message);
            }
            
            var worksheet = workbook.Worksheets[0];
            worksheet.Protect(workSheetPassword);

            if (worksheet.Name != WorksheetName)
                throw new InvalidExcelFileException("Worksheet with translations not found");

            var translationErrors = this.Verify(worksheet).Take(10).ToList();
            if (translationErrors.Any())
                throw new InvalidExcelFileException("Found errors in excel file") { FoundErrors = translationErrors };


            this.Delete(questionnaireId, translationId);
            
            for (int rowNumber = 2; rowNumber <= worksheet.UsedRange.RowCount; rowNumber++)
            {
                TranslationRow importedTranslation = GetExcelTranslation(worksheet.UsedRange, rowNumber);

                if(string.IsNullOrWhiteSpace(importedTranslation.Translation)) continue;

                this.translations.Store(new TranslationInstance
                {
                    QuestionnaireId = questionnaireId,
                    TranslationId = translationId,
                    QuestionnaireEntityId = Guid.Parse(importedTranslation.EntityId),
                    Value = importedTranslation.Translation,
                    TranslationIndex = importedTranslation.OptionValueOrValidationIndexOrFixedRosterId,
                    Type = (TranslationType)Enum.Parse(typeof(TranslationType), importedTranslation.Type)
                }, null);
            }
        }

        public void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId)
        {
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                .ToList());

            foreach (var storedTranslation in storedTranslations)
            {
                storedTranslation.TranslationId = newTranslationId;
                storedTranslation.QuestionnaireId = newQuestionnaireId;
                this.translations.Store(storedTranslation, storedTranslation);
            }
        }

        public void Delete(Guid questionnaireId, Guid translationId)
        {
            var storedTranslations = this.translations.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                .ToList());
            this.translations.Remove(storedTranslations);
        }

        private TranslationRow GetExcelTranslation(IRange cells, int rowNumber) => new TranslationRow
        {
            EntityId = cells[$"A{rowNumber}"].Text,
            Type = cells[$"B{rowNumber}"].Text,
            OptionValueOrValidationIndexOrFixedRosterId = cells[$"C{rowNumber}"].Text,
            Translation = cells[$"E{rowNumber}"].Text
        };

        private IEnumerable<TranslationValidationError> Verify(IWorksheet worksheet)
        {
            for (int rowNumber = 2; rowNumber < worksheet.UsedRange.RowCount; rowNumber++)
            {
                var importedTranslation = GetExcelTranslation(worksheet.UsedRange.Cells, rowNumber);

                Guid entityId;
                if (!Guid.TryParse(importedTranslation.EntityId, out entityId))
                {
                    var cellAddress = worksheet.Cells[$"A{rowNumber}"].Address;

                    yield return new TranslationValidationError
                    {
                        Message = $"{EntityIdColumnName} has invalid id at [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }

                TranslationType importedType;
                if (!Enum.TryParse(importedTranslation.Type, out importedType) || importedType == TranslationType.Unknown)
                {
                    var cellAddress = worksheet.Cells[$"B{rowNumber}"].Address;

                    yield return new TranslationValidationError
                    {
                        Message = $"{TranslationTypeColumnName} has invalid type [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }

                if (translationTypesWithIndexes.Contains(importedType) && string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = worksheet.Cells[$"C{rowNumber}"].Address;

                    yield return new TranslationValidationError
                    {
                        Message = $"{TranslationTypeColumnName} has invalid index at [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }
            }
        }

        private byte[] GetExcelFileContent(QuestionnaireDocument questionnaire, ITranslation translation)
        {
            IWorkbook workbook = Factory.GetWorkbook();
            IWorksheet worksheet = workbook.Worksheets[0];

            IRange cells = worksheet.Cells;

            worksheet.Name = WorksheetName;

            cells["A1"].Value = EntityIdColumnName;
            cells["B1"].Value = TranslationTypeColumnName;
            cells["C1"].Value = "Index";
            cells["D1"].Value = "Original text";
            cells["E1"].Value = "Translation";
            cells["A1:E1"].Font.Bold = true;
            cells["A1:D1"].Interior.Color = Color.FromArgb(240, 240, 240);
            cells["A1:D1"].Borders.Color = Color.FromArgb(225, 225, 225);
            cells["A1"].AddComment("Read only field");
            cells["B1"].AddComment("Read only field");
            cells["C1"].AddComment("Read only field");
            cells["D1"].AddComment("Read only field");

            int currentRowNumber = 1;

            var textsToTranslate = this.GetTranlsatedTexts(questionnaire, translation);
            foreach (var translationRow in textsToTranslate)
            {
                if (string.IsNullOrWhiteSpace(translationRow.OriginalText)) continue;

                currentRowNumber++;

                cells[$"A{currentRowNumber}"].Value = translationRow.EntityId;
                cells[$"B{currentRowNumber}"].Value = translationRow.Type;
                cells[$"C{currentRowNumber}"].Value = translationRow.OptionValueOrValidationIndexOrFixedRosterId;
                cells[$"D{currentRowNumber}"].Value = translationRow.OriginalText;
                cells[$"E{currentRowNumber}"].Value = translationRow.Translation;
                cells[$"E{currentRowNumber}"].Locked = false;
                cells[$"A{currentRowNumber}:D{currentRowNumber}"].Interior.Color = Color.FromArgb(240, 240, 240);
                cells[$"A{currentRowNumber}:D{currentRowNumber}"].Borders.Color = Color.FromArgb(225, 225, 225);
            }
            
            cells["A:E"].Columns.AutoFit();
            cells["D:E"].ColumnWidth = 100;
            cells["D:E"].WrapText = true;

            worksheet.Protect(workSheetPassword);

            return workbook.SaveToMemory(FileFormat.Excel8);
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
            var singleQuestion = question as SingleQuestion;
            var isNeedIgnoreOptions = (singleQuestion?.CascadeFromQuestionId.HasValue ?? false) 
                                               || (singleQuestion?.IsFilteredCombobox ?? false);

            if (isNeedIgnoreOptions)
                return Enumerable.Empty<TranslationRow>();

            return from option in question.Answers
                select new TranslationRow
                {
                    EntityId = question.PublicKey.FormatGuid(),
                    Type = TranslationType.OptionTitle.ToString("G"),
                    OriginalText = option.AnswerText,
                    Translation = translation.GetAnswerOption(question.PublicKey, option.AnswerValue),
                    OptionValueOrValidationIndexOrFixedRosterId = option.AnswerValue
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
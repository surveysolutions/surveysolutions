using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Aspose.Cells;
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
        private const string workSheetPassword = "P@$$w0rd";

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
                TranslationName = questionnaire.Translations.FirstOrDefault(x => x.Id == translationId)?.Name,
                ContentAsExcelFile = this.GetExcelFileContent(questionnaire, translation, FileFormatType.Xlsx, SaveFormat.Xlsx)
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
                ContentAsExcelFile = this.GetExcelFileContent(questionnaire, translation, FileFormatType.Xlsx, SaveFormat.Xlsx)
            };

            return translationFile;
        }

        public TranslationFile GetTemplateAsOpenOfficeFile(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            var translation = new QuestionnaireTranslation(new List<TranslationDto>());
            var translationFile = new TranslationFile
            {
                QuestionnaireTitle = questionnaire.Title,
                TranslationName = string.Empty,
                ContentAsExcelFile = this.GetExcelFileContent(questionnaire, translation, FileFormatType.ODS, SaveFormat.ODS)
            };

            return translationFile;
        }


        public void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation)
        {
            if (translationId == null) throw new ArgumentNullException(nameof(translationId));
            if (excelRepresentation == null) throw new ArgumentNullException(nameof(excelRepresentation));

            Workbook workbook;
            try
            {
                workbook = new Workbook(new MemoryStream(excelRepresentation));
            }
            catch (Exception ex)
            {
                throw new InvalidExcelFileException(ex.Message);
            }

            Worksheet worksheet = workbook.Worksheets[0];

            if (worksheet.Name != WorksheetName)
                throw new InvalidExcelFileException("Worksheet with translations not found");

            var translationErrors = this.Verify(worksheet).Take(10).ToList();
            if (translationErrors.Any())
                throw new InvalidExcelFileException("Found errors in excel file") { FoundErrors = translationErrors };


            this.Delete(questionnaireId, translationId);
            
            for (int rowNumber = 2; rowNumber <= worksheet.Cells.Rows.Count; rowNumber++)
            {
                TranslationRow importedTranslation = GetExcelTranslation(worksheet.Cells, rowNumber);

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

        private TranslationRow GetExcelTranslation(Cells cells, int rowNumber) => new TranslationRow
        {
            EntityId = cells[$"A{rowNumber}"].StringValue,
            Type = cells[$"B{rowNumber}"].StringValue,
            OptionValueOrValidationIndexOrFixedRosterId = cells[$"C{rowNumber}"].StringValue,
            Translation = cells[$"E{rowNumber}"].StringValue
        };

        private IEnumerable<TranslationValidationError> Verify(Worksheet worksheet)
        {
            for (int rowNumber = 2; rowNumber < worksheet.Cells.Rows.Count; rowNumber++)
            {
                var importedTranslation = GetExcelTranslation(worksheet.Cells, rowNumber);

                Guid entityId;
                if (!Guid.TryParse(importedTranslation.EntityId, out entityId))
                {
                    var cellAddress = $"A{rowNumber}"; //worksheet.Cells[$"A{rowNumber}"].;

                    yield return new TranslationValidationError
                    {
                        Message = $"{EntityIdColumnName} has invalid id at [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }

                TranslationType importedType;
                if (!Enum.TryParse(importedTranslation.Type, out importedType) || importedType == TranslationType.Unknown)
                {
                    var cellAddress = $"B{rowNumber}"; //worksheet.Cells[$"B{rowNumber}"].Address;

                    yield return new TranslationValidationError
                    {
                        Message = $"{TranslationTypeColumnName} has invalid type [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }

                if (translationTypesWithIndexes.Contains(importedType) && string.IsNullOrWhiteSpace(importedTranslation.OptionValueOrValidationIndexOrFixedRosterId))
                {
                    var cellAddress = $"C{rowNumber}"; //worksheet.Cells[$"C{rowNumber}"].Address;

                    yield return new TranslationValidationError
                    {
                        Message = $"{TranslationTypeColumnName} has invalid index at [{cellAddress}]",
                        ErrorAddress = cellAddress
                    };
                }
            }
        }

        private byte[] GetExcelFileContent(QuestionnaireDocument questionnaire, ITranslation translation, FileFormatType fileFormatType, SaveFormat saveFormat)
        {
            Workbook workbook = new Workbook(fileFormatType);

            int i = workbook.Worksheets.Count > 0 ? 0 : workbook.Worksheets.Add();

            Worksheet worksheet = workbook.Worksheets[i];

            worksheet.Name = WorksheetName;

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

            AddComment(workbook, "A1", "Read only field");
            AddComment(workbook, "B1", "Read only field");
            AddComment(workbook, "C1", "Read only field");
            AddComment(workbook, "D1", "Read only field");

            int currentRowNumber = 1;

            var textsToTranslate = this.GetTranlsatedTexts(questionnaire, translation);
            foreach (var translationRow in textsToTranslate)
            {
                if (string.IsNullOrWhiteSpace(translationRow.OriginalText)) continue;

                currentRowNumber++;

                worksheet.Cells[$"A{currentRowNumber}"].Value = translationRow.EntityId;
                worksheet.Cells[$"B{currentRowNumber}"].Value = translationRow.Type;
                worksheet.Cells[$"C{currentRowNumber}"].Value = translationRow.OptionValueOrValidationIndexOrFixedRosterId;
                worksheet.Cells[$"D{currentRowNumber}"].Value = translationRow.OriginalText;
                worksheet.Cells[$"E{currentRowNumber}"].Value = translationRow.Translation;
            }

            //worksheet.Protect(ProtectionType.All, workSheetPassword, null);

            worksheet.AutoFitColumns();
            worksheet.Cells.SetColumnWidth(3, 100);
            worksheet.Cells.SetColumnWidth(4, 100);

            LockColumn(worksheet, 0);
            LockColumn(worksheet, 1);
            LockColumn(worksheet, 2);
            LockColumn(worksheet, 3);

            for (int j = 2; j < worksheet.Cells.Rows.Count; j++)
            {
                SetWordWrap(worksheet, $"D{j}");
            }
            MemoryStream stream = new MemoryStream();
            workbook.Save(stream, saveFormat);

            // Rewind the stream position back to zero so it is ready for the next reader.
            stream.Position = 0;
            return stream.ToArray();
        }

        private static void LockColumn(Worksheet worksheet, int i)
        {
            var style = worksheet.Cells.Columns[(byte) i].Style;
            style.IsLocked = true;
            worksheet.Cells.Columns[(byte) i].ApplyStyle(style, new StyleFlag {Locked = true});
        }

        private void SetWordWrap(Worksheet worksheet, string address)
        {
            var cell = worksheet.Cells[address];
            var cellStyle = cell.GetStyle();
            cellStyle.IsTextWrapped = true;
            cell.SetStyle(cellStyle);
        }

        private void FormatCell(Worksheet worksheet, string address)
        {
            var cell = worksheet.Cells[address];
            var cellStyle = cell.GetStyle();
            cellStyle.Font.IsBold = true;
            cell.SetStyle(cellStyle);
        }

        private static void AddComment(Workbook workbook, string cellName, string comment)
        {
            int commentIndex = workbook.Worksheets[0].Comments.Add(cellName);
            Comment cellComment = workbook.Worksheets[0].Comments[commentIndex];
            cellComment.Note = comment;
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
            var shouldIgnoreOptions = (singleOptionQuestion?.CascadeFromQuestionId.HasValue ?? false) 
                                               || (singleOptionQuestion?.IsFilteredCombobox ?? false);

            if (shouldIgnoreOptions)
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
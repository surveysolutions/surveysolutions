using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.ImportExport.Models.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Designer.Code.ImportExport
{
    /// <summary>
    /// Imports a questionnaire supplied as a plain JSON document (without the companion files of a
    /// backup archive). Domain validation always runs before the questionnaire is created, so a
    /// rejected document never leaves a partially created questionnaire behind.
    /// </summary>
    public class QuestionnaireJsonImportService : IQuestionnaireJsonImportService
    {
        private const int MaxReportedErrors = 50;

        private readonly ILogger<QuestionnaireJsonImportService> logger;
        private readonly ICommandService commandService;
        private readonly IImportExportQuestionnaireMapper importExportQuestionnaireMapper;
        private readonly IQuestionnaireSerializer questionnaireSerializer;
        private readonly IQuestionnaireSchemaProvider schemaProvider;

        public QuestionnaireJsonImportService(
            ILogger<QuestionnaireJsonImportService> logger,
            ICommandService commandService,
            IImportExportQuestionnaireMapper importExportQuestionnaireMapper,
            IQuestionnaireSerializer questionnaireSerializer,
            IQuestionnaireSchemaProvider schemaProvider)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.importExportQuestionnaireMapper = importExportQuestionnaireMapper;
            this.questionnaireSerializer = questionnaireSerializer;
            this.schemaProvider = schemaProvider;
        }

        public QuestionnaireJsonImportResult ValidateAndImport(string questionnaireJson, Guid responsibleId)
        {
            var result = new QuestionnaireJsonImportResult
            {
                SchemaVersion = schemaProvider.GetSchemaVersion()
            };

            JToken parsedJson;
            try
            {
                parsedJson = JToken.Parse(questionnaireJson);
            }
            catch (JsonReaderException exception)
            {
                result.Errors.Add($"Questionnaire is not a valid JSON document: {exception.Message}");
                return result;
            }

            var schema = JSchema.Parse(schemaProvider.GetSchema());
            if (!parsedJson.IsValid(schema, out IList<ValidationError> schemaErrors))
            {
                result.Errors.AddRange(schemaErrors
                    .Take(MaxReportedErrors)
                    .Select(e => $"{e.Path} (line {e.LineNumber}): {e.Message}"));
                return result;
            }

            Questionnaire? questionnaire;
            try
            {
                questionnaire = questionnaireSerializer.Deserialize(questionnaireJson);
            }
            catch (JsonException exception)
            {
                result.Errors.Add($"Questionnaire cannot be processed: {exception.Message}");
                return result;
            }

            if (questionnaire == null)
            {
                result.Errors.Add("Questionnaire cannot be processed.");
                return result;
            }

            DropEntitiesRequiringCompanionFiles(questionnaire, result.Warnings);

            // Always create a new questionnaire owned by the caller; never overwrite an existing one.
            questionnaire.Id = Guid.NewGuid();

            QuestionnaireDocument questionnaireDocument;
            try
            {
                questionnaireDocument = importExportQuestionnaireMapper.Map(questionnaire);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to map imported questionnaire JSON to a questionnaire document.");
                result.Errors.Add($"Questionnaire structure cannot be processed: {exception.Message}");
                return result;
            }

            questionnaireDocument.PublicKey = questionnaire.Id;
            questionnaireDocument.CreationDate = DateTime.UtcNow;
            questionnaireDocument.CreatedBy = responsibleId;

            List<QuestionnaireVerificationMessage> verificationErrors;
            try
            {
                verificationErrors = new ImportQuestionnaireVerifier()
                    .Verify(questionnaireDocument.AsReadOnly())
                    .Where(m => m.MessageLevel >= VerificationMessageLevel.General)
                    .ToList();
            }
            catch (Exception exception)
            {
                // A generated questionnaire can be schema-valid yet malformed enough to break a verifier.
                logger.LogWarning(exception, "Verification of an imported questionnaire threw.");
                result.Errors.Add($"Questionnaire could not be verified: {exception.Message}");
                return result;
            }

            if (verificationErrors.Any())
            {
                result.Errors.AddRange(verificationErrors
                    .Take(MaxReportedErrors)
                    .Select(v => $"[{v.Code}] {v.Message}{FormatReference(v)}"));
                return result;
            }

            try
            {
                commandService.Execute(new ImportQuestionnaire(responsibleId, questionnaireDocument));
            }
            catch (QuestionnaireException exception)
            {
                logger.LogWarning(exception, "Imported questionnaire was rejected by a domain rule.");
                result.Errors.Add(exception.Message);
                return result;
            }

            result.Succeeded = true;
            result.QuestionnaireId = questionnaireDocument.PublicKey;
            result.QuestionnaireTitle = questionnaireDocument.Title;
            return result;
        }

        private static string FormatReference(QuestionnaireVerificationMessage message)
        {
            var reference = message.References.FirstOrDefault();
            return reference?.Id == null ? string.Empty : $" ({reference.Id})";
        }

        /// <summary>
        /// The JSON-only contract carries no attachment binaries, lookup table content, translation
        /// files or reusable category files, so such references cannot be restored. They are removed
        /// and reported as warnings instead of failing an otherwise valid import.
        /// </summary>
        private static void DropEntitiesRequiringCompanionFiles(
            Questionnaire questionnaire, List<QuestionnaireJsonImportWarning> warnings)
        {
            foreach (var attachment in questionnaire.Attachments)
                warnings.Add(new QuestionnaireJsonImportWarning
                {
                    Source = $"Attachment '{attachment.Name}'",
                    Description = "Attachment content cannot be transferred by the questionnaire JSON import and was removed.",
                    Action = "Skipped"
                });
            questionnaire.Attachments.Clear();

            foreach (var lookupTable in questionnaire.LookupTables)
                warnings.Add(new QuestionnaireJsonImportWarning
                {
                    Source = $"Lookup table '{lookupTable.TableName}'",
                    Description = "Lookup table content cannot be transferred by the questionnaire JSON import and was removed.",
                    Action = "Skipped"
                });
            questionnaire.LookupTables.Clear();

            foreach (var translation in questionnaire.Translations.Items)
                warnings.Add(new QuestionnaireJsonImportWarning
                {
                    Source = $"Translation '{translation.Name}'",
                    Description = "Translation content cannot be transferred by the questionnaire JSON import and was removed.",
                    Action = "Skipped"
                });
            questionnaire.Translations = new Translations();

            foreach (var categories in questionnaire.Categories)
                warnings.Add(new QuestionnaireJsonImportWarning
                {
                    Source = $"Reusable categories '{categories.Name}'",
                    Description = "Reusable categories content cannot be transferred by the questionnaire JSON import and was removed.",
                    Action = "Skipped"
                });
            questionnaire.Categories.Clear();

            var entities = EnumerateEntities(questionnaire).ToList();

            foreach (var staticText in entities.OfType<StaticText>())
                staticText.AttachmentName = null;

            foreach (var categoricalQuestion in entities.OfType<ICategoricalQuestion>())
            {
                if (categoricalQuestion.CategoriesId == null) continue;

                warnings.Add(new QuestionnaireJsonImportWarning
                {
                    Source = $"Question '{(categoricalQuestion as AbstractQuestion)?.VariableName}'",
                    Description = "Reference to reusable categories was removed because reusable categories are not part of the questionnaire JSON import.",
                    Action = "Modified"
                });
                categoricalQuestion.CategoriesId = null;
            }

            foreach (var answer in entities.OfType<SingleQuestion>().SelectMany(q => q.Answers ?? new List<Answer>()))
                answer.AttachmentName = null;

            foreach (var answer in entities.OfType<MultiOptionsQuestion>().SelectMany(q => q.Answers ?? new List<Answer>()))
                answer.AttachmentName = null;
        }

        private static IEnumerable<QuestionnaireEntity> EnumerateEntities(Questionnaire questionnaire)
        {
            var roots = new List<QuestionnaireEntity>();
            if (questionnaire.CoverPage != null) roots.Add(questionnaire.CoverPage);
            roots.AddRange(questionnaire.Children);

            var stack = new Stack<QuestionnaireEntity>(roots);
            while (stack.Count > 0)
            {
                var entity = stack.Pop();
                yield return entity;

                var children = entity switch
                {
                    CoverPage coverPage => coverPage.Children,
                    Group group => group.Children,
                    _ => null
                };

                if (children == null) continue;

                foreach (var child in children)
                    stack.Push(child);
            }
        }
    }
}

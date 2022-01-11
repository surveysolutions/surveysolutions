using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using Group = Main.Core.Entities.SubEntities.Group;
using TranslationType = WB.Core.BoundedContexts.Designer.ImportExport.Models.TranslationType;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class TranslationImportExportService : ITranslationImportExportService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IQuestionnaireSerializer questionnaireSerializer;

        public TranslationImportExportService(DesignerDbContext dbContext,
            IMapper mapper,
            IQuestionnaireSerializer questionnaireSerializer)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.questionnaireSerializer = questionnaireSerializer;
        }

        public string GetTranslationsJson(QuestionnaireDocument questionnaire, Guid translationId)
        {
            var questionnaireId = questionnaire.PublicKey;
            var storedTranslations = dbContext.TranslationInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId)
                .ToList()
                .Cast<TranslationDto>()
                .ToList();

            var items = storedTranslations.Select(t =>
                new TranslationItem()
                {
                    Type = (TranslationType)t.Type,
                    Value = t.Value,
                    EntityId = t.QuestionnaireEntityId,
                    OptionIndex = t.TranslationIndex,
                    EntityVariableName = questionnaire.Find<IComposite>(t.QuestionnaireEntityId)?.GetVariable()
                }).ToList();
            var json = questionnaireSerializer.Serialize(items);
            return json;
        }

        public void StoreTranslationsFromJson(QuestionnaireDocument questionnaire, Guid translationId, string json)
        {
            if (questionnaire == null) throw new ArgumentNullException(nameof(questionnaire));
            if (translationId == null) throw new ArgumentNullException(nameof(translationId));
            if (json == null) throw new ArgumentNullException(nameof(json));

            try
            {
                var items = questionnaireSerializer.Deserialize<TranslationItem>(json);
            
                Dictionary<Guid, bool> idsOfAllQuestionnaireEntities = questionnaire.Children.TreeToEnumerable(x => x.Children)
                    .ToDictionary(composite => composite.PublicKey, x => x is Group);
                idsOfAllQuestionnaireEntities[questionnaire.PublicKey] = true;

                List<TranslationInstance> translationInstances = new List<TranslationInstance>();
                foreach (var translation in items)
                {
                    var translationInstance = GetQuestionnaireTranslation(questionnaire, translationId, translation, idsOfAllQuestionnaireEntities);
                    if (translationInstance != null)
                        translationInstances.Add(translationInstance);
                }
            
                var uniqueTranslationInstances = translationInstances
                    .Distinct(new TranslationInstance.IdentityComparer())
                    .ToList();

                // this.dbContext.TranslationInstances.AddRange(translationInstances);
                foreach (var translationInstance in uniqueTranslationInstances)
                {
                    dbContext.TranslationInstances.Add(translationInstance);
                }

                dbContext.SaveChanges();
            }
            catch (COMException e)
            {
                throw new InvalidFileException(ExceptionMessages.TranslationsCantBeExtracted, e);
            }
        }

        private TranslationInstance? GetQuestionnaireTranslation(QuestionnaireDocument questionnaire, Guid translationId, TranslationItem translationItem,
            Dictionary<Guid, bool> idsOfAllQuestionnaireEntities)
        {
            Guid questionnaireEntityId = Guid.Empty;
            bool isGroup = false;

            if (!translationItem.EntityVariableName.IsNullOrEmpty())
            {
                var entity = questionnaire.FirstOrDefault<IComposite>(c => c.VariableName == translationItem.EntityVariableName);
                if (entity != null)
                {
                    questionnaireEntityId = entity.PublicKey;
                    isGroup = entity is Group;
                }
            }
            else if (translationItem.EntityId.HasValue && idsOfAllQuestionnaireEntities.ContainsKey(translationItem.EntityId.Value))
            {
                isGroup = idsOfAllQuestionnaireEntities[translationItem.EntityId.Value];
            }
            else
            {
                return null;
            }

            var translationType = (WB.Core.SharedKernels.Questionnaire.Translations.TranslationType)translationItem.Type;

            var cleanedValue = GetCleanedValue(translationItem.Type, isGroup, translationItem.Value);

            return new TranslationInstance
            {
                Id = Guid.NewGuid(),
                QuestionnaireId = questionnaire.PublicKey,
                TranslationId = translationId,
                QuestionnaireEntityId = questionnaireEntityId,
                Value = cleanedValue,
                TranslationIndex = translationItem.OptionIndex,
                Type = translationType
            };
        }

        private string GetCleanedValue(Models.TranslationType translationType, bool isGroup, string? value)
        {
            switch (translationType)
            {
                case Models.TranslationType.Title:
                case Models.TranslationType.Instruction:
                    return isGroup ? 
                        HttpUtility.HtmlDecode(CommandUtils.SanitizeHtml(value, true)):
                        HttpUtility.HtmlDecode(CommandUtils.SanitizeHtml(value));                
                default:
                    return HttpUtility.HtmlDecode(CommandUtils.SanitizeHtml(value, true));
            }
        }

        
        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var storedTranslations = dbContext.TranslationInstances
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList();
            dbContext.TranslationInstances.RemoveRange(storedTranslations);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class ImportExportQuestionnaireMapper : IImportExportQuestionnaireMapper
    {
        private readonly IMapper mapper;

        public ImportExportQuestionnaireMapper(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public Questionnaire Map(QuestionnaireDocument questionnaireDocument)
        {
            var idToVariableNameMap = GetIdToVariableNameMap(questionnaireDocument);
            var map = mapper.Map<Models.Questionnaire>(questionnaireDocument, opt => 
                opt.Items[ImportExportQuestionnaireConstants.MapCollectionName] = idToVariableNameMap);
            return map;
        }


        private Dictionary<Guid, string> GetIdToVariableNameMap(QuestionnaireDocument questionnaireDocument)
        {
            var map = new Dictionary<Guid, string>();
            questionnaireDocument.ForEachTreeElement<IComposite>(c => c.Children, (parent, child) =>
            {
                map[child.PublicKey] = child.VariableName;
            });
            return map;
        }

        public QuestionnaireDocument Map(Questionnaire questionnaire)
        {
            var variableNameToIdMap = GenerateVariableNameToIdMap(questionnaire);

            var questionnaireDocument = mapper.Map<QuestionnaireDocument>(questionnaire, opt => 
                opt.Items[ImportExportQuestionnaireConstants.MapCollectionName] = variableNameToIdMap);

            FixAfterMapping(questionnaireDocument);

            return questionnaireDocument;
        }

        private Dictionary<string, Guid> GenerateVariableNameToIdMap(Questionnaire questionnaire)
        {
            var map = new Dictionary<string, Guid>();

            var coverPageEntities = questionnaire.CoverPage?.TreeToEnumerable<QuestionnaireEntity>(x =>
            {
                if (x is CoverPage coverPage)
                    return coverPage.Children;
                return Enumerable.Empty<QuestionnaireEntity>();
            }) ?? Enumerable.Empty<QuestionnaireEntity>();
            var questionnaireEntities = questionnaire.Children.TreeToEnumerable<QuestionnaireEntity>(x =>
            {
                if (x is Group @group)
                    return @group.Children;
                return Enumerable.Empty<QuestionnaireEntity>();
            });
            var allEntries = coverPageEntities.Concat(questionnaireEntities);
            foreach (var entity in allEntries)
            {
                if (entity is IQuestion question && question.VariableName != null && !question.VariableName.IsNullOrEmpty())
                    map[question.VariableName] = question.Id ?? Guid.NewGuid();
                else if (entity is Group group && group.VariableName != null && !group.VariableName.IsNullOrEmpty())
                    map[group.VariableName] = group.Id ?? Guid.NewGuid();
                else if (entity is Variable variable && variable.VariableName != null && !variable.VariableName.IsNullOrEmpty())
                    map[variable.VariableName] = variable.Id ?? Guid.NewGuid();
            }
            return map;
        }

        private static void FixAfterMapping(QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument.ForEachTreeElement<IComposite>(c => c.Children, (parent, child) =>
            {
                if (child is Main.Core.Entities.SubEntities.IQuestion question)
                {
                    if (question.LinkedToQuestionId.HasValue &&
                        question.LinkedToQuestionId == question.LinkedToRosterId)
                    {
                        var linkedToQuestion = questionnaireDocument.Find<Main.Core.Entities.SubEntities.IQuestion>(
                            question.LinkedToQuestionId.Value);
                        question.LinkedToQuestionId = linkedToQuestion?.PublicKey;
                        var linkedToRoster = questionnaireDocument.Find<Main.Core.Entities.SubEntities.IGroup>(
                            question.LinkedToRosterId.Value);
                        question.LinkedToRosterId = linkedToRoster?.PublicKey;
                    }
                }
            });
        }
    }
}
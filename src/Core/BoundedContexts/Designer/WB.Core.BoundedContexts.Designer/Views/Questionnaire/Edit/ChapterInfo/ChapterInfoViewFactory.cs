using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class ChapterInfoViewFactory : IChapterInfoViewFactory
    {
        private readonly IQuestionTypeToCSharpTypeMapper questionnaireTypeMapper; 
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IReadOnlyList<VariableName> predefinedVariables = new List<VariableName>
        {
            new VariableName(null, "self", null),
            new VariableName(null, "@optioncode", "int"),
            new VariableName(null, "@rowindex", "int"),
            new VariableName(null, "@rowcode", "int")
        };

        public ChapterInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage, 
            IQuestionTypeToCSharpTypeMapper questionnaireTypeMapper)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireTypeMapper = questionnaireTypeMapper;
        }

        public NewChapterView Load(string questionnaireId, string chapterId)
        {
            var document = this.questionnaireStorage.GetById(questionnaireId).AsReadOnly();
            var chapterPublicKey = Guid.Parse(chapterId);
            var isExistsChapter = document.Find<IGroup>(chapterPublicKey)!=null;
            if (!isExistsChapter)
                return null;

            return new NewChapterView
            {
                Chapter = ConvertToChapterView(document, chapterPublicKey),
                VariableNames = this.CollectVariableNames(document)
            };
        }

        private IQuestionnaireItem ConvertToChapterView(ReadOnlyQuestionnaireDocument document, Guid chapterPublicKey)
        {
            var chapter = document.Find<IGroup>(chapterPublicKey);

            IQuestionnaireItem root = null;
            var allGroupViews = new Dictionary<IGroup, GroupInfoView>();
            chapter.ForEachTreeElement<IComposite>(x => x.Children, (parent, child) =>
            {

                IQuestionnaireItem questionnaireItem = null;

                if (child is IQuestion)
                {
                    questionnaireItem = this.ConvertToQuestionInfoView((IQuestion)child);
                }
                else if (child is IGroup)
                {
                    var groupItem = ConvertToGroupInfoView((IGroup)child);
                    allGroupViews.Add((IGroup)child, groupItem);
                    questionnaireItem = groupItem;
                }
                else if (child is IStaticText)
                {
                    questionnaireItem = this.ConvertToStaticTextInfoView((IStaticText)child);
                }
                else if (child is IVariable)
                {
                    questionnaireItem = this.ConvertToVariableInfoView((IVariable)child);
                }

                if (parent != null)
                {
                    allGroupViews.Last(x => x.Key == parent).Value.Items.Add(questionnaireItem);
                }
                else
                    root = questionnaireItem;
            });

            return root;
        }

        private VariableView ConvertToVariableInfoView(IVariable variable)
        {
            return new VariableView
            {
                Id = variable.PublicKey,
                ItemId = variable.PublicKey.FormatGuid(),
                VariableData = new VariableData(variable.Type, variable.Name, variable.Expression, variable.Label)
            };
        }

        private StaticTextInfoView ConvertToStaticTextInfoView(IStaticText staticText)
        {
            return new StaticTextInfoView
            {
                ItemId = staticText.PublicKey.FormatGuid(),
                Text = staticText.Text,
                AttachmentName = staticText.AttachmentName,
                HasCondition = !string.IsNullOrWhiteSpace(staticText.ConditionExpression),
                HasValidation = staticText.ValidationConditions.Count > 0
            };
        }

        private GroupInfoView ConvertToGroupInfoView(IGroup group)
        {
            return new GroupInfoView
            {
                ItemId = group.PublicKey.FormatGuid(),
                Title = group.Title,
                IsRoster = group.IsRoster,
                HasCondition = !string.IsNullOrWhiteSpace(group.ConditionExpression),
                Variable = group.VariableName,
                Items = new List<IQuestionnaireItem>(),
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };
        }

        private QuestionInfoView ConvertToQuestionInfoView(IQuestion question)
        {
            return new QuestionInfoView
            {
                ItemId = question.PublicKey.FormatGuid(),
                Title = question.QuestionText,
                Variable = question.StataExportCaption,
                HasCondition = !string.IsNullOrWhiteSpace(question.ConditionExpression),
                HasValidation = question.ValidationConditions.Count > 0,
                Type = question.QuestionType,
                LinkedToQuestionId = question.LinkedToQuestionId?.FormatGuid(),
                LinkedToRosterId = question.LinkedToRosterId?.FormatGuid(),
                LinkedFilterExpression = question.LinkedFilterExpression
            };
        }

        private VariableName[] CollectVariableNames(ReadOnlyQuestionnaireDocument document)
        {
            List<VariableName> variables = predefinedVariables.ToList();

            var questionnaireItems = document.Find<IComposite>();

            var variableNames = questionnaireItems
                .Select(x => new VariableName(x.PublicKey.FormatGuid(), x.GetVariable(), GetQuestionType(x, document)))
                .Where(variableName => !string.IsNullOrWhiteSpace(variableName.Name))
                .ToList();

            variables.AddRange(variableNames);

            return variables.ToArray();
        }

        public string GetQuestionType(IComposite entity, ReadOnlyQuestionnaireDocument questionnaire)
        {
            var variable = entity as IVariable;
            if (variable != null) return questionnaireTypeMapper.GetVariableType(variable.Type);

            var question = entity as IQuestion;
            if (question!=null) return questionnaireTypeMapper.GetQuestionType(question, questionnaire).Replace(typeof(YesNoAndAnswersMissings).Name, "YesNoAnswers");

            return entity is IGroup ? "Roster" : null;
        }
    }
}
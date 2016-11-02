using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class ChapterInfoViewFactory : IChapterInfoViewFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly string[] predefinedVariables = {"self", "@optioncode", "@rowindex", "@rowname", "@rowcode" };


        public ChapterInfoViewFactory(IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public NewChapterView Load(string questionnaireId, string chapterId)
        {
            var document = this.questionnaireStorage.GetById(questionnaireId);
            var chapterPublicKey = Guid.Parse(chapterId);
            var isExistsChapter = document.Children.Any(c => c.PublicKey == chapterPublicKey);
            if (!isExistsChapter)
                return null;

            return new NewChapterView
            {
                Chapter = ConvertToChapterView(document, chapterPublicKey),
                VariableNames = this.CollectVariableNames(document)
            };
        }

        private IQuestionnaireItem ConvertToChapterView(QuestionnaireDocument document, Guid chapterPublicKey)
        {
            var chapter = (IGroup)document.Children.Single(c => c.PublicKey == chapterPublicKey);

            IQuestionnaireItem root = null;
            var allGroupViews = new Dictionary<Guid, GroupInfoView>();
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
                    allGroupViews.Add(child.PublicKey, groupItem);
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
                    allGroupViews[parent.PublicKey].Items.Add(questionnaireItem);
                else
                    root = questionnaireItem;
            });

            return root;
        }

        private VariableView ConvertToVariableInfoView(IVariable variable)
        {
            return new VariableView()
            {
                Id = variable.PublicKey,
                ItemId = variable.PublicKey.FormatGuid(),
                VariableData = new VariableData(variable.Type, variable.Name, variable.Expression, variable.Description),
            };
        }

        private StaticTextInfoView ConvertToStaticTextInfoView(IStaticText staticText)
        {
            return new StaticTextInfoView()
            {
                ItemId = staticText.PublicKey.FormatGuid(),
                Text = staticText.Text,
                AttachmentName = staticText.AttachmentName,
                HasCondition = !string.IsNullOrWhiteSpace(staticText.ConditionExpression),
                HasValidation = staticText.ValidationConditions.Count > 0,
            };
        }

        private GroupInfoView ConvertToGroupInfoView(IGroup group)
        {
            return new GroupInfoView()
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
            return new QuestionInfoView()
            {
                ItemId = question.PublicKey.FormatGuid(),
                Title = question.QuestionText,
                Variable = question.StataExportCaption,
                HasCondition = !string.IsNullOrWhiteSpace(question.ConditionExpression),
                HasValidation = question.ValidationConditions.Count > 0,
                Type = question.QuestionType,
                LinkedToQuestionId = question.LinkedToQuestionId?.FormatGuid(),
                LinkedToRosterId = question.LinkedToRosterId?.FormatGuid(),
                LinkedFilterExpression = question.LinkedFilterExpression,
            };
        }

        private VariableName[] CollectVariableNames(QuestionnaireDocument document)
        {
            List<VariableName> variables = predefinedVariables.Select(x => new VariableName(null, x)).ToList();

            var questionnaireItems = document.TreeToEnumerable<IComposite>(x => x.Children);

            var variableNames = questionnaireItems
                .Select(x => new VariableName(x.PublicKey.FormatGuid(), x.GetVariable()))
                .Where(variableName => !string.IsNullOrWhiteSpace(variableName.Name))
                .ToList();

            variables.AddRange(variableNames);

            return variables.ToArray();
        }
    }
}
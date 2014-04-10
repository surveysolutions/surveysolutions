using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class ChapterInfoViewFactory : IViewFactory<ChapterInfoViewInputModel, ChapterInfoView>
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage;
        private readonly IExpressionProcessor expressionProcessor;

        public ChapterInfoViewFactory(IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage,
            IExpressionProcessor expressionProcessor)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.expressionProcessor = expressionProcessor;
        }

        public ChapterInfoView Load(ChapterInfoViewInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetById(input.QuestionnaireId);
            if (questionnaire == null) return null;

            var chapter =
                questionnaire.FirstOrDefault<IGroup>(group => group.PublicKey.FormatGuid().Equals(input.ChapterId));
            if (chapter == null) return null;

            var chapterInfoView = new ChapterInfoView {Title = chapter.Title, GroupId = chapter.PublicKey.FormatGuid()};

            FillGroupsFromChapter(questionnaire: questionnaire, chapterOrGroup: chapter,
                currentGroupInfoView: chapterInfoView);

            return chapterInfoView;
        }

        private void FillGroupsFromChapter(QuestionnaireDocument questionnaire, IGroup chapterOrGroup,
             GroupInfoView currentGroupInfoView)
        {
            foreach (var groupOrQuestion in chapterOrGroup.Children)
            {
                var group = groupOrQuestion as IGroup;
                if (group != null)
                {
                    var childGroupInfoView = new GroupInfoView(group.IsRoster)
                    {
                        GroupId = group.PublicKey.FormatGuid(),
                        Title = group.Title
                    };
                    currentGroupInfoView.Groups.Add(childGroupInfoView);

                    FillGroupsFromChapter(questionnaire: questionnaire, chapterOrGroup: group,
                        currentGroupInfoView:  childGroupInfoView);
                }

                var question = groupOrQuestion as IQuestion;
                if (question != null)
                {
                    var questionsUsedInConditionExpression = string.IsNullOrEmpty(question.ConditionExpression)
                        ? new string[0]
                        : expressionProcessor.GetIdentifiersUsedInExpression(question.ConditionExpression);

                    currentGroupInfoView.Questions.Add(new QuestionInfoView()
                    {
                        QuestionId = question.PublicKey.FormatGuid(),
                        Title = question.QuestionText,
                        Variable = question.StataExportCaption,
                        Type = question.QuestionType,
                        LinkedVariables = questionsUsedInConditionExpression,
                        BrokenLinkedVariables =
                            questionsUsedInConditionExpression.Where(
                                variable =>
                                    questionnaire.FirstOrDefault<IQuestion>(
                                        dependentQuestion => dependentQuestion.StataExportCaption.Equals(variable)) ==
                                    null)
                    });
                }
            }
        }
    }
}
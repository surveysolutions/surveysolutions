using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class ChapterInfoViewFactory : IChapterInfoViewFactory
    {
        private readonly string[] predefinedVariables = {"self"};

        private readonly IReadSideKeyValueStorage<GroupInfoView> readSideReader;

        public ChapterInfoViewFactory(IReadSideKeyValueStorage<GroupInfoView> readSideReader)
        {
            this.readSideReader = readSideReader;
        }

        public NewChapterView Load(string questionnaireId, string groupId)
        {
            var questionnaire = this.readSideReader.GetById(questionnaireId);

            var chapterItem = questionnaire?.Items.Find(chapter => chapter.ItemId == groupId);
            if (chapterItem == null)
                return null;

            return new NewChapterView
            {
                Chapter = chapterItem,
                VariableNames = this.CollectVariableNames(questionnaire)
            };
        }

        private string[] CollectVariableNames(GroupInfoView questionnaire)
        {
            List<string> variables = new List<string>(predefinedVariables);

            var nodes = new Stack<IQuestionnaireItem>(new[] { questionnaire });
            while (nodes.Any())
            {
                IQuestionnaireItem node = nodes.Pop();
                var nodeAsQuestionInfoView = node as QuestionInfoView;
                if (nodeAsQuestionInfoView != null)
                {
                    if (!string.IsNullOrWhiteSpace(nodeAsQuestionInfoView.Variable))
                        variables.Add(nodeAsQuestionInfoView.Variable);
                    continue;
                }

                var nodeAsGroupInfoView = node as GroupInfoView;
                if (nodeAsGroupInfoView == null)
                    continue;
                if(!string.IsNullOrWhiteSpace(nodeAsGroupInfoView.Variable))
                    variables.Add(nodeAsGroupInfoView.Variable);

                foreach (var item in nodeAsGroupInfoView.Items) nodes.Push(item);
            }

            return variables.Distinct().ToArray();
        }
    }
}
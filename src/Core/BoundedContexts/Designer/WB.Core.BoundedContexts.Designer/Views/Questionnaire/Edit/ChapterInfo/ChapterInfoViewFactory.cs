using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
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

            variables.AddRange(questionnaire.TreeToEnumerable<IQuestionnaireItem>(x => x.Items)
                .OfType<INameable>().Where(z => !string.IsNullOrEmpty(z.Variable)).Select(y => y.Variable).ToList());

            return variables.Distinct().ToArray();
        }
    }
}
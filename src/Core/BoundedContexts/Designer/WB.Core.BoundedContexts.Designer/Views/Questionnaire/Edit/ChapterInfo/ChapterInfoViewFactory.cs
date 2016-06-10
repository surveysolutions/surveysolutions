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

        private VariableName[] CollectVariableNames(GroupInfoView questionnaire)
        {
            List<VariableName> variables = predefinedVariables.Select(x => new VariableName(null, x)).ToList();

            variables.AddRange(questionnaire.TreeToEnumerable<IQuestionnaireItem>(x => x.Items)
                .Where(z => !string.IsNullOrEmpty((z as INameable)?.Variable))
                .Select(y => new VariableName(y.ItemId, ((INameable)y).Variable))
                .ToList());

            return variables.ToArray();
        }
    }
}
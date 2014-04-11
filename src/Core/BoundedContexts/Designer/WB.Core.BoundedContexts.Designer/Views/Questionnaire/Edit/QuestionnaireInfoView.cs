using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireInfoView : QuestionnaireItemMetaInfoView
    {
        public readonly IEnumerable<GroupMetaInfoView> Chapters;

        public QuestionnaireInfoView(IEnumerable<GroupInfoView> chapters)
            : base(
                groups: chapters.SelectMany(chapter => chapter.Groups).ToList(),
                questions: chapters.SelectMany(chapter => chapter.Questions).ToList())
        {
            Chapters =
                chapters.Select(
                    chapter =>
                        new GroupMetaInfoView(chapter.Groups, chapter.Questions)
                        {
                            GroupId = chapter.GroupId,
                            Title = chapter.Title
                        });
        }
    }
}

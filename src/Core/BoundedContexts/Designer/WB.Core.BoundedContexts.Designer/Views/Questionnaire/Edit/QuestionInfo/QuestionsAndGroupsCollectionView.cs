using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class QuestionsAndGroupsCollectionView : IReadSideRepositoryEntity
    {
        public List<QuestionDetailsView> Questions { get; set; }
        public List<GroupAndRosterDetailsView> Groups { get; set; }
        public List<StaticTextDetailsView> StaticTexts { get; set; }
    }
}
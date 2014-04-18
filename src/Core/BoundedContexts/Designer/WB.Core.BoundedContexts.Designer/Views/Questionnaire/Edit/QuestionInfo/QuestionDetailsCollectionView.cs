using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class QuestionDetailsCollectionView : IReadSideRepositoryEntity
    {
        public List<QuestionDetailsView> Questions { get; set; }
    }
}
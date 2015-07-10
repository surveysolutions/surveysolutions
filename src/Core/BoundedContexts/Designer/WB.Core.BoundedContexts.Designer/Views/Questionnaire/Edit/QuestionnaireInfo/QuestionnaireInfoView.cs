using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoView : IReadSideRepositoryEntity
    {
        public QuestionnaireInfoView()
        {
            this.SharedPersons = new List<SharedPerson>();
            this.Chapters = new List<ChapterInfoView>();
        }
        public string QuestionnaireId { get; set; }
        public string Title { get; set; }
        public bool IsPublic { get; set; }
        public List<ChapterInfoView> Chapters { get; set; }
        public List<SharedPerson> SharedPersons { get; set; }
        public int QuestionsCount { get; set; }
        public int GroupsCount { get; set; }
        public int RostersCount { get; set; }

        public bool IsReadOnlyForUser { get; set; }
    }
}

using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Web.Supervisor.Models
{
    public class AssigmentModel
    {
        public int ColumnsCount { get; set; }
        public UserLight Responsible { get; set; }
        public string CompleteQuestionnaireId { get; set; }
        public int FeaturedCount { get; set; }
    }
}
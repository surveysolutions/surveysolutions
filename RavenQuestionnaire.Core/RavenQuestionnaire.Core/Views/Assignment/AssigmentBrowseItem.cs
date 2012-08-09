using RavenQuestionnaire.Core.Entities.SubEntities;


namespace RavenQuestionnaire.Core.Views.Assignment
{
    public class AssigmentBrowseItem
    {
        public string Id { get; set; }
        public string TemplateId { get; private set; }
        public SurveyStatus Status { get; private set; }
        public UserLight Responsible { get; set; }
        public int Count { get; set; }

        public AssigmentBrowseItem()
        {
        }

        public AssigmentBrowseItem(string id, SurveyStatus status, string templateId, int count, UserLight responsible)
        {
            this.Id = id;
            this.Status = status;
            this.TemplateId = templateId;
            this.Count = count;
            this.Responsible = responsible;
        }
    }
}

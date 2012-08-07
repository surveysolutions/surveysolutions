using RavenQuestionnaire.Core.Entities.SubEntities;


namespace RavenQuestionnaire.Core.Views.Assignment
{
    public class AssigmentBrowseItem
    {
        public string Id { get; set; }
        public string Adress { get; private set; }
        public string ResponsibleId { get; private set; }
        public SurveyStatus Status { get; private set; }

        public AssigmentBrowseItem()
        {
        }

        public AssigmentBrowseItem(string id, string adress, SurveyStatus status)
        {
            this.Id = id;
            this.Adress = adress;
            this.Status = status;
        }

        public AssigmentBrowseItem(string id, string adress, string responsibleId, SurveyStatus status)
        {
            this.Id = id;
            this.Adress = adress;
            this.ResponsibleId = responsibleId;
            this.Status = status;
        }

    }
}

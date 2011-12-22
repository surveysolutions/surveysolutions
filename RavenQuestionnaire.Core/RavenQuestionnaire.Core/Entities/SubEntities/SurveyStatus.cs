namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class SurveyStatus
    {

        public SurveyStatus()
        {
        }

        public SurveyStatus(string id, string name)
        {
            Id = id;
            Name = name;
        }


        public string Id { get; set; }
        public string Name { get; set; }
    }
}

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class UserLight
    {
        public UserLight(){}

        public UserLight(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}

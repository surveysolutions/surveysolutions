namespace RavenQuestionnaire.Core.Entities
{
    public class OrderRequestItem
    {
        public string Field { get; set; }
        public OrderDirection Direction { get; set; }
    }

    public enum OrderDirection
    {
        Asc = 0,
        Desc = 1
    }
}
namespace Main.Core.Entities.SubEntities.Question
{
    public enum Quality
    {
        Lowest = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Highest = 4
    }

    public class AudioQuestion : ExternalServiceQuestion
    {
        public AudioQuestion()
        {
            this.Quality = Quality.Medium;
        }

        public Quality Quality { get; set; }
    }
}
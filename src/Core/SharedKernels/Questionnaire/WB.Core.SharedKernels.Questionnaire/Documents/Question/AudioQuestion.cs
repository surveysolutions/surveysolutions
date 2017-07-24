using WB.Core.SharedKernels.Questionnaire.Documents.Question;

namespace Main.Core.Entities.SubEntities.Question
{
    public class AudioQuestion : ExternalServiceQuestion
    {
        public AudioQuestion()
        {
            this.Quality = AudioQuality.Default;
        }

        public AudioQuality Quality { get; set; }
    }
}
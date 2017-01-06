using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewTextQuestion : GenericQuestion
    {
        public string QuestionIdentity { get; set; }
        public string Title { get; set; }
    }

    public abstract class GenericQuestion
    {
        public string Instructions { get; set; }
        public bool HideInstructions { get; set; }
    }
}
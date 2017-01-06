using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewTextQuestion : GenericQuestion
    {
    }

    public class InterviewSingleOptionQuestion : CategoricalQuestion
    {
    }

    public class CategoricalQuestion: GenericQuestion
    {
        public List<CategoricalOption> Options { get; set; }
    }

    public abstract class GenericQuestion
    {
        public string Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public string QuestionIdentity { get; set; }
        public string Title { get; set; }
    }

}
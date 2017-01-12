using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewTextQuestion : GenericQuestion
    {
        public string Mask { get; set; }
        public string Answer { get; set; }
    }

    public class InterviewSingleOptionQuestion : CategoricalQuestion
    {
        public int? Answer { get; set; }
    }

    public class CategoricalQuestion: GenericQuestion
    {
        public List<CategoricalOption> Options { get; set; }
    }

    public abstract class GenericQuestion : InterviewEntity
    {
        public string Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public bool IsAnswered { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public class Validity
    {
        public bool IsValid { get; set; }
        public string[] Messages { get; set; }
    }

    public abstract class InterviewEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsDisabled { get; set; }
        public bool HideIfDisabled { get; set; }
    }


    public class InterviewStaticText : InterviewEntity
    {
        public string AttachmentName { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    /// <summary>
    /// Used during dev, should be deleted when all types of questions are implemented
    /// </summary>
    public class StubEntity : GenericQuestion
    {
    }
}
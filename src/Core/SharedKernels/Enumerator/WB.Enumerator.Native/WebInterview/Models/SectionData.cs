using System.Collections.Generic;

namespace WB.Enumerator.Native.WebInterview.Models
{
    public class SectionData
    {
        public InterviewEntityWithType[] Entities { get; set; }
        public InterviewEntity[] Details { get; set; }
        public Dictionary<string, string> VariableNames { get; set; }
    }
}
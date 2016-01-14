using System;

namespace WB.UI.Headquarters.Services
{
    public class InterviewImportPrefilledQuestion
    {
        public Guid QuestionId { get; set; }
        public string Variable { get; set; }
        public bool IsGps { get; set; }
        public Type AnswerType { get; set; }
    }
}
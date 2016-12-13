using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int ChaptersCount { get; set; }
        public int GroupsCount { get; set; }
        public int RostersCount { get; set; }
        public int QuestionsCount { get; set; }
        public int QuestionsWithConditionsCount { get; set; }
    }
}
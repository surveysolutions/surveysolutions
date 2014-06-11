using System;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Models
{
    public class FeaturedAnswer
    {
        public Guid Id { get; set; }
        public QuestionType Type { get; set; }
        public string Answer { get; set; }
        public Guid[] Answers { get; set; }
    }

    public class AssignSuveyData
    {
        public Guid QuestionnaireId { get; set; }
        public UserLight Responsible { get; set; }
        public FeaturedAnswer[] Answers { get; set; }
    }
}
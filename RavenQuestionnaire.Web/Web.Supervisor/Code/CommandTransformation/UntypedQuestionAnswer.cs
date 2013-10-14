using System;
using Main.Core.Entities.SubEntities;

namespace Web.Supervisor.Code.CommandTransformation
{
    internal class UntypedQuestionAnswer
    {
        public Guid Id { get; set; }
        public object Answer { get; set; }
        public QuestionType Type { get; set; }
        public dynamic Settings { get; set; }
    }
}
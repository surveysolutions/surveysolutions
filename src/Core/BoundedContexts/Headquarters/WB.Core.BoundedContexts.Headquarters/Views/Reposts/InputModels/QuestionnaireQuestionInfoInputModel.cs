using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class QuestionnaireQuestionInfoInputModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public QuestionType? QuestionType { get; set; }
    }
}

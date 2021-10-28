using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class MultiOptionsQuestion : AbstractQuestion, ICategoricalQuestion, ILinkedQuestion
    {
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
        public bool YesNoView { get; set; }
        public Guid? CategoriesId { get; set; }
        public List<Answer>? Answers { get; set; }
        public Guid? LinkedToId { get; set; }
        public string? FilterExpression { get; set; }
    }
}

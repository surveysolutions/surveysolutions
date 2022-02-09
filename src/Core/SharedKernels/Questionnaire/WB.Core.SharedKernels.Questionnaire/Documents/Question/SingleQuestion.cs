using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.SubEntities.Question
{
    /// <summary>
    /// Single option question
    /// </summary>
    public class SingleQuestion : AbstractQuestion, ICategoricalQuestion
    {
        public SingleQuestion(string? questionText = null, List<IComposite>? children = null):base(questionText, children){ }

        public override QuestionType QuestionType => QuestionType.SingleOption;

        public bool ShowAsList { get; set; }
        public int? ShowAsListThreshold { get; set; }
        public Guid? CategoriesId { get; set; }
    }
}

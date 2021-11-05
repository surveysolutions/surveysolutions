using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class MultyOptionsQuestion : AbstractQuestion, IMultyOptionsQuestion
    {
        public MultyOptionsQuestion(string? questionText = null, List<IComposite>? children = null):base(questionText, children){ }

        public override QuestionType QuestionType => QuestionType.MultyOption;

        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
        public bool YesNoView { get; set; }
        public Guid? CategoriesId { get; set; }
    }
}

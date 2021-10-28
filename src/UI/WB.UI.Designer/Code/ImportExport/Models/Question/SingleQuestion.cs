using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    /// <summary>
    /// Single option question
    /// </summary>
    public class SingleQuestion : AbstractQuestion, ICategoricalQuestion, ILinkedQuestion
    {
        public bool ShowAsList { get; set; }
        public int? ShowAsListThreshold { get; set; }
        public Guid? CategoriesId { get; set; }
        public List<Answer>? Answers { get; set; }
        public bool? IsFilteredCombobox { get; set; }
        public string? FilterExpression { get; set; }
        public Guid? CascadeFromQuestionId { get; set; }
        public Guid? LinkedToRosterId { get; set; }
        public Guid? LinkedToQuestionId { get; set; }
    }
}

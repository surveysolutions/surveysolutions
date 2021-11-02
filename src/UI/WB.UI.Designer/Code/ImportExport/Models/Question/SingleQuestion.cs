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
        public SingleOptionDisplayMode DisplayMode { get; set; }
        public Guid? CascadeFromQuestionId { get; set; }
        public Guid? LinkedToId { get; set; }
        public string? FilterExpression { get; set; }
    }
    
    public enum SingleOptionDisplayMode
    {
        Radio,
        Combobox,
        Cascading,
    }
}

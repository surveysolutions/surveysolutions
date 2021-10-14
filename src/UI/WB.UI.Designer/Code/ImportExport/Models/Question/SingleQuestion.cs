using System;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    /// <summary>
    /// Single option question
    /// </summary>
    public class SingleQuestion : AbstractQuestion, ICategoricalQuestion
    {
        public bool ShowAsList { get; set; }
        public int? ShowAsListThreshold { get; set; }
        public Guid? CategoriesId { get; set; }
    }
}

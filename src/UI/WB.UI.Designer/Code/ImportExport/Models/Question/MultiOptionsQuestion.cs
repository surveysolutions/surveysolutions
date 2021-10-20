using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class MultiOptionsQuestion : AbstractQuestion
    {
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
        public bool YesNoView { get; set; }
        public Guid? CategoriesId { get; set; }
    }
}

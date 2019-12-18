using System;

namespace WB.Services.Export.Questionnaire
{
    public class MultyOptionsQuestion : Question, ICategoricalQuestion
    {
        public bool YesNoView { get; set; }
        public bool AreAnswersOrdered { get; set; }
        public bool? IsFilteredCombobox { get; set; }
        public Guid? CategoriesId { get; set; }
    }
}

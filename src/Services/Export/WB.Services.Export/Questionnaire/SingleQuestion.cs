using System;

namespace WB.Services.Export.Questionnaire
{
    public class SingleQuestion : Question, ICategoricalQuestion
    {
        public Guid? CategoriesId { get; set; }
    }
}

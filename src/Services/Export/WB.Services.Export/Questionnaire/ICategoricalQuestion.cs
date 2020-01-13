using System;

namespace WB.Services.Export.Questionnaire
{
    public interface ICategoricalQuestion
    {
        Guid? CategoriesId { get; }
    }
}

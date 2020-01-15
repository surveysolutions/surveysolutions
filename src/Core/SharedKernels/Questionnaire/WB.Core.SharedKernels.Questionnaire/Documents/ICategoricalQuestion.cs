using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public interface ICategoricalQuestion : IQuestion
    {
        Guid? CategoriesId { get; set; }
    }
}

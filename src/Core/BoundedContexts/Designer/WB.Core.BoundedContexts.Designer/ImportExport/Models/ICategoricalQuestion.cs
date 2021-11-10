using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public interface ICategoricalQuestion : IQuestion
    {
        Guid? CategoriesId { get; set; }
    }
}

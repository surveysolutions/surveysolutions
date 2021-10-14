using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface ICategoricalQuestion : IQuestion
    {
        Guid? CategoriesId { get; set; }
    }
}

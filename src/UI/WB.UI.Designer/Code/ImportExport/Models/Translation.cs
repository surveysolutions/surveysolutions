using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class Translation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;

        public Translation Clone()
        {
            return new Translation
            {
                Id = this.Id,
                Name = this.Name,
            };
        }
    }
}

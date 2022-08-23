using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Categories
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string? AttachmentName { get; set; }

        public Categories Clone() => new Categories
        {
            Id = this.Id,
            Name = this.Name,
            AttachmentName = this.AttachmentName
        };
    }
}

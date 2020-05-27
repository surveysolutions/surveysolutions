using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Macro
    {
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;

        public Macro Clone()
        {
            return new Macro
            {
                Name = this.Name,
                Description = this.Description,
                Content = this.Content
            };
        }
    }
}

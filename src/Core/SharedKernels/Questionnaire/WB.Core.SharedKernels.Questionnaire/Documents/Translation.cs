using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Translation
    {
        public Guid TranslationId { get; set; }
        public string Name { get; set; }

        public Translation Clone()
        {
            return new Translation
            {
                TranslationId = this.TranslationId,
                Name = this.Name,
            };
        }
    }
}
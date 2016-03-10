using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveySolutions.Api.Designer
{
    public class AttachmentDto
    {
         public Attachment Metadata { get; set; }
         public byte[] Content { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.BinaryData
{
    public class InterviewBinaryData
    {
        public InterviewBinaryData(Guid interviewId, string id, byte[] data)
        {
            this.InterviewId = interviewId;
            this.Id = id;
            this.Data = data;
        }
        public Guid InterviewId { get; private set; }
        public string Id { get; private set; }
        public byte[] Data { get; private set; }
    }
}

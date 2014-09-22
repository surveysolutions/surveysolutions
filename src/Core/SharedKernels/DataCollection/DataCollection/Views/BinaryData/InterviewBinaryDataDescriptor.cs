using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.BinaryData
{
    public class InterviewBinaryDataDescriptor
    {
        public InterviewBinaryDataDescriptor(Guid interviewId, string fileName, Func<byte[]> getData)
        {
            this.InterviewId = interviewId;
            this.FileName = fileName;
            this.getData = getData;
        }
        public Guid InterviewId { get; private set; }
        public string FileName { get; private set; }
        public byte[] Data { get { return getData(); } }
        private readonly Func<byte[]> getData;
    }
}

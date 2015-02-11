using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization
{
    public class IncomingSyncPackageException:Exception
    {
        public IncomingSyncPackageException(Guid? interviewId, string pathToPackage)
        {
            InterviewId = interviewId;
            PathToPackage = pathToPackage;
        }

        public IncomingSyncPackageException(string message, Guid? interviewId, string pathToPackage) : base(message)
        {
            InterviewId = interviewId;
            PathToPackage = pathToPackage;
        }

        public IncomingSyncPackageException(string message, Exception innerException, Guid? interviewId, string pathToPackage) : base(message, innerException)
        {
            InterviewId = interviewId;
            PathToPackage = pathToPackage;
        }

        public Guid? InterviewId { get; private set; }
        public string PathToPackage { get; private set; }
    }
}

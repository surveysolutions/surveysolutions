using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class MarkAssignmentAsReceivedByTablet : AssignmentCommand
    {
        public string DeviceId { get; set; }
        
        public MarkAssignmentAsReceivedByTablet(Guid publicKey, Guid userId, string deviceId, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            this.DeviceId = deviceId;
        }
    }
}

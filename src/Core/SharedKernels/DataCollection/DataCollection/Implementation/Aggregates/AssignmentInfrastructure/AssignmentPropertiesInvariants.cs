using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.AssignmentInfrastructure
{
    public class AssignmentPropertiesInvariants
    {
        public class ExceptionKeys
        {
            public static readonly string AssignmentId = "Assignment ID";
            public static readonly string AssignmentPublicKey = "Assignment PublicKey";
        }

        private readonly AssignmentProperties properties;

        public AssignmentPropertiesInvariants(AssignmentProperties properties)
        {
            this.properties = properties;
        }

        public void ThrowIfAssignmentDeleted()
        {
            if (this.properties.IsDeleted)
                throw new AssignmentException(
                    $"Assignment status is deleted",
                    AssignmentDomainExceptionType.AssignmentDeleted)
                {
                    Data =
                    {
                        {ExceptionKeys.AssignmentId, this.properties.Id},
                        {ExceptionKeys.AssignmentPublicKey, this.properties.PublicKey},
                    }
                };
        }

        public void ThrowIfCannotComplete()
        {
            if (this.properties.Status != AssignmentStatus.Open)
                throw new AssignmentException(
                    $"Assignment can only be completed from Open status. Current status: {this.properties.Status}",
                    AssignmentDomainExceptionType.InvalidStatusTransition)
                {
                    Data =
                    {
                        {ExceptionKeys.AssignmentId, this.properties.Id},
                        {ExceptionKeys.AssignmentPublicKey, this.properties.PublicKey},
                    }
                };
        }

        public void ThrowIfCannotClose()
        {
            if (this.properties.Status != AssignmentStatus.Open && this.properties.Status != AssignmentStatus.Completed)
                throw new AssignmentException(
                    $"Assignment can only be closed from Open and Completed status. Current status: {this.properties.Status}",
                    AssignmentDomainExceptionType.InvalidStatusTransition)
                {
                    Data =
                    {
                        {ExceptionKeys.AssignmentId, this.properties.Id},
                        {ExceptionKeys.AssignmentPublicKey, this.properties.PublicKey},
                    }
                };
        }

        public void ThrowIfCannotReopen()
        {
            if (this.properties.Status != AssignmentStatus.Completed && this.properties.Status != AssignmentStatus.Closed)
                throw new AssignmentException(
                    $"Assignment can only be reopened from Completed or Closed status. Current status: {this.properties.Status}",
                    AssignmentDomainExceptionType.InvalidStatusTransition)
                {
                    Data =
                    {
                        {ExceptionKeys.AssignmentId, this.properties.Id},
                        {ExceptionKeys.AssignmentPublicKey, this.properties.PublicKey},
                    }
                };
        }
    }
}

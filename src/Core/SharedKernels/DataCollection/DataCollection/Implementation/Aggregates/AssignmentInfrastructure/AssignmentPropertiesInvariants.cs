using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

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
    }
}

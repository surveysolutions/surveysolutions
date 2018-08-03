using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public interface IAssignmentDocumentFromDtoBuilder
    {
        AssignmentDocument GetAssignmentDocument(AssignmentApiDocument remote, IQuestionnaire questionnaire);
    }
}
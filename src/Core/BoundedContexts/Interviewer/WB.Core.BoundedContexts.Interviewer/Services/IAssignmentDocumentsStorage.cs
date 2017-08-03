using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IAssignmentDocumentsStorage
    {
        void Store(AssignmentDocument documents);
        void Store(IEnumerable<AssignmentDocument> documents);
        IReadOnlyCollection<AssignmentDocument> LoadAll();
        void RemoveAll();
        void Remove(int assignmentId);
        
        AssignmentDocument FetchPreloadedData(AssignmentDocument document);
        AssignmentDocument GetById(int assignmentId);
    }
}
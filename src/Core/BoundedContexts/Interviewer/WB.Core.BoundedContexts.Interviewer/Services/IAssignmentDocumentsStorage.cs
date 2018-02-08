using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        int Count(Expression<Func<AssignmentDocument, bool>> predicate);
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAssignmentDocumentsStorage
    {
        void Store(AssignmentDocument documents);
        void Store(IEnumerable<AssignmentDocument> documents);
        IReadOnlyCollection<AssignmentDocument> LoadAll();
        IReadOnlyCollection<AssignmentDocument> LoadAll(Guid? responsibleId);
        void RemoveAll();
        void Remove(int assignmentId);
        
        AssignmentDocument FetchPreloadedData(AssignmentDocument document);
        AssignmentDocument GetById(int assignmentId);

        int Count(Expression<Func<AssignmentDocument, bool>> predicate);

        int Count();
        void DecreaseInterviewsCount(int assignmentId);
    }
}

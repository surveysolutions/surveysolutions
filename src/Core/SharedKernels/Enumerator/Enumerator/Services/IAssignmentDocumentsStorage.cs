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

        IEnumerable<AssignmentDocument> Query(Expression<Func<AssignmentDocument, bool>> query);

        void RemoveAll();
        void Remove(int assignmentId);
        void Remove(params int[] assignmentIds);


        AssignmentDocument FetchPreloadedData(AssignmentDocument document);
        AssignmentDocument GetById(int assignmentId);

        int Count(Expression<Func<AssignmentDocument, bool>> predicate);

        int Count();
        void DecreaseInterviewsCount(int assignmentId);
    }
}

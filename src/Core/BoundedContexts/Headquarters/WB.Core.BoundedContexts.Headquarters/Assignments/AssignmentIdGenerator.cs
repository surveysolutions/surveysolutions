using System;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    class AssignmentIdGenerator : IAssignmentIdGenerator
    {
        private readonly IUnitOfWork unitOfWork;

        public AssignmentIdGenerator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public int GetNextDisplayId()
        {
            string sql = "SELECT nextval('assignment_id_sequence');";
            var sqlQuery = unitOfWork.Session.CreateSQLQuery(sql);
            var nextDisplayId = sqlQuery.UniqueResult();
            return Convert.ToInt32(nextDisplayId);
        }
    }
}

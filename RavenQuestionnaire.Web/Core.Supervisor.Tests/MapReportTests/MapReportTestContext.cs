using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Reposts.Factories;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Tests.MapReportTests
{
    internal class MapReportTestContext
    {
        protected static MapReport CreateMapReport(IReadSideRepositoryReader<AnswersByVariableCollection> answersByVariableStorage = null)
        {
            return new MapReport(answersByVariableStorage ?? Mock.Of<IReadSideRepositoryReader<AnswersByVariableCollection>>());
        }
    }
}

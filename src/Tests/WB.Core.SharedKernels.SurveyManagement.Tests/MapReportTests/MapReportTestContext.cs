using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.MapReportTests
{
    internal class MapReportTestContext
    {
        protected static MapReport CreateMapReport(IReadSideRepositoryReader<AnswersByVariableCollection> answersByVariableStorage = null)
        {
            return new MapReport(answersByVariableStorage ?? Mock.Of<IReadSideRepositoryReader<AnswersByVariableCollection>>());
        }
    }
}

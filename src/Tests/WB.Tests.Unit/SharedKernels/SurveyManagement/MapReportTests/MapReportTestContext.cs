using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    internal class MapReportTestContext
    {
        protected static MapReport CreateMapReport(IReadSideKeyValueStorage<AnswersByVariableCollection> answersByVariableStorage = null)
        {
            return new MapReport(answersByVariableStorage ?? Mock.Of<IReadSideKeyValueStorage<AnswersByVariableCollection>>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    [Subject(typeof(ChartStatisticsViewFactory))]
    internal class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(StatisticsGroupedByDateAndTemplate data)
        {
            var statsMock = Mock.Of<IReadSideRepositoryReader<StatisticsGroupedByDateAndTemplate>>(_ => _.GetById(Moq.It.IsAny<string>())==data);
            return new ChartStatisticsViewFactory(statsMock);
        }
    }

}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(IReadSideRepositoryReader<StatisticsGroupedByDateAndTemplate> statisticsReader = null)
        {
            return new ChartStatisticsViewFactory(
                statisticsReader ?? Stub<IReadSideRepositoryReader<StatisticsGroupedByDateAndTemplate>>.WithNotEmptyValues);
        }
    }
}

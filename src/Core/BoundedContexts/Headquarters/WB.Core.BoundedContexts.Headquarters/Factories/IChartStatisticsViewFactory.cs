using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IChartStatisticsViewFactory
    {
        ChartStatisticsView Load(ChartStatisticsInputModel input);
        List<QuestionnaireVersionsComboboxViewItem> GetQuestionnaireListWithData();
    }
}

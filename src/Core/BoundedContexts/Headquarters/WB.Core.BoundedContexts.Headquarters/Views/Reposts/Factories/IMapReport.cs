using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface IMapReport
    {
        MapReportView Load(MapReportInputModel input);
    }
}
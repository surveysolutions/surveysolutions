using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class MapUsersInputModel : ListViewModelBase
    {
        public string SearchBy { get; set; }
        public string MapName { get; set; }

        public MapUsersInputModel(){}
    }
}

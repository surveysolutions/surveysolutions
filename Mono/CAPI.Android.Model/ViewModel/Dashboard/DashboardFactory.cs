using Main.Core.View;
using Main.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardFactory : IViewFactory<DashboardInput, DashboardModel>
    {

        private readonly IDenormalizerStorage<DashboardModel> _documentStorage;

        public DashboardFactory(IDenormalizerStorage<DashboardModel> documentStorage)
        {
            _documentStorage = documentStorage;
        }

        #region Implementation of IViewFactory<DashboardInput,DashboardModel>

        public DashboardModel Load(DashboardInput input)
        {
            return _documentStorage.GetByGuid(input.UserId);
           // return _documentStorage.Query().First();
        
        }

        #endregion
    }
}
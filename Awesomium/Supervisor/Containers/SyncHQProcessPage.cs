using Common.Utils;
using Browsing.Supervisor.Controls;
using global::Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    public partial class SyncHQProcessPage : Screen
    {

        #region Fields
        
        private ISettingsProvider clientSettings;

        #endregion

        public SyncHQProcessPage(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();
        }
    }
}

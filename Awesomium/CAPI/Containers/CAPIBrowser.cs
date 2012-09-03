using Awesomium.Windows.Forms;
using Browsing.Common.Containers;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIBrowser : Browser
    {
        public CAPIBrowser(WebControl webView, ScreenHolder holder)
            : base(webView, holder)
        {
            InitializeComponent();
        }
    }
}

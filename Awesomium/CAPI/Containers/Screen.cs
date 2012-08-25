using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Browsing.CAPI.Containers
{
    public class Screen : UserControl
    {
        public Screen(ScreenHolder holder)
        {
            this.holder = holder;
            this.holder.LoadedScreens.Add(this);
        }

        private readonly ScreenHolder holder;
        protected ScreenHolder Holder
        {
            get { return this.holder; }
        }
    }
}

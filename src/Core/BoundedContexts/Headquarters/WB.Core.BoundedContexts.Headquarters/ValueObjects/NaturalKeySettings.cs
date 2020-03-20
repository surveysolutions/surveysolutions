using System;
using System.Collections.Generic;
using System.Text;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects
{
    public class NaturalKeySettings : AppSetting
    {
        public int MaxValue { get; set; }
    }
}

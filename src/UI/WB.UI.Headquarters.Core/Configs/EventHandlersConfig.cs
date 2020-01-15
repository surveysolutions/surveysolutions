using System;

namespace WB.UI.Headquarters.Configs
{
    public class EventHandlersConfig
    {
        public string[] Disabled { get; set; } = Array.Empty<string>();

        public string[] IgnoredException { get; set; } = Array.Empty<string>();
    }
}

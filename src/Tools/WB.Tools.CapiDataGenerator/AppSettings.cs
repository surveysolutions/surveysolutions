using System;

namespace WB.Tools.CapiDataGenerator
{
    public class AppSettings
    {
        public bool IsSupervisorEvents;

        private static AppSettings _instance;
        public static AppSettings Instance
        {
            get { return _instance ?? (_instance = new AppSettings()); }
        }

        private AppSettings()
        {
            
        }
    }
}

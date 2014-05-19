using System;

namespace WB.Tools.CapiDataGenerator
{
    public class AppSettings
    {
        private AppSettings(){}

        private static AppSettings _instance;
        public static AppSettings Instance
        {
            get { return _instance ?? (_instance = new AppSettings()); }
        }

        public bool AreSupervisorEventsNowPublishing { get; set; }
      
        public GenerationMode CurrentMode { set; get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Tools.QuestionnaireDocumentsConverter
{
    public class CommandLineSettings
    {
        public CommandLineSettings(string[] args)
        {
            EventStoreDbName = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-url")
                {
                    DataBaseUrl = args[i + 1];
                    continue;
                }
                if (args[i] == "-events")
                {
                    EventStoreDbName = args[i + 1];
                    continue;
                }
                if (args[i] == "-views")
                {
                    ViewStoreDbName = args[i + 1];
                    continue;
                }
            }
        }
        public string DataBaseUrl { get; private set; }
        public string EventStoreDbName { get; private set; }
        public string ViewStoreDbName { get; private set; }
    }
}

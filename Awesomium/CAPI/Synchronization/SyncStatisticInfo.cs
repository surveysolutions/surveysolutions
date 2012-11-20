using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Browsing.CAPI.Synchronization
{
    public class SyncStatisticInfo
    {

        public SyncStatisticInfo()
        {
        }
        
        public SyncStatisticInfo(string interviewersName, int assignments, int approvedQuestionaries, int rejectQuestionaries, bool ifNew)
        {
            InterviewersName = interviewersName;
            Assignments = assignments;
            ApprovedQuestionaries = approvedQuestionaries;
            RejectQuestionaries = rejectQuestionaries;
            New = ifNew;

        }

        public string InterviewersName { get; private set; }
        public int Assignments { get; private set; }
        public int ApprovedQuestionaries { get; private set; }
        public int RejectQuestionaries { get; private set; }
        public bool New { get; private set; }

    }
}

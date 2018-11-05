using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Interview
{

    public class InterviewLevel
    {
        public InterviewLevel()
        {
            this.RosterRowTitles = new Dictionary<Guid, string>();
            this.QuestionsSearchCache = new Dictionary<Guid, InterviewEntity>();
            this.Variables = new Dictionary<Guid, object>();
            this.DisabledVariables = new HashSet<Guid>();
        }
        public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, RosterVector vector)
            : this()
        {
            this.RosterScope = scopeVector;
            this.RosterVector = vector;
        }

        public RosterVector RosterVector { get; set; }
        public Dictionary<Guid, string> RosterRowTitles { get; set; }
        public Dictionary<Guid, InterviewEntity> QuestionsSearchCache { get; set; }
        public Dictionary<Guid, object> Variables { get; set; }
        public HashSet<Guid> DisabledVariables { get; set; }

        public ValueVector<Guid> RosterScope { get; set; }

        public static string GetLevelKeyName(ValueVector<Guid> rosterScope, RosterVector rosterVector)
        {
            var keyParts = rosterScope.Select(x => x.FormatGuid()).ToList();
            if (rosterVector.Length == 0)
                keyParts.Add("#");
            else
            {
                keyParts.AddRange(rosterVector.Select(x => x.ToString()));
            }

            var levelKey = string.Join("-", keyParts);
            return levelKey;
        }
    }
}

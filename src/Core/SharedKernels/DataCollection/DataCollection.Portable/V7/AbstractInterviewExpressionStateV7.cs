using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection.V7
{
    public abstract class AbstractInterviewExpressionStateV7 : AbstractInterviewExpressionStateV6, IInterviewExpressionStateV7
    {
        public AbstractInterviewExpressionStateV7()
        {
        }

        #region methods using InterviewScopes should be overriden
        public new Dictionary<string, IExpressionExecutableV7> InterviewScopes = new Dictionary<string, IExpressionExecutableV7>();

        public AbstractInterviewExpressionStateV7(Dictionary<string, IExpressionExecutableV7> interviewScopes,
            Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
        {
            this.InterviewProperties = interviewProperties.Clone();

            var newScopes = interviewScopes.ToDictionary(interviewScope => interviewScope.Key, interviewScope => interviewScope.Value.CopyMembers(this.GetRosterInstances));

            var newSiblingRosters = siblingRosters
                .ToDictionary(
                    interviewScope => interviewScope.Key,
                    interviewScope => new List<string>(interviewScope.Value));


            foreach (var interviewScope in interviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[Util.GetRosterStringKey(parent.GetRosterKey())]);

                interviewScope.Value.SetInterviewProperties(this.InterviewProperties);
            }

            this.InterviewScopes = newScopes;
            this.SiblingRosters = newSiblingRosters;
        }

        public new IEnumerable<IExpressionExecutableV7> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
        {
            var siblingsKey = Util.GetSiblingsKey(rosterKey, scopeId);

            var siblingRosters = this.SiblingRosters.ContainsKey(siblingsKey)
                ? this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x])
                : null;

            return siblingRosters;
        }

        #endregion

        public List<LinkedQuestionFilterResult> ProcessLinkedQuestionFilters()
        {
            var result = new List<LinkedQuestionFilterResult>();
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                result.AddRange(interviewScopeKvpValue.ExecuteLinkedQuestionFilters());
            }
            return result;
        }

        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return Clone() as IInterviewExpressionStateV2;
        }
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone()
        {
            return Clone() as IInterviewExpressionStateV4;
        }

        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone()
        {
            return Clone() as IInterviewExpressionStateV5;
        }

        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone()
        {
            return Clone() as IInterviewExpressionStateV6;
        }

        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone()
        {
            return Clone() as IInterviewExpressionStateV7;
        }
    }
}
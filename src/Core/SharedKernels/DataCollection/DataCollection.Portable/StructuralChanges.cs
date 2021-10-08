using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class StructuralChanges
    {
        public void ClearAllChanges()
        {
            ChangedSingleQuestions.Clear();
            ChangedMultiQuestions.Clear();
            RemovedRosters.Clear();
        }

        public Dictionary<Identity, int?> ChangedSingleQuestions { get; } = new Dictionary<Identity, int?>();
        public Dictionary<Identity, int[]> ChangedMultiQuestions { get; } = new Dictionary<Identity, int[]>();
        public List<Identity> RemovedRosters { get; } = new List<Identity>();

        public void AddChangedSingleQuestion(Identity questionIdentity, int? newAnswer)
        {
            ChangedSingleQuestions.Add(questionIdentity, newAnswer);
        }

        public void AddChangedMultiQuestion(Identity questionIdentity, int[] newAnswer)
        {
            ChangedMultiQuestions.Add(questionIdentity, newAnswer);
        }

        public void AddRemovedRoster(Identity rosterIdentity)
        {
            RemovedRosters.Add(rosterIdentity);
        }

        public void AddRemovedRosters(IEnumerable<Identity> rosterIdentities)
        {
            RemovedRosters.AddRange(rosterIdentities);
        }
    }
}

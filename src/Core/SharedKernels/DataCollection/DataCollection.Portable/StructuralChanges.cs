using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class StructuralChanges
    {
        public void ClearAllChanges()
        {
            AnswerChangesForSingleQuestions.Clear();
            AnswerChangesForMultiQuestions.Clear();
            AnswerChangesForYesNoQuestions.Clear();
        }

        public Dictionary<Identity, int?> AnswerChangesForSingleQuestions { get; set; } = new Dictionary<Identity, int?>();
        public Dictionary<Identity, int[]> AnswerChangesForMultiQuestions { get; set; } = new Dictionary<Identity, int[]>();
        public Dictionary<Identity, YesNoAnswersOnly> AnswerChangesForYesNoQuestions { get; set; } = new Dictionary<Identity, YesNoAnswersOnly>();
    }
}
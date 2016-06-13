namespace WB.Core.SharedKernels.DataCollection
{
    public class AnswerAndStructureChangeNotifier
    {
        private readonly StructuralChanges structuralChanges;

        public AnswerAndStructureChangeNotifier(StructuralChanges structuralChanges)
        {
            this.structuralChanges = structuralChanges;
        }

        public void NotifySingleAnswerChange(Identity questionIdentity, int? newAnswer)
        {
            this.structuralChanges.AnswerChangesForSingleQuestions.Add(questionIdentity, newAnswer);
        }

        public void NotifyMultiAnswerChange(Identity questionIdentity, int[] newAnswer)
        {
            this.structuralChanges.AnswerChangesForMultiQuestions.Add(questionIdentity, newAnswer);
        }

        public void NotifyMultiYesNoAnswerChange(Identity questionIdentity, YesNoAnswersOnly newAnswer)
        {
            this.structuralChanges.AnswerChangesForYesNoQuestions.Add(questionIdentity, newAnswer);
        }
    }
}

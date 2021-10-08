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
            this.structuralChanges.AddChangedSingleQuestion(questionIdentity, newAnswer);
        }

        public void NotifyMultiAnswerChange(Identity questionIdentity, int[] newAnswer)
        {
            this.structuralChanges.AddChangedMultiQuestion(questionIdentity, newAnswer);
        }
    }
}

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerPicture : ScenarioAnswerCommand
    {
        public AnswerPicture(string variable, RosterVector rosterVector, string pictureFileName) : base(variable, rosterVector)
        {
            PictureFileName = pictureFileName;
        }

        public string PictureFileName { get;}
    }
}

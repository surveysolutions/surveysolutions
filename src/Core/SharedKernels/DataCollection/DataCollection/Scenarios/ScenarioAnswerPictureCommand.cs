namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerPictureCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerPictureCommand(string variable, RosterVector rosterVector, string pictureFileName) : base(variable, rosterVector)
        {
            PictureFileName = pictureFileName;
        }

        public string PictureFileName { get;}
    }
}

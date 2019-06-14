using System;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerAudio : ScenarioAnswerCommand
    {
        public AnswerAudio(string variable, RosterVector rosterVector, string fileName, TimeSpan length) : base(variable, rosterVector)
        {
            FileName = fileName;
            Length = length;
        }

        public string FileName { get; }

        public TimeSpan Length { get; }
    }
}

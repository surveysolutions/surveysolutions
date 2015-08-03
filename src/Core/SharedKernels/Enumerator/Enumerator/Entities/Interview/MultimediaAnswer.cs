using System;

using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities
{
    public class MultimediaAnswer : BaseInterviewAnswer
    {
        public string PictureFileName { get; private set; }

        public MultimediaAnswer() { }
        public MultimediaAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            this.PictureFileName = answer;
        }

        public override bool IsAnswered
        {
            get { return !this.PictureFileName.IsNullOrEmpty(); }
        }

        public override void RemoveAnswer()
        {
            this.PictureFileName = null;
        }
    }
}
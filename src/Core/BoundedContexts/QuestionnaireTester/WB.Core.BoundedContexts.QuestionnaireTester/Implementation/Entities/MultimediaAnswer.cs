using System;

using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
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
            this.IsAnswered = !this.PictureFileName.IsNullOrEmpty();
        }

        public override void RemoveAnswer()
        {
            this.IsAnswered = false;
            this.PictureFileName = null;
        }
    }
}
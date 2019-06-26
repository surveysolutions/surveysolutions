namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class TextListAnswer
    {
        public TextListAnswer(int code, string text)
        {
            Code = code;
            Text = text;
        }

        public int Code { get;  }

        public string Text { get;  }
    }
}
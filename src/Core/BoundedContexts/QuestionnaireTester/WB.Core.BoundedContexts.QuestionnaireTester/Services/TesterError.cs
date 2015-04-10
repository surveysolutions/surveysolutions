namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public class TesterError
    {
        public TesterError(ErrorCode code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public ErrorCode Code { get; private set; }
        public string Message { get; private set; }
    }
}
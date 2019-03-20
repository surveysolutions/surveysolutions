namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class PersonalizedWebInterviewEmail
    {
        public PersonalizedWebInterviewEmail(string subject, string messageHtml, string messageText)
        {
            Subject = subject;
            MessageText = messageText;
            MessageHtml = messageHtml;
        }

        public string Subject { get; private set;}

        public string MessageText { get; private set; }

        public string MessageHtml  { get; private set; }
    }
}

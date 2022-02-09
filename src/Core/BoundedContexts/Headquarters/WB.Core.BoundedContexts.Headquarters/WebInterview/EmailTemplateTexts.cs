namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public class EmailTemplateTexts
    {
        public static class InvitationTemplate
        {
            public static string Subject => "Invitation to take a part in %SURVEYNAME%";
            public static string Message => @"Welcome to %SURVEYNAME%!
 
Thank you for cooperation!";
            public static string PasswordDescription => "This interview is protected. Please use following password:";
            public static string LinkText => "START NEW INTERVIEW";
        }

        public static class ResumeTemplate
        {
            public static string Subject => "Invitation to take a part in %SURVEYNAME%";
            public static string Message => @"Welcome to %SURVEYNAME%!
 
Thank you for cooperation!";
            public static string PasswordDescription => "This interview is protected. Please use following password:";
            public static string LinkText => "CONTINUE INTERVIEW";
        }

        public static class Reminder_NoResponse
        {
            public static string Subject => "Reminder, don’t forget to take a part in %SURVEYNAME%";
            public static string Message => @"You are receiving this reminder because you haven’t started responding to %SURVEYNAME%! 

Thank you for cooperation!";
            public static string PasswordDescription => "This interview is protected. Please use following password:";
            public static string LinkText => "START NEW INTERVIEW";
        }

        public static class Reminder_PartialResponse
        {
            public static string Subject => "Reminder, please complete your response to %SURVEYNAME%";
            public static string Message => @"You are receiving this reminder because you have started responding to %SURVEYNAME%, but haven’t completed the process.
 
Please answer all applicable questions and click the ‘COMPLETE’ button to submit your responses.
 
Thank you for cooperation!";
            public static string PasswordDescription => "This interview is protected. Please use following password:";
            public static string LinkText => "CONTINUE INTERVIEW";
        }

        public static class RejectEmail
        {
            public static string Subject => "Your action is required in %SURVEYNAME%";
            public static string Message => @"Thank you for taking part in %SURVEYNAME%!
 
While processing your response our staff has found some issues, which you are hereby asked to review.
 
We would appreciate if you try addressing all issues marked in your response and click the ‘COMPLETE’ button to submit your response.
 
Thank you for cooperation!";
            public static string PasswordDescription => "This interview is protected. Please use following password:";
            public static string LinkText => "CONTINUE INTERVIEW";
        }

        public static class CompleteEmail
        {
            public static string Subject => "Thank you for taking part in %SURVEYNAME%!";
            public static string Message => @"This interview has been completed!";
            public static string PasswordDescription => null;
            public static string LinkText => null;
        }

    }
}

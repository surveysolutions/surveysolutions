using System;
using System.Text.RegularExpressions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewEmailTemplate
    {
        public string Subject { get; }
        public string MainText { get; }
        public string PasswordDescription { get; }
        public string LinkText { get; }

        public WebInterviewEmailTemplate(string subject, string mainText, string passwordDescription, string linkText)
        {
            this.Subject = subject;
            this.MainText = mainText;
            this.PasswordDescription = passwordDescription;
            this.LinkText = linkText;
        }
    }

    public class EmailContent
    {
        private const string Password = "%PASSWORD%";
        private const string SurveyLink = "%SURVEYLINK%";
        private const string SurveyName = "%SURVEYNAME%";
        private const string QuestionnaireTitle = "%QUESTIONNAIRE%";

        public EmailContent(WebInterviewEmailTemplate template, string questionnaireTitle, string link, string password)
        {
            Subject = template.Subject
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            MainText = template.MainText
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle)
                .Replace(SurveyLink, link)
                .Replace(Password, password);
            LinkText = template.LinkText?
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            PasswordDescription = template.PasswordDescription?
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
        }

        public string Subject { get; private set; }
        public string MainText { get; private set; }
        public string PasswordDescription { get; }
        public string LinkText { get; }

        public void RenderInterviewData(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            Subject = ReplaceVariablesWithData(Subject, interview, questionnaire);
            MainText = ReplaceVariablesWithData(MainText, interview, questionnaire);
        }

        private static Regex FindVariables = new Regex("%[(A-Za-z0-9_)+]%", RegexOptions.Compiled);
        private string ReplaceVariablesWithData(string text, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            return FindVariables.Replace(text, match =>
            {
                var questionId = questionnaire.GetQuestionIdByVariable(match.Value.Trim('%'));
                if (!questionId.HasValue)
                    return String.Empty;
                var answer = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));
                return answer;
            });
        }
    }
}



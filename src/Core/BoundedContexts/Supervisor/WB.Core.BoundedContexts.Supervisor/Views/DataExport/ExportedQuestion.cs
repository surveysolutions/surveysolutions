using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedQuestion
    {
        public ExportedQuestion(InterviewQuestion question, ExportedHeaderItem header)
        {
            QuestionId = question.Id;

            Answers = GetAnswers(question, header);

            if (Answers.Length != header.ColumnNames.Length)
                throw new InvalidOperationException(
                    string.Format(
                        "something wrong with export logic, answer's count is less then required by template. Was '{0}', expected '{1}'",
                        Answers.Length, header.ColumnNames.Length));
        }

        public Guid QuestionId { get; private set; }
        public string[] Answers { get; private set; }

        private string[] GetAnswers(InterviewQuestion question, ExportedHeaderItem header)
        {
            if (question.Answer == null || !question.Enabled)
                return header.ColumnNames.Select(c => string.Empty).ToArray();

            if (header.ColumnNames.Length == 1)
                return new string[] { AnswerToStringValue(question.Answer) };

            var listOfAnswers = TryCastToEnumerable(question.Answer);
            if (listOfAnswers != null)
                return this.BuildAnswerListForQuestionByHeader(listOfAnswers.ToArray(), header);

            return new string[0];
        }

        private IEnumerable<object> TryCastToEnumerable(object value)
        {
            var arrayOfDecimal = value as IEnumerable<decimal>;
            if (arrayOfDecimal != null)
                return arrayOfDecimal.Select(d => (object) d);

            var arrayOfInteger = value as IEnumerable<int>;
            if (arrayOfInteger != null)
                return arrayOfInteger.Select(i => (object) i);

            var listOfAnswers = value as IEnumerable<object>;
            if (listOfAnswers != null)
                return listOfAnswers;
            return null;
        }

        private string AnswerToStringValue(object answer)
        {
            const string DefaultDelimiter = ",";

            var arrayOfObject = TryCastToEnumerable(answer);
            if (arrayOfObject != null)
                return string.Join(DefaultDelimiter, arrayOfObject);

            return answer.ToString();
        }

        private string[] BuildAnswerListForQuestionByHeader(object[] answers, ExportedHeaderItem header)
        {
            var result = new string[header.ColumnNames.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = answers.Length > i ? AnswerToStringValue(answers[i]) : string.Empty;
            }

            return result;
        }
    }
}

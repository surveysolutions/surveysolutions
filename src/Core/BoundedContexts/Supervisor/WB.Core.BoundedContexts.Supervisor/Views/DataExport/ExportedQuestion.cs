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
            if (question.Answer == null)
                return header.ColumnNames.Select(c => string.Empty).ToArray();

            if (header.ColumnNames.Length == 1)
                return new string[] { AnswerToStringValue(question.Answer) };

            var listOfAnswers = question.Answer as IEnumerable<object>;
            if (listOfAnswers != null)
                return this.BuildAnswerListForQuestionByHeader(listOfAnswers.ToArray(), header);

            return new string[0];
        }

        private string AnswerToStringValue(object answer)
        {
            const string DefaultDelimiter = ",";

            var arrayOfDecimal = answer as decimal[];
            if (arrayOfDecimal != null)
                return string.Join(DefaultDelimiter, arrayOfDecimal);

            var arrayOfInteger = answer as int[];
            if (arrayOfInteger != null)
                return string.Join(DefaultDelimiter, arrayOfInteger);

            var listOfAnswers = answer as IEnumerable<object>;
            if (listOfAnswers != null)
                return string.Join(DefaultDelimiter, listOfAnswers);

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

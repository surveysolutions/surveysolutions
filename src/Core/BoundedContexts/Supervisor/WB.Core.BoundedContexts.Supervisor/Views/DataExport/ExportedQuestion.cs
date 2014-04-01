using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedQuestion
    {
        public ExportedQuestion()
        {
        }

        public ExportedQuestion(Guid questionId, string[] answers)
        {
            QuestionId = questionId;
            Answers = answers;
        }

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

        public Guid QuestionId { get; set; }
        public string[] Answers { get; set; }

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

            var interviewTextListAnswer = value as InterviewTextListAnswers;
            if (interviewTextListAnswer != null)
            {
                return interviewTextListAnswer.Answers.Select(a => a.Answer).ToArray();
            }

            return null;
        }

        private string AnswerToStringValue(object answer)
        {
            if (answer == null)
                return string.Empty;

            var arrayOfObject = TryCastToEnumerable(answer);
            if (arrayOfObject != null && arrayOfObject.Any())
                return arrayOfObject.Last().ToString();

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

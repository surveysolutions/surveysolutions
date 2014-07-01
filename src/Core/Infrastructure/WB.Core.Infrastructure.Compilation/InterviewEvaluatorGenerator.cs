using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.Infrastructure.Compilation
{
    public class InterviewEvaluatorGenerator : IEvaluatorGenerator {

        public string GenerateEvaluator(QuestionnaireDocument questionnaire)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var question in questionnaire.GetAllQuestions<IQuestion>())
            {
                builder.AppendLine(GenerateQuestion(question));
            }

            return string.Format(template, builder.ToString());
        }

        private string GenerateQuestion(IQuestion question)
        {
            // dummy
            return string.Format("int {0};", question.StataExportCaption);
        }

        private string template = "public class InterviewEvaluator : IInterviewEvaluator{ {0} }";
    }
}
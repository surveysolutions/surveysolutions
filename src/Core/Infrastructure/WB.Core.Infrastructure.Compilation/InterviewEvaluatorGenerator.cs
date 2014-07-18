using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.Infrastructure.Compilation
{
    public class InterviewEvaluatorGenerator : IEvaluatorGenerator {

        public InterviewEvaluatorGenerator(IDynamicCompiler interviewCompiler = null, ICodeGenerator codeGenerator = null)
        {
            this.interviewCompiler = interviewCompiler ?? new RoslynInterviewCompiler();
            this.codeGenerator = codeGenerator ?? new CodeGenerator();
        }

        private IDynamicCompiler interviewCompiler;
        private ICodeGenerator codeGenerator;

        public EmitResult GenerateEvaluator(QuestionnaireDocument questionnaire, out string generatedAssembly)
        {

            this.codeGenerator = new CodeGenerator();
            this.interviewCompiler = new RoslynInterviewCompiler();

            string genertedEvaluator = this.codeGenerator.Generate(questionnaire);

            return this.interviewCompiler.GenerateAssemblyAsString(questionnaire.PublicKey, genertedEvaluator, new string[] { }, out generatedAssembly);
        }
    }
}
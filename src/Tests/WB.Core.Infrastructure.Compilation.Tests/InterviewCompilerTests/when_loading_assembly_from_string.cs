using System;
using System.Reflection;
using Machine.Specifications;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.Infrastructure.BaseStructures;

namespace WB.Core.Infrastructure.Compilation.Tests.InterviewCompilerTests
{
    internal class when_loading_assembly_from_string
    {

        Establish context = () =>
        {
            compiler = new InterviewCompiler();
            emitResult = compiler.GenerateAssemblyAsString(id, testClass, out resultAssembly);
            if (emitResult.Success == true && !string.IsNullOrEmpty(resultAssembly))
            {
                var compiledAssembly = Assembly.Load(Convert.FromBase64String(resultAssembly));
                Type calculator = compiledAssembly.GetType("InterviewEvaluator");
                evaluator =  Activator.CreateInstance(calculator) as IInterviewEvaluator;
            }
        };

        private Because of = () =>
            evaluationResult = evaluator.Test();


        private It should_result_succeded = () =>
            evaluationResult.ShouldEqual(42);

        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;
        private static IInterviewEvaluator evaluator;

        private static int evaluationResult; 

        public static string testClass = @"
            public class InterviewEvaluator : IInterviewEvaluator
            {
                public static object Evaluate()
                {
                    return 2+2*2;
                }

                public int Test()
                {
                    return 40 + 2;
                }
 
            }";
    }
}

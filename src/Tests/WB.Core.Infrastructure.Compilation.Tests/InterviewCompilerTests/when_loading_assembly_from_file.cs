using System;
using System.IO;
using System.Reflection;
using Machine.Specifications;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.SharedKernels.ExpressionProcessing;

namespace WB.Core.Infrastructure.Compilation.Tests.InterviewCompilerTests
{
    internal class when_loading_assembly_from_file
    {
        private Establish context = () =>
        {
            compiler = new RoslynInterviewCompiler();
            emitResult = compiler.GenerateAssemblyAsString(id, testClass, new string[] { },
                new string[] { }, out resultAssembly);

            filePath = Path.GetTempFileName();

            if (emitResult.Success == true && !string.IsNullOrEmpty(resultAssembly))
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

                var compiledAssembly = Assembly.LoadFrom(filePath);
                Type calculator = compiledAssembly.GetType("InterviewEvaluator");
                evaluator = Activator.CreateInstance(calculator) as IInterviewEvaluator;
            }
        };

        private Because of = () =>
            evaluationResult = evaluator.Test();

        private It should_result_succeded = () =>
            evaluationResult.ShouldEqual(42);

        Cleanup stuff = () =>
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        };

        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;
        private static IInterviewEvaluator evaluator;

        private static int evaluationResult;

        private static string filePath;

        public static string testClass =
            @"using System;
            using System.Collections.Generic;
            using System.Linq;
            using WB.Core.SharedKernels.ExpressionProcessing;
            public class InterviewEvaluator : IInterviewEvaluator
            {
                public static object Evaluate()
                {
                    return 2+2*2;
                }

                private List<int> values = new List<int>() {40, 2};

                public int Test()
                {
                    return values.Sum(i => i);
                }

                public List<Identity> CalculateValidationChanges()
                {
                    return new List<Identity>();
                }

                public List<Identity> CalculateConditionChanges()
                {
                    return new List<Identity>();
                }
 
            }";

    }
}

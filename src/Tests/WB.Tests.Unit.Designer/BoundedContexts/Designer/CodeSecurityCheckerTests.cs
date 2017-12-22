﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(CodeSecurityChecker))]
    [Localizable(false)]
    public class CodeSecurityCheckerTests
    {
        [Test]
        public void should_allow_use_of_simple_two_digits()
        {
            string code = string.Format(TestClassToCompile, "2 + 2");
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var codeSecurityChecker = GetCodeSecurityChecker();
            var compilation = CreateCompilation(syntaxTree.ToEnumerable());

            // Act
            var forbiddenClassesUsed = codeSecurityChecker.FindForbiddenClassesUsage(syntaxTree, compilation);

            Assert.That(forbiddenClassesUsed, Is.Empty);
        }

        [TestCase("Activator.CreateInstance(typeof(AccessViolationException))", "System.Activator")]
        [TestCase("AppContext.BaseDirectory", "System.AppContext")]
        [TestCase("AppDomain.GetCurrentThreadId()", "System.AppDomain")]
        [TestCase("Console.WriteLine(1)", "System.Console")]
        [TestCase("Environment.Exit(1)", "System.Environment")]
        [TestCase("GC.Collect()", "System.GC")]
        [TestCase("System.IO.File.ReadAllLines(\"\")", "System.IO.File")]
        [TestCase("new System.Net.Mail.SmtpClient()", "System.Net.Mail.SmtpClient")]
        public void should_not_allow_usage_of_dangerous_classess(string codeToCheck, string expectedClassName)
        {
            string code = string.Format(TestClassToCompile, codeToCheck);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var codeSecurityChecker = GetCodeSecurityChecker();
            var compilation = CreateCompilation(syntaxTree.ToEnumerable());
            // Act
            var forbiddenClassesUsed = codeSecurityChecker.FindForbiddenClassesUsage(syntaxTree, compilation);

            Assert.That(forbiddenClassesUsed, Has.Count.EqualTo(1));
            Assert.That(forbiddenClassesUsed[0], Is.EqualTo(expectedClassName));
        }

        private static CodeSecurityChecker GetCodeSecurityChecker()
        {
            return new CodeSecurityChecker();
        }

        static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees)
        {
            return CSharpCompilation.Create(
                "rules.dll",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    checkOverflow: true,
                    optimizationLevel: OptimizationLevel.Release,
                    warningLevel: 1,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default),
                syntaxTrees: syntaxTrees,
                references: Create.DynamicCompilerSettingsProvider().GetAssembliesToReference());
        }

        const string TestClassToCompile =
            @"  using System;
                using System.Collections.Generic;
                using System.Linq;
                using WB.Core.SharedKernels.DataCollection;
                public class InterviewEvaluator : IInterviewEvaluator
            {{
                public void CalculateConditionChanges()
                {{
                    {0};
                }}
            }}";
    }
}
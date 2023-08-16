﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(CodeSecurityChecker))]
    [Localizable(false)]
    public class CodeSecurityCheckerTests
    {
        [Test]
        public void should_not_crash_when_used_class_from_not_referenced_assembly()
        {
            string code = string.Format(TestClassToCompile, "Activator.CreateInstance<System.Net.Http.HttpClientFactory>().GetReturnUrl()");
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var codeSecurityChecker = GetCodeSecurityChecker();
            var compilation = CreateCompilation(syntaxTree.ToEnumerable());

            // Act
            Assert.DoesNotThrow(() => codeSecurityChecker.FindForbiddenClassesUsage(syntaxTree, compilation));
        }

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
        [TestCase("Environment.Exit(1)", "System.Environment")]
        [TestCase("GC.Collect()", "System.GC")]
        [TestCase("var t = new System.Threading.SemaphoreSlim(0, 3)", "System.Threading.SemaphoreSlim")]
        [TestCase("num2_float.Value.ToString(\"N\", new System.Globalization.CultureInfo(\"hi-IN\"))", "System.Globalization.CultureInfo")]
        public void should_not_allow_usage_of_dangerous_classes(string codeToCheck, string expectedClassName)
        {
            string code = string.Format(TestClassToCompile, codeToCheck);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var codeSecurityChecker = GetCodeSecurityChecker();
            var compilation = CreateCompilation(syntaxTree.ToEnumerable());
            
            // act
            var forbiddenClassesUsed = codeSecurityChecker.FindForbiddenClassesUsage(syntaxTree, compilation).ToList();

            Assert.That(forbiddenClassesUsed, Has.Count.EqualTo(1));
            Assert.That(forbiddenClassesUsed[0], Is.EqualTo(expectedClassName));
        }
        
        [TestCase("new System.Assembly")]
        [TestCase("new System.List()")]
        public void should_allow_usage_of_same_classes(string codeToCheck)
        {
            string code = string.Format(TestClassToCompile, codeToCheck);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var codeSecurityChecker = GetCodeSecurityChecker();
            var compilation = CreateCompilation(syntaxTree.ToEnumerable());
            
            // act
            var forbiddenClassesUsed = codeSecurityChecker.FindForbiddenClassesUsage(syntaxTree, compilation).ToList();

            Assert.That(forbiddenClassesUsed, Has.Count.EqualTo(0));
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

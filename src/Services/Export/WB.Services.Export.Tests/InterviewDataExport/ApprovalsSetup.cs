using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Namers.StackTraceParsers;
using NUnit.Framework;
using System.IO;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [SetUpFixture]
    public class ApprovalsSetup
    {
        [OneTimeSetUp]
        public void ApprovalsConfig()
        {
            ApprovalTests.Approvals.RegisterDefaultNamerCreation(() => new UnitTestFrameworkNamer());
        }
    }

    public class UnitTestFrameworkNamer : IApprovalNamer
    {
        private readonly StackTraceParser stackTraceParser;
        private string subdirectory;

        public UnitTestFrameworkNamer()
        {
            Approvals.SetCaller();
            stackTraceParser = new StackTraceParser();
            stackTraceParser.Parse(Approvals.CurrentCaller.StackTrace);
            HandleSubdirectory();
        }

        private void HandleSubdirectory()
        {
            var subdirectoryAttribute = Approvals.CurrentCaller.GetFirstFrameForAttribute<UseApprovalSubdirectoryAttribute>();
            if (subdirectoryAttribute != null)
            {
                subdirectory = subdirectoryAttribute.Subdirectory;
            }
        }

        public string Name
        {
            get
            {
                var name = this.stackTraceParser.ApprovalName;
                var dot = name.IndexOf('.');

                return name.Substring(dot + 1);
            }
        }

        public virtual string SourcePath => stackTraceParser.SourcePath + GetSubdirectory();

        public string GetSubdirectory()
        {
            if (string.IsNullOrEmpty(subdirectory))
            {
                return string.Empty;
            }
            return Path.DirectorySeparatorChar + subdirectory;
        }
    }
}

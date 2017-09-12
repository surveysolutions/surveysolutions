using System;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    [TestFixture()]
    public class ExportQuestion_Variable_Tests
    {
        [TestCase(789, VariableType.LongInteger, "789")]
        [TestCase(789.56, VariableType.Double, "789.56")]
        [TestCase(true, VariableType.Boolean, "True")]
        [TestCase("it is string", VariableType.String, "it is string")]
        public void when_export_variable(object variable, VariableType variableType, string exportResult)
        {
            ExportedVariableHeaderItem headerItem = new ExportedVariableHeaderItem()
            {
                VariableType = variableType,
            };

            var exportService = new ExportQuestionService();

            var exportedVariable = exportService.GetExportedVariable(variable, headerItem);

            Assert.AreEqual(exportedVariable.Length, 1);
            Assert.AreEqual(exportedVariable[0], exportResult);
        }

        [Test]
        public void when_export_datetime_variable()
        {
            DateTime dateTime = new DateTime(2017, 9, 12, 14, 09, 37);
            ExportedVariableHeaderItem headerItem = new ExportedVariableHeaderItem()
            {
                VariableType = VariableType.DateTime
            };

            var exportService = new ExportQuestionService();

            var exportedVariable = exportService.GetExportedVariable(dateTime, headerItem);

            Assert.AreEqual(exportedVariable.Length, 1);
            Assert.AreEqual(exportedVariable[0], "2017-09-12T14:09:37");
        }
    }
}
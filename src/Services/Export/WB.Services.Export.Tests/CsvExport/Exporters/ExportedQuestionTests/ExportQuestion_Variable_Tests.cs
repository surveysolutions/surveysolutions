using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    [TestFixture()]
    public class ExportQuestion_Variable_Tests
    {
        [TestCase(789L, VariableType.LongInteger, "789")]
        [TestCase(789.56, VariableType.Double, "789.56")]
        [TestCase(true, VariableType.Boolean, "1")]
        [TestCase("it is string", VariableType.String, "it is string")]
        public void when_export_variable(object variable, VariableType variableType, string exportResult)
        {
            ExportedVariableHeaderItem headerItem = new ExportedVariableHeaderItem()
            {
                VariableType = variableType,
            };

            var exportService = new ExportQuestionService();

            var exportedVariable = exportService.GetExportedVariable(variable, headerItem, false);

            Assert.AreEqual(exportedVariable.Length, 1);
            Assert.AreEqual(exportedVariable[0], exportResult);
        }

        [TestCase(789L, VariableType.LongInteger)]
        [TestCase(789.56, VariableType.Double)]
        [TestCase(true, VariableType.Boolean)]
        [TestCase("it is string", VariableType.String)]
        public void when_export_disable_variable(object variable, VariableType variableType)
        {
            ExportedVariableHeaderItem headerItem = new ExportedVariableHeaderItem()
            {
                ColumnHeaders = new List<HeaderColumn>() { new HeaderColumn(){Name = "column_name" } },
                VariableType = variableType,
            };

            var exportService = new ExportQuestionService();

            var exportedVariable = exportService.GetExportedVariable(variable, headerItem, true);

            Assert.AreEqual(exportedVariable.Length, 1);
            Assert.AreEqual(exportedVariable[0], ExportFormatSettings.DisableValue);
        }

        [Test]
        public void when_export_double_variable_with_decimal_type_value()
        {
            decimal variable = 789.56M;
;
            ExportedVariableHeaderItem headerItem = new ExportedVariableHeaderItem()
            {
                VariableType = VariableType.Double
            };

            var exportService = new ExportQuestionService();

            var exportedVariable = exportService.GetExportedVariable(variable, headerItem, false);

            Assert.AreEqual(exportedVariable.Length, 1);
            Assert.AreEqual(exportedVariable[0], "789.56");
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

            var exportedVariable = exportService.GetExportedVariable(dateTime, headerItem, false);

            Assert.AreEqual(exportedVariable.Length, 1);
            Assert.AreEqual(exportedVariable[0], "2017-09-12T14:09:37");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Moq.It;

namespace WB.Tests.Integration.SqlToTabDataExportServiceTests
{
    [Subject(typeof(SqlToTabDataExportService))]
    internal class SqlToTabDataExportServiceTestContext
    {
        protected static SqlToTabDataExportService CreateSqlToTabDataExportService(ISqlServiceFactory sqlServiceFactory = null, QuestionnaireExportStructure questionnaireExportStructure = null, ICsvWriterService csvWriterService=null)
        {
            var fileSystemAccessor = new FileSystemIOAccessor();
            sqlServiceFactory = sqlServiceFactory ?? Create.SqliteServiceFactoryForTests("sqllite_export_test", fileSystemAccessor);
            var sqlDataAccessor = new SqlDataAccessor(fileSystemAccessor);
            if (questionnaireExportStructure != null)
                new SqlDataExportWriter(sqlDataAccessor, sqlServiceFactory, fileSystemAccessor).CreateStructure(
                    questionnaireExportStructure, "");

            return new SqlToTabDataExportService(fileSystemAccessor, sqlServiceFactory,
                Mock.Of<ICsvWriterFactory>(_ => _.OpenCsvWriter(It.IsAny<Stream>(), It.IsAny<string>()) == csvWriterService), new SqlDataAccessor(fileSystemAccessor),
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(_ => _.GetById(It.IsAny<string>()) == questionnaireExportStructure));
        }


        protected static void RunCommand(ISqlServiceFactory sqlServiceFactory, string sql, object parameters = null)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(""))
            {
                sqlService.ExecuteCommand(sql, parameters);
            }
        }

        protected class CsvWriterServiceStub : ICsvWriterService
        {
            private readonly List<List<string>> writtenData = new List<List<string>>{new List<string>()};

            public void Dispose()
            {
            }

            public void WriteField<T>(T cellValue)
            {
                writtenData.Last().Add(cellValue.ToString());
            }

            public void NextRecord()
            {
                writtenData.Add(new List<string>());
            }

            public string[][] WrittenData { get { return writtenData.Select(d => d.ToArray()).ToArray(); } }
        }
    }
}

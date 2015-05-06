using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FilebasedExportedDataAccessorTests
{
    internal class when_export_all_data_and_archive_is_missing : FilebasedExportedDataAccessorTestContext
    {
        Establish context = () =>
        {
            filebasedExportedDataAccessor = CreateFilebasedExportedDataAccessor(
                dataFiles: new[] { "f1", "f2" },
                environmentFiles: new[] { "e1", "e2" },
                zipCallback: (f, d) => addedFiles = f.ToArray());
        };

        Because of = () =>
            archiveName= filebasedExportedDataAccessor.GetFilePathToExportedCompressedData(questionnaireId,questionnaireVersion, ExportDataType.Tab);

        It should_archive_name_contain_questionnaire_id_and_version = () =>
            archiveName.ShouldContain("exported_data_11111111-1111-1111-1111-111111111111_3");

        It should_archive_contain_data_files_and_environment_files = () =>
            addedFiles.ShouldEqual(new[] { "f1", "f2", "e1", "e2" });

        private static FilebasedExportedDataAccessor filebasedExportedDataAccessor;

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion =3;

        private static string[] addedFiles;
        private static string archiveName;
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;

namespace WB.Services.Export.DescriptionGenerator
{
    internal class DescriptionGenerator : IDescriptionGenerator
    {
        private readonly IProductVersion productVersion;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public DescriptionGenerator(IProductVersion productVersion, IFileSystemAccessor fileSystemAccessor)
        {
            this.productVersion = productVersion;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void GenerateDescriptionFile(QuestionnaireExportStructure questionnaire, string basePath, string dataFilesExtension)
        {
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine(
                $"Exported from Survey Solutions Headquarters {this.productVersion} on {DateTime.Today:D}");

            foreach (var level in questionnaire.HeaderToLevelMap.Values)
            {
                string fileName = $"{level.LevelName}{dataFilesExtension}";
                var variables = level.HeaderItems.Values.Select(question => question.VariableName);

                descriptionBuilder.AppendLine();
                descriptionBuilder.AppendLine(fileName);
                descriptionBuilder.AppendLine(string.Join(", ", variables));
            }

            this.fileSystemAccessor.WriteAllText(
                Path.Combine(basePath, "export__readme.txt"),
                descriptionBuilder.ToString());
        }
    }
}
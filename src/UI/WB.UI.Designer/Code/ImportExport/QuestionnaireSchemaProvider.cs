using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WB.UI.Designer.Code.ImportExport
{
    public class QuestionnaireSchemaProvider : IQuestionnaireSchemaProvider
    {
        private static readonly Lazy<(string Schema, string Version)> Schema = new(ReadSchema);

        public string GetSchema() => Schema.Value.Schema;

        public string GetSchemaVersion() => Schema.Value.Version;

        private static (string Schema, string Version) ReadSchema()
        {
            var anchorType = typeof(QuestionnaireImportService);
            var resourceName = $"{anchorType.Namespace}.QuestionnaireSchema.json";

            using Stream? stream = anchorType.Assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException("Can't find json schema for questionnaire");

            using var reader = new StreamReader(stream);
            var schemaText = reader.ReadToEnd();

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(schemaText));
            var version = Convert.ToHexString(hash).Substring(0, 16).ToLowerInvariant();

            return (schemaText, version);
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class DoFile
    {
        public const string ContentFileNameExtension = "do";
        private readonly StringBuilder doContent = new StringBuilder();
        private static Regex CleanupRegex = new Regex(@"\t|\n|\r|`|'", RegexOptions.Compiled);

        public void AppendLine()
        {
            doContent.AppendLine();
        }

        public void BuildInsheet(string fileName)
        {
            AppendHeaderFromFile();
            doContent.AppendLine($"insheet using \"{fileName}\", tab case names");
        }

        private static string? doFileHeader = null;

        private void AppendHeaderFromFile()
        {
            if (string.IsNullOrEmpty(doFileHeader))
            {
                var resourceName = "WB.Services.Export.CsvExport.Exporters.DoFileHeader.txt";
                var assembly = Assembly.GetAssembly(typeof(DoFile));

                if (assembly != null)
                {
                    using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using StreamReader reader = new StreamReader(stream);
                        doFileHeader = reader.ReadToEnd();
                    }
                }
            }

            doContent.Append(doFileHeader);
        }

        public void AssignValuesToVariable(string variableName, string valueName)
        {
            doContent.AppendLine($"label values {variableName} {valueName}");
        }

        public void AppendLabelToVariableMatching(string variableName, string labelName)
        {
            doContent.AppendLine($"label variable {variableName} `\"{this.RemoveNotAllowedCharsAndDecode(labelName)}\"'");
        }

        public void AppendCaptionLabelToVariableMatching(string variableName, string labelName)
        {
            doContent.AppendLine($"capture label variable {variableName} `\"{this.RemoveNotAllowedCharsAndDecode(labelName)}\"'");
        }

        public void DefineLabel(string labelName, IEnumerable<VariableValueLabel> labels)
        {
            //stata allows only int values less 2,147,483,620 to be labeled
            //stata doesn't allow to declare empty dictionaries
            int limitValue = 2147483620;
            var localBuilder = new StringBuilder();
            bool hasValidValue = false;

            foreach (var label in labels)
            {
                if (decimal.TryParse(label.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value) && value < limitValue && (value % 1) == 0)
                {
                    localBuilder.Append($"{value:F0} `\"{this.RemoveNotAllowedCharsAndDecode(label.Label)}\"' ");
                    hasValidValue = true;
                }
                else
                {
                    localBuilder.Append($"/*{label.Value} `\"{this.RemoveNotAllowedCharsAndDecode(label.Label)}\"'*/ ");
                }
            }

            doContent.AppendFormat(hasValidValue ? "label define {0} " : "/*label define {0}*/ ", labelName);

            doContent.Append(localBuilder);

            doContent.AppendLine();
        }

        private string RemoveNotAllowedCharsAndDecode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var replaced = CleanupRegex.Replace(s, string.Empty);

            return WebUtility.HtmlDecode(replaced);
        }

        public override string ToString()
        {
            return doContent.ToString();
        }
    }
}

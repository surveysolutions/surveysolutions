using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WB.UI.Designer.Pdf
{
    public class PdfConvert
    {
        static PdfConvertEnvironment pdfConvertEnvironmentDefaults;

        public static PdfConvertEnvironment EnvironmentDefaults => pdfConvertEnvironmentDefaults ?? (pdfConvertEnvironmentDefaults = new PdfConvertEnvironment
        {
            TempFolderPath = Path.GetTempPath(),
            WkHtmlToPdfPath = Path.Combine(OSUtil.GetProgramFilesx86Path(), @"wkhtmltopdf\wkhtmltopdf.exe"),
            Timeout = 60000
        });
       
        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (environment == null)
                environment = EnvironmentDefaults;

            String outputPdfFilePath;
            bool delete;
            if (woutput.OutputFilePath != null)
            {
                outputPdfFilePath = woutput.OutputFilePath;
                delete = false;
            }
            else
            {
                outputPdfFilePath = Path.Combine(environment.TempFolderPath, String.Format("{0}.pdf", Guid.NewGuid()));
                delete = true;
            }

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException($"File '{environment.WkHtmlToPdfPath}' not found. Check if wkhtmltopdf application is installed.");

            StringBuilder paramsBuilder = new StringBuilder();
            paramsBuilder.Append("--page-size A4 ");
            paramsBuilder.Append("--margin-left 0 ");
            paramsBuilder.Append("--margin-right 0 ");
            paramsBuilder.Append("--margin-bottom 7 ");

            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
                paramsBuilder.Append("--margin-top 25 ");
                paramsBuilder.Append("--header-spacing 5 ");
            }
            else
            {
                paramsBuilder.Append("--margin-top 10 ");
            }

            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
            }
            else
            {
                if (!string.IsNullOrEmpty(document.PageNumbersFormat))
                {
                    paramsBuilder.AppendFormat("--footer-right \"{0}\" ", document.PageNumbersFormat);
                }
            }

            if (!string.IsNullOrEmpty(document.CoverUrl))
            {
                paramsBuilder.AppendFormat("cover \"{0}\" ", document.CoverUrl);
            }

            paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPdfFilePath);

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = paramsBuilder.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.Start();

                    using (Task<bool> processWaiter = Task.Factory.StartNew(() => process.WaitForExit(environment.Timeout)))
                    using (Task<string> outputReader = Task.Factory.StartNew((Func<object, string>)ReadStream, process.StandardOutput))
                    using (Task<string> errorReader = Task.Factory.StartNew((Func<object, string>)ReadStream, process.StandardError))
                    {
                        bool waitResult = processWaiter.Result;

                        if (!waitResult)
                        {
                            process.Kill();
                        }

                        bool sucsessfullyAwaited = Task.WaitAll(new Task[]{ outputReader, errorReader}, environment.Timeout);

                        if (!waitResult || !sucsessfullyAwaited)
                        {
                            throw new PdfConvertTimeoutException();
                        }

                        var standardOutput = outputReader.Result;
                        var standardError = errorReader.Result;

                        if (!File.Exists(outputPdfFilePath))
                        {
                            if (process.ExitCode != 0)
                            {
                                throw new PdfConvertException(
                                    $"Html to PDF conversion of '{document.Url}' failed. Exit code {process.ExitCode}. Wkhtmltopdf output: \r\n{standardOutput} \r\n Wkhtmltopdf errors: \r\n{standardError}");
                            }
                            throw new PdfConvertException(
                                $"Html to PDF conversion of '{document.Url}' failed. Reason: Output file '{outputPdfFilePath}' not found.");
                        }
                    }
                }
            }
            finally
            {
                if (delete && File.Exists(outputPdfFilePath))
                    File.Delete(outputPdfFilePath);
            }
        }

        private static string ReadStream(object streamReader)
        {
            string result = ((StreamReader)streamReader).ReadToEnd();

            return result;
        }
    }
}

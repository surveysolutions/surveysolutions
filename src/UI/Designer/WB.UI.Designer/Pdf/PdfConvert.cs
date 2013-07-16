using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;

namespace WB.UI.Designer.Pdf
{
    public class PdfConvert
    {
        static PdfConvertEnvironment _e;

        public static PdfConvertEnvironment Environment
        {
            get
            {
                if (_e == null)
                    _e = new PdfConvertEnvironment
                    {
                        TempFolderPath = Path.GetTempPath(),
                        WkHtmlToPdfPath = Path.Combine(OSUtil.GetProgramFilesx86Path(), @"wkhtmltopdf\wkhtmltopdf.exe"),
                        Timeout = 60000
                    };
                return _e;
            }
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output)
        {
            ConvertHtmlToPdf(document, null, output);
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (environment == null)
                environment = Environment;

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
                throw new PdfConvertException(String.Format("File '{0}' not found. Check if wkhtmltopdf application is installed.", environment.WkHtmlToPdfPath));

           

            StringBuilder paramsBuilder = new StringBuilder();
            paramsBuilder.Append("--page-size A4 ");
            //paramsBuilder.Append("--redirect-delay 0 "); not available in latest version
            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
                paramsBuilder.Append("--margin-top 25 ");
                paramsBuilder.Append("--header-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
                paramsBuilder.Append("--margin-bottom 25 ");
                paramsBuilder.Append("--footer-spacing 5 ");
            }

            if (!string.IsNullOrEmpty(document.PageNumbersFormat))
            {
                paramsBuilder.AppendFormat("--header-right \"{0}\" ", document.PageNumbersFormat);
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
                    process.StartInfo.CreateNoWindow = !environment.Debug;
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = paramsBuilder.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = !environment.Debug;
                    process.StartInfo.RedirectStandardOutput = !environment.Debug;

                    var output = new StringBuilder();
                    var error = new StringBuilder();

                    using (var outputWaitHandle = new AutoResetEvent(false)) // solution taken from http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why because previously it was locked
                    using (var errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        };

                        process.Start();

                        if (process.StartInfo.RedirectStandardError)
                        {
                            process.BeginOutputReadLine();
                        }
                        if (process.StartInfo.RedirectStandardOutput)
                        {
                            process.BeginErrorReadLine();
                        }

                        if (process.WaitForExit(environment.Timeout) && outputWaitHandle.WaitOne(environment.Timeout) && errorWaitHandle.WaitOne(environment.Timeout))
                        {
                            if (!File.Exists(outputPdfFilePath))
                            {
                                if (process.ExitCode != 0)
                                {
                                    throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Exit code {1}. Wkhtmltopdf output: \r\n{2}", document.Url, process.ExitCode, error));
                                }
                                throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Reason: Output file '{1}' not found.", document.Url, outputPdfFilePath));
                            }


                            if (woutput.OutputStream != null)
                            {
                                using (Stream fs = new FileStream(outputPdfFilePath, FileMode.Open))
                                {
                                    byte[] buffer = new byte[32 * 1024];
                                    int read;

                                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        woutput.OutputStream.Write(buffer, 0, read);
                                    }
                                }

                                if (woutput.OutputCallback != null)
                                {
                                    woutput.OutputCallback(document, File.ReadAllBytes(outputPdfFilePath));
                                }
                            }
                        }
                        else
                        {
                            throw new PdfConvertTimeoutException();
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
    }
}

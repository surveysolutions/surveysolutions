// -----------------------------------------------------------------------
// <copyright file="SuccessMarker.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Web.Hosting;
using Main.Core;
using Ninject;

namespace Questionnaire.Core.Web.Helpers
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class SuccessMarker
    {
        private static string SuccessFilePath
        {
            get { return HostingEnvironment.MapPath("/App_Data/" + successMarker); }
        }

        private static readonly string successMarker = "success.marker";

        public static void Stop()
        {/*
            File.CreateText(SuccessMarker.SuccessFilePath);
            // Get file info
            FileInfo myFile = new FileInfo(SuccessMarker.SuccessFilePath);

            // Remove the hidden attribute of the file
            myFile.Attributes |= FileAttributes.Hidden;*/
        }
        public static void Start(IKernel kernel)
        {
           /* if (!File.Exists(SuccessMarker.SuccessFilePath))
            {*/
                NCQRSInit.RebuildReadLayer(kernel);
         /*   }
            else
                File.Delete(SuccessMarker.SuccessFilePath);*/
        }
    }
}
